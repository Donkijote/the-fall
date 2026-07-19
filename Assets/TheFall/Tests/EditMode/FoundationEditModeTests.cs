using System.Linq;
using NUnit.Framework;
using TheFall.Application.Input;
using TheFall.Domain;
using UnityEngine.InputSystem;

namespace TheFall.Tests.EditMode
{
    public sealed class FoundationEditModeTests
    {
        [Test]
        public void DomainAssembly_DoesNotReferenceUnityEngine()
        {
            var references = typeof(DomainAssembly).Assembly.GetReferencedAssemblies();

            Assert.That(references.Any(reference => reference.Name.StartsWith("UnityEngine")), Is.False);
        }

        [Test]
        public void ProjectInput_ContainsEverySharedIntentAction()
        {
            Assert.That(InputSystem.actions, Is.Not.Null);

            foreach (PlayerIntentKind intent in System.Enum.GetValues(typeof(PlayerIntentKind)))
            {
                Assert.That(InputSystem.actions.FindAction($"Gameplay/{intent}"), Is.Not.Null);
            }
        }
    }
}
