using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;

namespace ArcaneOnyx.ModuleDefines
{
    // Presence of this module declares MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST so other
    // modules can gate optional integration code behind
    // `#if MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST`. Applied to every build target the
    // editor knows about, so switching platform never drops the define.
    [InitializeOnLoad]
    internal static class ScriptableObjectDatabaseModuleDefine
    {
        private const string Symbol = "MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST";

        static ScriptableObjectDatabaseModuleDefine()
        {
            // Deferred: on a fresh .unitypackage import this static ctor runs while the
            // asset database is still importing, and PlayerSettings edits made there are
            // ignored by the compilation pipeline (the define would only apply after an
            // editor restart). Wait for the editor to go idle, then apply and recompile.
            EditorApplication.delayCall += Apply;
        }

        private static void Apply()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Apply;
                return;
            }

            var changed = false;
            foreach (var named in AllNamedBuildTargets())
            {
                try
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbols(named)
                                                .Split(';')
                                                .Where(s => !string.IsNullOrEmpty(s))
                                                .ToList();
                    if (defines.Contains(Symbol)) continue;
                    defines.Add(Symbol);
                    PlayerSettings.SetScriptingDefineSymbols(named, string.Join(";", defines));
                    changed = true;
                }
                catch (Exception)
                {
                    // Platform not supported by this editor install; nothing to define for it.
                }
            }

            if (!changed) return;
            // Flush ProjectSettings.asset and force the recompile ourselves; setting the
            // symbols alone does not trigger one from script.
            AssetDatabase.SaveAssets();
            CompilationPipeline.RequestScriptCompilation();
        }

        // NamedBuildTarget exposes its platforms only as static fields, so read them by
        // reflection instead of hardcoding a list that would rot across editor versions.
        private static IEnumerable<NamedBuildTarget> AllNamedBuildTargets()
        {
            var targets = typeof(NamedBuildTarget)
                          .GetFields(BindingFlags.Public | BindingFlags.Static)
                          .Where(f => f.FieldType == typeof(NamedBuildTarget))
                          .Select(f => (NamedBuildTarget)f.GetValue(null))
                          .Where(t => !string.IsNullOrEmpty(t.TargetName))
                          .ToList();

            if (targets.Count > 0) return targets;

            // Reflection came up empty (API shape changed): fall back to the active target
            // so the define is at least correct for what the user is building right now.
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return new[] { NamedBuildTarget.FromBuildTargetGroup(group) };
        }
    }
}
