using System;
using TheFall.Presentation.UI;

namespace TheFall.Presentation.Scenes
{
    public enum FirstPlayableSceneKind
    {
        Login,
        Hub,
        Match,
    }

    /// <summary>
    /// Defines the mutually exclusive presentation-scene boundary for the first playable.
    /// Bootstrap and its application state remain persistent across these scenes.
    /// </summary>
    public static class FirstPlayableSceneContract
    {
        public const string LoginSceneName = "Login";
        public const string HubSceneName = "Hub";
        public const string MatchSceneName = "Match";

        public static string SceneName(FirstPlayableSceneKind sceneKind)
        {
            switch (sceneKind)
            {
                case FirstPlayableSceneKind.Login:
                    return LoginSceneName;
                case FirstPlayableSceneKind.Hub:
                    return HubSceneName;
                case FirstPlayableSceneKind.Match:
                    return MatchSceneName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneKind));
            }
        }

        public static FirstPlayableSceneKind SceneForScreen(FirstPlayableScreenKind screenKind)
        {
            switch (screenKind)
            {
                case FirstPlayableScreenKind.Login:
                    return FirstPlayableSceneKind.Login;
                case FirstPlayableScreenKind.Hub:
                case FirstPlayableScreenKind.Setup:
                    return FirstPlayableSceneKind.Hub;
                case FirstPlayableScreenKind.Loading:
                case FirstPlayableScreenKind.Match:
                case FirstPlayableScreenKind.Result:
                    return FirstPlayableSceneKind.Match;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenKind));
            }
        }

        public static bool Supports(
            FirstPlayableSceneKind sceneKind,
            FirstPlayableScreenKind screenKind)
        {
            return SceneForScreen(screenKind) == sceneKind;
        }
    }
}
