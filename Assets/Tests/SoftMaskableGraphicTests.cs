using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Coffee.UISoftMask;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tests
{
    public class SoftMaskableGraphicTests
    {
        private const int k_ContainerCount = 64;
        private GameObject _canvasRoot;
        private SoftMask _mask;

        [SetUp]
        public void SetUp()
        {
            _canvasRoot = new GameObject("SoftMaskableGraphicTests", typeof(RectTransform), typeof(Canvas));
            _canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var maskGo = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(SoftMask));
            maskGo.transform.SetParent(_canvasRoot.transform, false);
            _mask = maskGo.GetComponent<SoftMask>();
            Canvas.ForceUpdateCanvases();
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasRoot) Object.DestroyImmediate(_canvasRoot);
        }

        private static int BeforeCanvasRebuildSubscriberCount()
        {
            var callbacks = typeof(SoftMask).Assembly.GetType("Coffee.UISoftMaskInternal.UIExtraCallbacks");
            Assert.That(callbacks, Is.Not.Null);
            var action = callbacks.GetField("s_BeforeCanvasRebuildAction", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null);
            Assert.That(action, Is.Not.Null);
            var delegates = action.GetType().BaseType
                ?.GetField("_delegates", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(action) as ICollection;
            Assert.That(delegates, Is.Not.Null);
            return delegates.Count;
        }

        private GameObject CreateContainer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_mask.transform, false);
            return go;
        }

        private static IEnumerator Rebuild()
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTest]
        public IEnumerator SoftMaskableWithoutGraphicDoesNotSubscribeEveryFrame()
        {
            var before = BeforeCanvasRebuildSubscriberCount();
            var containers = new List<GameObject>();
            for (var i = 0; i < k_ContainerCount; i++)
            {
                containers.Add(CreateContainer("Container" + i));
            }

            yield return Rebuild();

            foreach (var container in containers)
            {
                Assert.That(container.GetComponent<SoftMaskable>(), Is.Not.Null,
                    "SoftMask adds a SoftMaskable to every descendant");
            }

            Assert.That(BeforeCanvasRebuildSubscriberCount() - before, Is.Zero,
                "A SoftMaskable without a graphic must not register a per-frame callback");
        }

        [UnityTest]
        public IEnumerator GraphicAddedLaterIsMaskedAndOrderedAfterGraphic()
        {
            var container = CreateContainer("Container");
            yield return Rebuild();
            Assert.That(container.GetComponent<SoftMaskable>(), Is.Not.Null);

            var image = container.AddComponent<Image>();
            yield return Rebuild();

            Assert.That(image.materialForRendering, Is.Not.EqualTo(image.material),
                "A graphic added after the SoftMaskable must be soft-masked");

            var components = new List<Component>();
            container.GetComponents(components);
            var softMaskables = container.GetComponents<SoftMaskable>();
            Assert.That(softMaskables.Length, Is.EqualTo(1));
            Assert.That(components.IndexOf(softMaskables[0]), Is.GreaterThan(components.IndexOf(image)),
                "The SoftMaskable must come after the graphic");

            var before = BeforeCanvasRebuildSubscriberCount();
            yield return Rebuild();
            Assert.That(BeforeCanvasRebuildSubscriberCount(), Is.EqualTo(before),
                "The one-shot order check must unsubscribe");
        }
    }
}
