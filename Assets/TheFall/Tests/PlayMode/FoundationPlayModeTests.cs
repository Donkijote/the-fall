using System.Collections;
using NUnit.Framework;
using TheFall.Presentation.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class FoundationPlayModeTests
    {
        [UnityTest]
        public IEnumerator BootstrapScene_ComposesTheFoundation()
        {
            if (CompositionRoot.Instance != null)
            {
                Object.Destroy(CompositionRoot.Instance.gameObject);
                yield return null;
            }

            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);

            var deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != "Login" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            var compositionRoot = CompositionRoot.Instance;
            Assert.That(compositionRoot, Is.Not.Null);
            Assert.That(compositionRoot.IsComposed, Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Login"));
        }
    }
}
