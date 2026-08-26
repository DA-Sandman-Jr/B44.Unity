#nullable enable

using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace B44.Unity.Proving.Tests
{
    /// <summary>
    /// The only tests in this repository that prove the Unity integration works.
    /// They need a real editor; nothing in <c>B44.Unity.Tests</c> substitutes for
    /// them, and nothing there should be read as if it did.
    /// </summary>
    public sealed class B44BoundaryProbeTests
    {
        /// <summary>
        /// The shape B44.Common formats an event into. The field list is matched
        /// separately rather than pinned here, so the test does not depend on the
        /// order a dictionary happens to enumerate in.
        /// </summary>
        private const string BoundaryLinePattern =
            @"^\[B44Proving\] Info boundary_crossed correlationId=\S+ :: .+$";

        [UnityTest]
        public IEnumerator TheProbeCrossesTheBoundaryInAnAssembledPlayer()
        {
            // Registered before the component exists, because Awake runs during
            // AddComponent and the expectation has to be in place by then.
            LogAssert.Expect(LogType.Log, new Regex(BoundaryLinePattern));

            GameObject host = new GameObject(nameof(B44BoundaryProbe));
            try
            {
                B44BoundaryProbe probe = host.AddComponent<B44BoundaryProbe>();

                // One real frame of the player loop, so this is a running Unity
                // application and not a static method call wearing a component.
                yield return null;

                Assert.That(
                    probe.VerbositySuppressedTheDebugEvent,
                    Is.True,
                    "B44.Common's verbosity rules should have dropped the below-threshold event.");

                Assert.That(probe.RoutedToUnity, Has.Count.EqualTo(1));

                string line = probe.RoutedToUnity[0];
                Assert.That(line, Does.Match(BoundaryLinePattern));

                // The Unity environment facts went in as plain values and came
                // back inside a line B44.Common formatted.
                Assert.That(line, Does.Contain("engine=Unity"));
                Assert.That(line, Does.Contain("runtime=" + Application.unityVersion));

                Assert.That(
                    probe.SavePath,
                    Does.StartWith(Application.persistentDataPath),
                    "UnitySavePaths must resolve under Unity's persistent data directory.");

                Assert.That(
                    probe.SaveReachedDisk,
                    Is.True,
                    "Nothing was written to the save path. The repository fell back to memory, so B44's " +
                    "file-backed persistence did not actually run under Unity.");

                Assert.That(probe.Reloaded, Is.Not.Null, "The file-backed store returned nothing after a save.");
                Assert.That(probe.Reloaded!.Chapter, Is.EqualTo("boundary"));
                Assert.That(probe.Reloaded.Attempts, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }

            LogAssert.NoUnexpectedReceived();
        }
    }
}
