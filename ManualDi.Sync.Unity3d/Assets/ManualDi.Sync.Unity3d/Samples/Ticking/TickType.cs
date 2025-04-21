namespace ManualDi.Sync.Unity3d.Samples.Ticking
{
    /// <summary>
    /// Represents when a tick is going to be performed.
    /// </summary>
    public enum TickType
    {
        /// <summary>
        /// Executed just right before <see cref="Update"/>. Custom update not provided by Unity.
        /// </summary>
        PreUpdate,

        /// <summary>
        /// Executed by the Unity's default MonoBehaviour Update.
        /// </summary>
        Update,

        /// <summary>
        /// Executed by the Unity's default MonoBehaviour LateUpdate.
        /// </summary>
        LateUpdate,

        /// <summary>
        /// Executed by the Unity's default MonoBehaviour FixedUpdate.
        /// </summary>
        FixedUpdate,
    }
}