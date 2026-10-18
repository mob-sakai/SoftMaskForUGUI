using System;
using System.Collections;
using System.Reflection;
using Coffee.UISoftMask;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tests
{
    public class SoftMaskSubscriptionTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject _canvasRoot;
        private Transform _firstParent;
        private Transform _secondParent;
        private CanvasViewChangeTrigger _trigger;

        [SetUp]
        public void SetUp()
        {
            _canvasRoot = new GameObject("SoftMaskSubscriptionTests", typeof(RectTransform), typeof(Canvas));
            _canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _firstParent = CreateParent("First");
            _secondParent = CreateParent("Second");
            _trigger = CanvasViewChangeTrigger.Find(_firstParent);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasRoot) Object.DestroyImmediate(_canvasRoot);
        }

        private Transform CreateParent(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_canvasRoot.transform, false);
            return go.transform;
        }

        private SoftMask CreateMask()
        {
            var go = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SoftMask));
            var mask = go.GetComponent<SoftMask>();
            go.GetComponent<Image>().enabled = false;
            go.transform.SetParent(_firstParent, false);
            Canvas.ForceUpdateCanvases();
            return mask;
        }

        private int SubscriptionCount()
        {
            var field = typeof(CanvasViewChangeTrigger).GetField("onCanvasViewChanged", PrivateInstance);
            Assert.That(field, Is.Not.Null);
            return (field.GetValue(_trigger) as Delegate)?.GetInvocationList().Length ?? 0;
        }

        [UnityTest]
        public IEnumerator DisabledMasksReparentedUnderSameCanvasDoNotLeaveSubscriptions()
        {
            for (int i = 0; i < 32; i++)
            {
                var mask = CreateMask();
                Assert.That(SubscriptionCount(), Is.EqualTo(1), "Active mask should subscribe after reparenting");
                mask.gameObject.SetActive(false);
                Assert.That(SubscriptionCount(), Is.Zero, "OnDisable must unsubscribe");
                mask.transform.SetParent(_secondParent, false);
                Assert.That(SubscriptionCount(), Is.Zero, "Disabled reparenting must not subscribe");
                Object.Destroy(mask.gameObject);
            }
            yield return null;
            Assert.That(SubscriptionCount(), Is.Zero);
        }

        [UnityTest]
        public IEnumerator ReenabledMasksCanSubscribeAndUnsubscribeNormally()
        {
            var mask = CreateMask();
            mask.gameObject.SetActive(false);
            mask.transform.SetParent(_secondParent, false);
            mask.gameObject.SetActive(true);
            mask.transform.SetParent(_firstParent, false);
            Canvas.ForceUpdateCanvases();
            Assert.That(SubscriptionCount(), Is.EqualTo(1));
            mask.gameObject.SetActive(false);
            Assert.That(SubscriptionCount(), Is.Zero);
            Object.Destroy(mask.gameObject);
            yield return null;
            Assert.That(SubscriptionCount(), Is.Zero);
        }

        [UnityTest]
        public IEnumerator OnDestroyDetachesAnExistingSubscriptionOnADisabledMask()
        {
            var mask = CreateMask();
            mask.gameObject.SetActive(false);
            // Arrange the inconsistent state produced by the old disabled reparent callback.
            var callback = (Action)typeof(SoftMask).GetField("_onCanvasViewChanged", PrivateInstance).GetValue(mask);
            Assert.That(callback, Is.Not.Null);
            typeof(SoftMask).GetField("_viewChangeTrigger", PrivateInstance).SetValue(mask, _trigger);
            _trigger.onCanvasViewChanged += callback;
            Assert.That(SubscriptionCount(), Is.EqualTo(1));
            Object.Destroy(mask.gameObject);
            yield return null;
            Assert.That(SubscriptionCount(), Is.Zero, "OnDestroy must detach before clearing the delegate");
        }
    }
}
