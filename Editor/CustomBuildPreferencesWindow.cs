using ImverGames.CustomBuildSettings.Data;
using UnityEditor;
using UnityEngine;

namespace ImverGames.BuildIncrementor.Editor
{
    public class CustomBuildPreferencesWindow : EditorWindow
    {
        public static CustomBuildPreferencesWindow Instance { get; private set; }

        private static BuildPreferencesData buildPreferencesData;
        
        public static CustomBuildPreferencesWindow CreateOrFocusWindow()
        {
            if (Instance == null)
            {
                Instance = GetWindow<CustomBuildPreferencesWindow>("Custom Build Preferences");
                Instance.minSize = new Vector2(600, 300);
                Instance.Show();
            }
            else
            {
                Instance.Focus();
            }
            
            buildPreferencesData = DataBinder.GetData<BuildPreferencesData>();
            
            return Instance;
        }

        private void OnGUI()
        {
            buildPreferencesData.GlobalDataStorage = EditorGUILayout.ObjectField("Global Data Storage",
                buildPreferencesData.GlobalDataStorage, typeof(GlobalDataStorage), false) as GlobalDataStorage;
        }
    }
}