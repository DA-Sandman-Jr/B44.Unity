#nullable enable

using System.Collections.Generic;
using System.IO;
using B44.Common.Diagnostics;
using B44.Common.Persistence;
using B44.Unity.Diagnostics;
using B44.Unity.Persistence;
using UnityEngine;

namespace B44.Unity.Proving
{
    /// <summary>
    /// Runs one pass across the B44.Unity boundary from Unity's own component
    /// lifecycle, and records what it observed. It judges nothing: the Test
    /// Framework decides pass and fail, so there is no second verdict protocol
    /// here to disagree with it.
    /// </summary>
    /// <remarks>
    /// This is the game side of the proof, and it is deliberately a
    /// <see cref="MonoBehaviour"/> rather than a plain class called from a test.
    /// A static method invoked directly would demonstrate that the assemblies
    /// link, which the compile project already covers; running from
    /// <c>Awake</c> in a live player loop is what demonstrates the boundary
    /// works in an assembled Unity application.
    /// </remarks>
    public sealed class B44BoundaryProbe : MonoBehaviour
    {
        /// <summary>The game declares its own categories; B44.Common ships none.</summary>
        public static readonly LogCategory Proving = new LogCategory("B44Proving");

        private readonly List<string> _routedToUnity = new List<string>();

        /// <summary>Formatted events that reached the sink, in order.</summary>
        public IReadOnlyList<string> RoutedToUnity
        {
            get { return _routedToUnity; }
        }

        /// <summary>Where <see cref="UnitySavePaths"/> put the save.</summary>
        public string SavePath { get; private set; } = string.Empty;

        /// <summary>
        /// True when the below-threshold event was dropped by B44.Common's own
        /// verbosity rules rather than reaching the sink.
        /// </summary>
        public bool VerbositySuppressedTheDebugEvent { get; private set; }

        /// <summary>
        /// Whether a file actually appeared at <see cref="SavePath"/> after the
        /// save.
        /// </summary>
        /// <remarks>
        /// The load-bearing observation. <c>CreateWithFallback</c> falls back to
        /// an in-memory store when the file store cannot be built — by design —
        /// and an in-memory store round-trips perfectly. Without checking the
        /// disk, a run in which B44's persistence never worked under Unity at
        /// all would satisfy every other assertion here.
        /// </remarks>
        public bool SaveReachedDisk { get; private set; }

        /// <summary>What the file-backed store returned after a save.</summary>
        public ProvingState? Reloaded { get; private set; }

        private void Awake()
        {
            StructuredGameLogger logger = new StructuredGameLogger(Capture);

            // Debug sits below the default Info threshold, so a correct
            // implementation drops this one. Asserting on that is what separates
            // "B44's logger ran" from "something forwarded a string to Unity".
            logger.Log(Proving, LogSeverity.Debug, "below_default_verbosity");
            VerbositySuppressedTheDebugEvent = _routedToUnity.Count == 0;

            // Unity environment facts going the other way, as plain values. The
            // capability receiving them has no way to tell they came from an
            // engine.
            logger.Log(Proving, LogSeverity.Info, "boundary_crossed", new Dictionary<string, object?>
            {
                { "engine", "Unity" },
                { "runtime", Application.unityVersion },
            });

            SavePath = UnitySavePaths.Resolve(Path.Combine("b44-proving", "state.json"));

            IRepository<ProvingState> repository = RepositoryFactory.CreateWithFallback(
                () => new AtomicJsonFileStore<ProvingState>(SavePath),
                UnreadableSavePolicy.Reset,
                UnityLoggerFactory.LogWarning);

            // Start from nothing so a leftover file from an earlier run cannot
            // make a broken save look like a working one.
            repository.Clear();
            repository.Save(new ProvingState { Chapter = "boundary", Attempts = 1 });
            SaveReachedDisk = File.Exists(SavePath);
            Reloaded = repository.Load();
            repository.Clear();
        }

        /// <summary>
        /// Tees each event into <see cref="RoutedToUnity"/> and then hands it to
        /// the package's routing. Composing a sink like this is the reason
        /// <see cref="UnityLoggerFactory.WriteToUnity"/> is exposed separately
        /// from <see cref="UnityLoggerFactory.CreateWithUnitySink"/>.
        /// </summary>
        private void Capture(StructuredLogEvent logEvent, string formatted)
        {
            _routedToUnity.Add(formatted);
            UnityLoggerFactory.WriteToUnity(logEvent, formatted);
        }
    }
}
