#nullable enable

namespace B44.Unity.Proving
{
    /// <summary>
    /// The data the proving run hands to B44 and reads back out again.
    /// </summary>
    /// <remarks>
    /// Deliberately a plain class: no <c>UnityEngine</c> using, no
    /// <c>[Serializable]</c>, no <c>ScriptableObject</c>. That is the point being
    /// proved — a Unity game's data reaches an engine-free B44 capability, gets
    /// persisted and restored by it, and comes back, without the capability
    /// acquiring any knowledge of Unity. Public settable properties and a
    /// parameterless constructor are what <c>System.Text.Json</c> needs, and are
    /// the only shape constraint B44 imposes.
    /// </remarks>
    public sealed class ProvingState
    {
        public string Chapter { get; set; } = string.Empty;

        public int Attempts { get; set; }
    }
}
