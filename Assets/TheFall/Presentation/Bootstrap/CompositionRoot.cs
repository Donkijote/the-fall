using TheFall.Presentation.Input;
using UnityEngine;

namespace TheFall.Presentation.Bootstrap
{
    /// <summary>
    /// Owns the application object graph. Dependencies are composed explicitly here as
    /// application and infrastructure services are introduced; no DI container is used.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InputIntentSource))]
    public sealed class CompositionRoot : MonoBehaviour
    {
        public bool IsComposed { get; private set; }

        private void Awake()
        {
            Compose();
            DontDestroyOnLoad(gameObject);
        }

        private void Compose()
        {
            if (IsComposed)
            {
                return;
            }

            GetComponent<InputIntentSource>().ValidateConfiguration();
            IsComposed = true;
        }
    }
}
