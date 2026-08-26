#nullable enable

using B44.Common.Diagnostics;

namespace B44.Unity.Diagnostics
{
    /// <summary>
    /// The three output channels Unity exposes through <c>UnityEngine.Debug</c>.
    /// </summary>
    /// <remarks>
    /// This is not an engine abstraction and is not meant to become one. It
    /// exists so the severity decision below is a value a test can assert on
    /// without a Unity runtime present, which is the only reason it is named at
    /// all. Nothing outside <see cref="UnityLoggerFactory"/> consumes it.
    /// </remarks>
    public enum UnityLogChannel
    {
        /// <summary><c>Debug.Log</c>.</summary>
        Log,

        /// <summary><c>Debug.LogWarning</c>.</summary>
        Warning,

        /// <summary><c>Debug.LogError</c>.</summary>
        Error,
    }

    /// <summary>
    /// Maps a B44 severity onto a Unity output channel. Deliberately free of
    /// Unity types so the rule is unit-testable on a machine with no editor
    /// installed — which is every machine this repository's CI runs on.
    /// </summary>
    public static class UnityLogRouting
    {
        /// <summary>
        /// Chooses the channel for <paramref name="severity"/>.
        /// </summary>
        /// <remarks>
        /// Threshold comparisons rather than exact matches, for the same reason
        /// B44.Godot's routing uses them: equality only behaves identically while
        /// <see cref="LogSeverity.Error"/> happens to be the highest severity, and
        /// stops being correct the moment a higher one is added.
        /// </remarks>
        public static UnityLogChannel ChannelFor(LogSeverity severity)
        {
            if (severity >= LogSeverity.Error)
            {
                return UnityLogChannel.Error;
            }

            return severity >= LogSeverity.Warning ? UnityLogChannel.Warning : UnityLogChannel.Log;
        }
    }
}
