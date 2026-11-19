using System;
using System.Collections;
using System.Diagnostics;
using Coffee.UISoftMask;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public class InstantiateBenchmark : MonoBehaviour
{
    [SerializeField] private GameObject m_Template;
    [SerializeField] private RectTransform m_UnmaskedParent;
    [SerializeField] private SoftMask m_EnabledMask;
    [SerializeField] private SoftMask m_DisabledMask;
    [SerializeField] private int[] m_ItemCounts = { 70, 140, 280 };
    [SerializeField] private int m_Repetitions = 3;

    private IEnumerator Start()
    {
        if (!m_Template || !m_UnmaskedParent || !m_EnabledMask || !m_DisabledMask ||
            m_ItemCounts == null || m_ItemCounts.Length == 0 || m_Repetitions < 1)
        {
            Debug.LogError("The benchmark reference, number of elements, or number of attempts is invalid.");
            yield break;
        }

        yield return null;
        foreach (var count in m_ItemCounts)
        {
            if (count < 1)
            {
                Debug.LogError($"Please enter at least one element.: {count}");
                yield break;
            }

            yield return RunCase("Unmasked", m_UnmaskedParent, null, false, false, count);
            yield return RunCase("SoftMask enabled", m_EnabledMask.transform, m_EnabledMask, false, false, count);
            yield return RunCase("SoftMask disabled during creation", m_DisabledMask.transform, m_DisabledMask, true, false, count);
            yield return RunCase("SoftMask and root SoftMaskable disabled during creation",
                m_DisabledMask.transform, m_DisabledMask, true, true, count);
        }

        Debug.Log("[Issue246] Benchmark complete");
    }

    private IEnumerator RunCase(string label, Transform parent, SoftMask mask, bool enableAfterCreation,
        bool suspendRootSoftMaskable, int count)
    {
        var rootSoftMaskable = suspendRootSoftMaskable ? mask.GetComponent<SoftMaskable>() : null;
        if (suspendRootSoftMaskable && !rootSoftMaskable)
        {
            Debug.LogError("SoftMaskable is not included in the SoftMask comparison set.");
            yield break;
        }

        var creationTimes = new double[m_Repetitions];
        var enablingTimes = new double[m_Repetitions];
        var rebuildTimes = new double[m_Repetitions];
        var totalTimes = new double[m_Repetitions];

        for (var run = -1; run < m_Repetitions; run++)
        {
            if (mask && enableAfterCreation) mask.enabled = false;
            if (rootSoftMaskable) rootSoftMaskable.enabled = false;
            var items = new GameObject[count];
            var stopwatch = Stopwatch.StartNew();
            Profiler.BeginSample("Issue246.Instantiate");
            for (var i = 0; i < count; i++)
            {
                items[i] = Instantiate(m_Template, parent, false);
            }
            Profiler.EndSample();
            var creationMs = stopwatch.Elapsed.TotalMilliseconds;

            Profiler.BeginSample("Issue246.EnableSoftMask");
            if (mask && enableAfterCreation) mask.enabled = true;
            if (rootSoftMaskable) rootSoftMaskable.enabled = true;
            Profiler.EndSample();
            var enablingMs = stopwatch.Elapsed.TotalMilliseconds - creationMs;

            Profiler.BeginSample("Issue246.RebuildCanvases");
            Canvas.ForceUpdateCanvases();
            Profiler.EndSample();
            var totalMs = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Stop();
            var rebuildMs = totalMs - creationMs - enablingMs;

            if (run >= 0)
            {
                creationTimes[run] = creationMs;
                enablingTimes[run] = enablingMs;
                rebuildTimes[run] = rebuildMs;
                totalTimes[run] = totalMs;
                Debug.Log($"[Issue246] {label}; count={count}; run={run + 1}; " +
                          $"instantiate={creationMs:F3} ms; enable={enablingMs:F3} ms; " +
                          $"rebuild={rebuildMs:F3} ms; total={totalMs:F3} ms");
            }

            yield return null;
            if (mask && enableAfterCreation) mask.enabled = false;
            if (rootSoftMaskable) rootSoftMaskable.enabled = false;
            foreach (var item in items) Destroy(item);
            yield return null;
            yield return null;
            if (rootSoftMaskable) rootSoftMaskable.enabled = true;
        }

        Array.Sort(creationTimes);
        Array.Sort(enablingTimes);
        Array.Sort(rebuildTimes);
        Array.Sort(totalTimes);
        Debug.Log($"[Issue246] {label}; count={count}; median instantiate={Median(creationTimes):F3} ms; " +
                  $"enable={Median(enablingTimes):F3} ms; rebuild={Median(rebuildTimes):F3} ms; " +
                  $"total={Median(totalTimes):F3} ms");
    }

    private static double Median(double[] values)
    {
        var middle = values.Length / 2;
        return values.Length % 2 == 0 ? (values[middle - 1] + values[middle]) / 2 : values[middle];
    }
}
