using UnityEngine;

namespace TheFall.Presentation.Scenes
{
    /// <summary>
    /// Keeps a scene's foundation role visible in the Inspector without giving it gameplay behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScenePurpose : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        private string _description = string.Empty;

        public string Description => _description;

#if UNITY_EDITOR
        public void SetDescription(string description)
        {
            _description = description;
        }
#endif
    }
}
