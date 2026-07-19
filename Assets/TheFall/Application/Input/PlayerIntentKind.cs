namespace TheFall.Application.Input
{
    /// <summary>
    /// Platform-neutral input vocabulary established by the bootstrap.
    /// Gameplay behavior and intent payloads are intentionally deferred.
    /// </summary>
    public enum PlayerIntentKind
    {
        Point,
        Navigate,
        Inspect,
        Select,
        Confirm,
        Cancel,
    }
}
