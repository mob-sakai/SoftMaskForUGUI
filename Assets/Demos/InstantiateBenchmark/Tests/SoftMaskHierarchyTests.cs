using System.Collections;
using Coffee.UISoftMask;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SoftMaskHierarchyTests
{
    private GameObject _canvas;
    private SoftMask _mask;
    private bool _automatic;

    [SetUp]
    public void SetUp()
    {
        _automatic = UISoftMaskProjectSettings.addSoftMaskableAutomatically;
        UISoftMaskProjectSettings.addSoftMaskableAutomatically = true;
        _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        var root = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(_canvas.transform, false);
        _mask = root.AddComponent<SoftMask>();
        Canvas.ForceUpdateCanvases();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_canvas);
        UISoftMaskProjectSettings.addSoftMaskableAutomatically = _automatic;
        Canvas.ForceUpdateCanvases();
    }

    [UnityTest]
    public IEnumerator AddedSubtreesAreUpdatedBeforeCanvasRebuild()
    {
        var first = CreateChild(_mask.transform);
        var grandchild = CreateChild(first.transform);
        var second = CreateChild(_mask.transform);

        Assert.IsNull(first.GetComponent<SoftMaskable>());
        Canvas.ForceUpdateCanvases();
        Assert.IsNotNull(first.GetComponent<SoftMaskable>());
        Assert.IsNotNull(grandchild.GetComponent<SoftMaskable>());
        Assert.IsNotNull(second.GetComponent<SoftMaskable>());
        Assert.IsFalse(first.GetComponent<SoftMaskable>().ignored);

        var nested = CreateChild(first.transform);
        Canvas.ForceUpdateCanvases();
        Assert.IsNotNull(nested.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator DisabledMaskStillTracksThroughEnabledSoftMaskable()
    {
        _mask.enabled = false;
        var child = CreateChild(_mask.transform);
        Canvas.ForceUpdateCanvases();
        Assert.IsNotNull(child.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator DisabledComponentsDoNotProcessPendingChanges()
    {
        var tracker = _mask.GetComponent<SoftMaskable>();
        var child = CreateChild(_mask.transform);
        _mask.enabled = false;
        tracker.enabled = false;
        Canvas.ForceUpdateCanvases();
        Assert.IsNull(child.GetComponent<SoftMaskable>());

        _mask.enabled = true;
        Assert.IsNotNull(child.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator ManualModeDoesNotProcessPendingChanges()
    {
        var child = CreateChild(_mask.transform);
        UISoftMaskProjectSettings.addSoftMaskableAutomatically = false;
        Canvas.ForceUpdateCanvases();
        Assert.IsNull(child.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator RemovedChildIsNotUpdatedByFormerParent()
    {
        var child = CreateChild(_mask.transform);
        child.transform.SetParent(_canvas.transform, false);
        Canvas.ForceUpdateCanvases();
        Assert.IsNull(child.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator InactiveSubtreeIsUpdatedWhenActivated()
    {
        var child = CreateChild(_mask.transform);
        child.SetActive(false);
        var grandchild = CreateChild(child.transform);
        Canvas.ForceUpdateCanvases();
        Assert.IsNotNull(child.GetComponent<SoftMaskable>());
        Assert.IsNull(grandchild.GetComponent<SoftMaskable>());

        child.SetActive(true);
        Assert.IsNotNull(grandchild.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator ChangingMaskModeCancelsThePendingMaskUpdate()
    {
        _mask.GetComponent<SoftMaskable>().enabled = false;
        var child = CreateChild(_mask.transform);
        _mask.maskingMode = SoftMask.MaskingMode.Normal;
        Canvas.ForceUpdateCanvases();
        Assert.IsNull(child.GetComponent<SoftMaskable>());

        _mask.maskingMode = SoftMask.MaskingMode.SoftMasking;
        Assert.IsNotNull(child.GetComponent<SoftMaskable>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator PreviouslyExistingComponentsArePreserved()
    {
        var child = CreateChild(_mask.transform);
        var existing = child.AddComponent<SoftMaskable>();
        existing.ignoreSelf = true;
        var grandchild = CreateChild(child.transform);
        Canvas.ForceUpdateCanvases();
        Assert.AreSame(existing, child.GetComponent<SoftMaskable>());
        Assert.IsTrue(existing.ignoreSelf);
        Assert.IsNotNull(grandchild.GetComponent<SoftMaskable>());
        yield return null;
    }

    private static GameObject CreateChild(Transform parent)
    {
        var child = new GameObject("Child", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(parent, false);
        return child;
    }
}
