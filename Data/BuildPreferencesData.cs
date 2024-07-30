using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    public class BuildPreferencesData : IBuildData
    {
        private const string STORAGE_PATH = "Assets/Editor Default Resources/Custom Build Settings/Global Storage/";
        private const string STORAGE_NAME = "GlobalDataStorage.asset";
        private static string fullPath => $"{STORAGE_PATH}{STORAGE_NAME}";

        public GlobalDataStorage GlobalDataStorage;
        
        public BuildPreferencesData()
        {
            if (!TryLoadDataStorage(out GlobalDataStorage))
                CreateStorage();
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

            GlobalDataStorage = globalStorage;
        }

        public void Clear()
        {
            GlobalDataStorage = null;
        }
    }
}