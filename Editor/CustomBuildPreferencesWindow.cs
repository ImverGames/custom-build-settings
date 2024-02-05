using ImverGames.CustomBuildSettings.Data;
using UnityEditor;
using UnityEngine;

namespace ImverGames.BuildIncrementor.Editor
{
    public class CustomBuildPreferencesWindow : EditorWindow
    {
        private const string STORAGE_PATH = "Assets/Editor Default Resources/Build Incrementor/Global Storage/";
        private const string STORAGE_NAME = "GlobalDataStorage.asset";
        private static string fullPath => $"{STORAGE_PATH}{STORAGE_NAME}";
        
        private BuildPreferencesData buildPreferencesData;

        public CustomBuildPreferencesWindow ShowWindow()
        {
            return GetWindow<CustomBuildPreferencesWindow>("Custom Build Preferences");
        }
        
        public void Initialize(BuildPreferencesData buildPreferencesData)
        {
            this.buildPreferencesData = buildPreferencesData;

            if (!TryLoadDataStorage(out buildPreferencesData.GlobalDataStorage))
                CreateStorage();
        }

        private void OnGUI()
        {
            buildPreferencesData.GlobalDataStorage = EditorGUILayout.ObjectField("Global Data Storage",
                buildPreferencesData.GlobalDataStorage, typeof(GlobalDataStorage), false) as GlobalDataStorage;
        }

        private bool TryLoadDataStorage(out GlobalDataStorage globalDataStorage)
        {
            globalDataStorage = null;
            
            globalDataStorage = EditorGUIUtility.Load(fullPath) as GlobalDataStorage;

            return globalDataStorage != null;
        }

        private void CreateStorage()
        {
            var directory = System.IO.Path.GetDirectoryName(fullPath);
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var globalStorage = ScriptableObject.CreateInstance<GlobalDataStorage>();
            
            AssetDatabase.CreateAsset(globalStorage, fullPath);
            AssetDatabase.SaveAssets();

            buildPreferencesData.GlobalDataStorage = globalStorage;
        }
    }
}