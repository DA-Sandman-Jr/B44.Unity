#nullable enable

using System;
using System.IO;
using UnityEngine;

namespace B44.Unity.Persistence
{
    /// <summary>
    /// Resolves a save location under Unity's per-user persistent data directory,
    /// as a plain path string that B44.Common's file-backed stores already accept.
    /// </summary>
    /// <remarks>
    /// B44.Common ships <c>SavePaths.ResolveAppData</c> for the common per-user
    /// case, and under Unity it is the wrong answer on every platform that is not
    /// desktop: it resolves <see cref="Environment.SpecialFolder.ApplicationData"/>,
    /// which on Android and iOS is not the app-scoped, writable, backed-up
    /// location the player's save belongs in — and can come back empty. Unity
    /// answers that question with <c>Application.persistentDataPath</c>, so
    /// converting that answer into the string B44 expects is exactly the kind of
    /// environment translation this package exists for.
    ///
    /// It deliberately stops at a path. Creating directories, writing, and the
    /// atomic-replace and backup rules all stay in B44.Common, which does them
    /// without an engine and is tested without one.
    /// </remarks>
    public static class UnitySavePaths
    {
        /// <summary>
        /// Resolves <paramref name="relativePath"/> under Unity's persistent data
        /// directory — for example <c>"saves/campaign.json"</c>.
        /// </summary>
        public static string Resolve(string relativePath)
        {
            return Combine(Application.persistentDataPath, relativePath);
        }

        /// <summary>
        /// The path rule on its own, with the persistent data directory passed in.
        /// </summary>
        /// <remarks>
        /// Split out from <see cref="Resolve"/> so the rule can be tested with no
        /// Unity runtime present. <see cref="Resolve"/> is then thin enough that
        /// reading it is the same as testing it.
        /// </remarks>
        /// <param name="persistentDataRoot">Normally <c>Application.persistentDataPath</c>.</param>
        /// <param name="relativePath">A relative path, which may contain directories.</param>
        public static string Combine(string persistentDataRoot, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(persistentDataRoot))
            {
                throw new ArgumentException(
                    "The persistent data directory must not be null or empty. Under Unity this is " +
                    "Application.persistentDataPath, which is only populated once the player has started.",
                    nameof(persistentDataRoot));
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("The save path must not be null or empty.", nameof(relativePath));
            }

            // Path.Combine silently discards the root when the second argument is
            // absolute, which would put a save somewhere the platform may not even
            // let the game write. Rejecting it names the mistake instead.
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException(
                    "The save path must be relative to the persistent data directory, not rooted.",
                    nameof(relativePath));
            }

            return Path.Combine(persistentDataRoot, relativePath);
        }
    }
}
