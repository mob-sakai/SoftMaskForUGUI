#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using Debug = UnityEngine.Debug;

internal static class TextMeshProSupport
{
    private const string k_Version = "userData: v3.3.0";
    private const string k_VersionUnity6 = k_Version + " (Unity 6)";

    private static readonly Dictionary<string, string> s_FileGuids = new Dictionary<string, string>()
    {
        { "Hidden-TMP_Bitmap-Mobile-SoftMaskable.shader.meta", "guid: 9f04b08d6b8294ad58254b2d246d730a" },
        { "Hidden-TMP_Bitmap-SoftMaskable.shader.meta", "guid: 98210fc82c55b44748b9a8679828a56a" },
        { "Hidden-TMP_SDF Overlay-SoftMaskable.shader.meta", "guid: a89f3215f3b4c4b058fab65c73921eec" },
        { "Hidden-TMP_SDF SSD-SoftMaskable.shader.meta", "guid: 73f09e0b51f9545d686192848731ea76" },
        { "Hidden-TMP_SDF-Mobile Overlay-SoftMaskable.shader.meta", "guid: eae998fe9f1e24576af0d5e5830bf6ed" },
        { "Hidden-TMP_SDF-Mobile SSD-SoftMaskable.shader.meta", "guid: 1a7472e1b1a7c452e840b1d40b27b54b" },
        { "Hidden-TMP_SDF-Mobile-SoftMaskable.shader.meta", "guid: e94e1f7424a0a43ac8b7ed672319bb4b" },
        { "Hidden-TMP_SDF-SoftMaskable.shader.meta", "guid: 63261060f5b084969aca3decbe52d1d0" }
    };

    [MenuItem("Development/TextMeshPro/", false, -40)]
    private static void _()
    {
    }

    [MenuItem("Development/TextMeshPro/List GUID, Version for TMP Samples", false)]
    private static void ListGuidsAndVersions()
    {
        var sb = new StringBuilder();
        ForEachMeta(x =>
        {
            var (path, guid, version) = x;
            sb.AppendLine($"{guid},\tversion: {version},\t{Path.GetFileName(path)}");
        });
        Debug.Log(sb);
    }

    [MenuItem("Development/TextMeshPro/Update GUID, Version for TMP Samples", false)]
    private static void UpdateGuidsAndVersions()
    {
        ForEachMeta(x =>
        {
            var (path, guid, version) = x;
            var fileName = Path.GetFileName(path);
            s_FileGuids.TryGetValue(fileName.Replace("-Unity6", ""), out var expectedGuid);
            var expectedVersion = path.Contains("-Unity6") ? k_VersionUnity6 : k_Version;

            if (expectedGuid != guid || expectedVersion != version)
            {
                Debug.Log($"Update: {fileName}");
                var meta = File.ReadAllText(path);
                File.WriteAllText(path, meta.Replace(guid, expectedGuid).Replace(version, expectedVersion));
            }
        });
    }

    private static void ForEachMeta(Action<(string path, string guid, string version)> action)
    {
        foreach (var p in Directory.GetFiles("Packages/src/Samples~", "*.shader.meta", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(p);
            if (!fileName.Contains("TMP_")) continue;

            var meta = File.ReadAllText(p);
            var guid = Regex.Match(meta, @"(guid: \w+)$", RegexOptions.Multiline).Groups[1].Value;
            var version = Regex.Match(meta, @"(userData: .*)$", RegexOptions.Multiline).Groups[1].Value;

            action((p, guid, version));
        }
    }
}
#endif
