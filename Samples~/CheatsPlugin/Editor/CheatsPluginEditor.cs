using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Cheats.Editor
{
    [PluginOrder(3, "Cheats/CheatsPlugin")]
    public class CheatsPluginEditor : IBuildPluginEditor
    {
        private BuildDataProvider buildDataProvider;
        
        private BuildValue<bool> useCheats;

        public void InvokeSetupPlugin(BuildDataProvider buildDataProvider)
        {
            this.buildDataProvider = buildDataProvider;

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
