using TheFall.Application.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFall.Presentation.Input
{
    /// <summary>
    /// Resolves platform controls to the shared intent-named actions. It does not decide
    /// whether an intent is legal or how gameplay resolves it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputIntentSource : MonoBehaviour
    {
        private const string GameplayMap = "Gameplay";

        public InputAction GetAction(PlayerIntentKind intent)
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                throw new MissingReferenceException("The project-wide The Fall input actions are not configured.");
            }

            return actions.FindAction($"{GameplayMap}/{intent}", true);
        }

        public void ValidateConfiguration()
        {
            foreach (PlayerIntentKind intent in System.Enum.GetValues(typeof(PlayerIntentKind)))
            {
                GetAction(intent);
            }
        }
    }
}
