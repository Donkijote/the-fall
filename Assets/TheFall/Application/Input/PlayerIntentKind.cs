namespace TheFall.Application.Input
{
    /// <summary>
    /// Project-wide Input System action names. Presentation adapters translate these controls
    /// into the platform-neutral card interaction intents owned by the application layer.
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
