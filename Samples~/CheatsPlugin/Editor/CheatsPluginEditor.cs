using ImverGames.CustomBuildSettings.Data;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Cheats.Editor
{
    public class CheatsPluginEditor : IBuildPluginEditor
    {
        private BuildIncrementorData buildIncrementorData;
        
        private bool foldout = true;
        
        private BuildValue<bool> useCheats;

        public void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData)
        {
            this.buildIncrementorData = buildIncrementorData;

            useCheats = new BuildValue<bool>();

#if CHEATS_ON
            useCheats.Value = true;
#else
            useCheats.Value = false;
#endif
            

            useCheats.OnValueChanged += OnChangeCheats;
        }

        public void InvokeOnFocusPlugin()
        {
            
        }

        public void InvokeGUIPlugin()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Cheats");

            if (foldout)
            {
                GUILayout.Space(5);
                
                string label = useCheats.Value ? "DisableCheats" : "EnableCheats";
                useCheats.Value = GUILayout.Toggle(useCheats.Value, label);
                
                GUILayout.Space(10);

                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    out var defs);

                foreach (var define in defs)
                    GUILayout.TextField(define);
                
                GUILayout.Space(10);

            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            GUILayout.EndVertical();
        }

        private void OnChangeCheats(bool value)
        {
            if (value)
            {
                var defineSymbolsForGroup =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    $"{defineSymbolsForGroup};CHEATS_ON");

                Debug.Log("Turn Cheats On");
            }
            else
            {
                var defineSymbolsForGroup =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Replace("CHEATS_ON", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbolsForGroup);

                Debug.Log("Turn Cheats Off");
            }
        }

        public void InvokeBeforeBuild()
        {
            
        }

        public void InvokeAfterBuild()
        {
            
        }
        
        public void InvokeDestroyPlugin()
        {
            if (useCheats != null)
            {
                useCheats.OnValueChanged -= OnChangeCheats;
                useCheats = null;
            }
        }
    }
}
