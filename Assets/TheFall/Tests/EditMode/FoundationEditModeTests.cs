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

        [Test]
        public void ProjectInput_MapsTouchMouseAndKeyboardWithoutConflictingConfirmOrCancelBindings()
        {
            var inspect = InputSystem.actions.FindAction("Gameplay/Inspect");
            var select = InputSystem.actions.FindAction("Gameplay/Select");
            var confirm = InputSystem.actions.FindAction("Gameplay/Confirm");
            var cancel = InputSystem.actions.FindAction("Gameplay/Cancel");

            Assert.That(inspect.bindings.Any(binding =>
                binding.path == "<Touchscreen>/primaryTouch/press" &&
                binding.interactions.Contains("Hold")), Is.True);
            Assert.That(select.bindings.Any(binding =>
                binding.path == "<Touchscreen>/primaryTouch/press" &&
                binding.interactions.Contains("Tap")), Is.True);
            Assert.That(confirm.bindings.Any(binding => binding.path.Contains("Touchscreen")), Is.False);

            Assert.That(inspect.bindings.Any(binding => binding.path == "<Mouse>/rightButton"), Is.True);
            Assert.That(select.bindings.Any(binding => binding.path == "<Mouse>/leftButton"), Is.True);
            Assert.That(cancel.bindings.Any(binding => binding.path == "<Mouse>/rightButton"), Is.False);

            Assert.That(inspect.bindings.Any(binding => binding.path == "<Keyboard>/i"), Is.True);
            Assert.That(select.bindings.Any(binding => binding.path == "<Keyboard>/e"), Is.True);
            Assert.That(confirm.bindings.Any(binding => binding.path == "<Keyboard>/enter"), Is.True);
            Assert.That(cancel.bindings.Any(binding => binding.path == "<Keyboard>/escape"), Is.True);
        }
    }
}
