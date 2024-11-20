using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Coffee.UISoftMaskInternal.AssetModification
{
    internal class Runner
    {
        private readonly List<(string ext, Func<string, Modifier> create)> _factory;
        private readonly string _name;

        protected Runner(string name, List<(string ext, Func<string, Modifier> create)> factory)
        {
            _name = name;
            _factory = factory;
        }

        private IEnumerable<Modifier> CreateFromPath(string assetPath)
        {
            var ext = Path.GetExtension(assetPath);
            return _factory
                .Where(x => x.ext == ext)
                .Select(f => f.create?.Invoke(assetPath));
        }

        protected virtual Modifier[] GetModifiers(string[] assetPaths)
        {
            return assetPaths
                .SelectMany(CreateFromPath)
                .Where(x => x != null)
                .OrderBy(x => Path.GetExtension(x.path))
                .ToArray();
        }

        public void RunIfUserWantsTo()
        {
            var select = EditorUtility.DisplayDialogComplex($"Upgrade {_name}",
                "Upgrade all assets in this project?\n\n" +
                "'Go Ahead': Upgrades all assets in this project using the old APIs. You should make a backup before proceeding.\n\n" +
                "'Dry Run': Outputs the upgrade summary to the console without changing.", "I Made a Backup. Go Ahead!",
                "No Thanks", "Dry Run");
            if (select == 1) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var assetPaths = AssetDatabase.GetAllAssetPaths();
            var dryRun = select == 2;
            Run(assetPaths, dryRun);
        }

        public virtual void Run(string[] assetPaths, bool dryRun)
        {
            var modifiers = GetModifiers(assetPaths);
            var canceled = false;

            try
            {
                AssetDatabase.StartAssetEditing();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                for (var i = 0; i < modifiers.Length; i++)
                {
                    var percentage = (float)i / modifiers.Length;
                    var m = modifiers[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Upgrading...", m.path, percentage))
                    {
                        canceled = true;
                        break;
                    }

                    m.Modify(dryRun);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                var sb = new StringBuilder();
                sb.Append(dryRun ? "<b><color=orange>[DryRun]</color></b> " : "");
                sb.AppendLine($"<b>Modify '{_name}' is {(canceled ? "canceled" : "completed")}.</b>");
                sb.AppendLine("==== Modifications ====");
                Debug.Log(modifiers.Aggregate(sb, (x, m) => x.Append(m.GetModificationReport())));
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }
    }
}
