using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ArcaneOnyx.ModuleDefines
{
    // Presence of this module declares MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST so other
    // modules can gate optional integration code behind
    // `#if MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST`. Runs for the active build target
    // group and re-applies on domain reload (e.g. after switching platform).
    [InitializeOnLoad]
    internal static class ScriptableObjectDatabaseModuleDefine
    {
        private const string Symbol = "MODULE_SCRIPTABLE_OBJECT_DATABASE_EXIST";

        static ScriptableObjectDatabaseModuleDefine()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var named = NamedBuildTarget.FromBuildTargetGroup(group);
            var defines = PlayerSettings.GetScriptingDefineSymbols(named)
                                        .Split(';')
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToList();
            if (defines.Contains(Symbol)) return;
            defines.Add(Symbol);
            PlayerSettings.SetScriptingDefineSymbols(named, string.Join(";", defines));
        }
    }
}
