#nullable enable

using B44.Common.Diagnostics;
using UnityEngine;

namespace B44.Unity.Diagnostics
{
    /// <summary>
    /// Wires the engine-free <see cref="StructuredGameLogger"/> to Unity's output
    /// channels. Nothing below the engine boundary calls <c>UnityEngine.Debug</c>;
    /// this is the bridge.
    /// </summary>
    /// <remarks>
    /// The sibling of B44.Godot's <c>GodotLoggerFactory</c>. B44.Common documents
    /// that each host supplies its own sink factory and ships none itself, so the
    /// second occurrence this helper needs was already demonstrated by the Godot
    /// side; the only thing that differs here is which three methods the formatted
    /// line is handed to.
    /// </remarks>
    public static class UnityLoggerFactory
    {
        /// <summary>A logger whose sink is Unity's log, warning, and error channels.</summary>
        public static StructuredGameLogger CreateWithUnitySink()
        {
            return new StructuredGameLogger(WriteToUnity);
        }

        /// <summary>
        /// Routes one formatted event to Unity. Exposed separately so a game that
        /// composes its own sink — to tee events into a session report, say — can
        /// still use the standard routing instead of restating it.
        /// </summary>
        public static void WriteToUnity(StructuredLogEvent logEvent, string formatted)
        {
            switch (UnityLogRouting.ChannelFor(logEvent.Severity))
            {
                case UnityLogChannel.Error:
                    Debug.LogError(formatted);
                    break;
                case UnityLogChannel.Warning:
                    Debug.LogWarning(formatted);
                    break;
                default:
                    Debug.Log(formatted);
                    break;
            }
        }

        /// <summary>
        /// Warning sink for <c>RepositoryFactory.CreateWithFallback</c>, whose
        /// <c>onWarning</c> parameter is a plain <c>Action&lt;string&gt;</c>.
        /// Named here so games stop each writing the same lambda at their save
        /// wiring.
        /// </summary>
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
    }
}
