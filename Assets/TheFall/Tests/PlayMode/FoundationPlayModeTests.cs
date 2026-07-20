using System.Collections;
using NUnit.Framework;
using TheFall.Presentation.Bootstrap;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheFall.Tests.PlayMode
{
    public sealed class FoundationPlayModeTests
    {
        [UnityTest]
        public IEnumerator BootstrapScene_ComposesTheFoundation()
        {
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);

            var compositionRoot = CompositionRoot.Instance;
            Assert.That(compositionRoot, Is.Not.Null);
            Assert.That(compositionRoot.IsComposed, Is.True);
        }
    }
}
