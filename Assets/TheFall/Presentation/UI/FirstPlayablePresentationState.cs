namespace TheFall.Presentation.UI
{
    /// <summary>
    /// Keeps presentation-only choices alive while Bootstrap replaces Login, Hub, and Match scenes.
    /// Authoritative match state remains in FirstPlayableFlow.
    /// </summary>
    public sealed class FirstPlayablePresentationState
    {
        public bool HasEnteredGateway { get; set; }

        public bool CasasEnabled { get; set; } = true;

        public bool TrivilinImmediate { get; set; }

        public bool AnimationFastForwardEnabled { get; set; }

        public bool AnimationReducedMotionEnabled { get; set; }

        public bool AudioMasterEnabled { get; set; } = true;

        public bool AudioEffectsEnabled { get; set; } = true;

        public bool AudioMusicEnabled { get; set; }

        public string HomeChatChannel { get; set; } = "global";

        public string HomeChatUserMessageText { get; set; }
    }
}
