#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    public class GlobalDataStorage : ScriptableObject
    {
        [SerializeField] private List<DataStorage> pluginsData;

        public GlobalDataStorage()
        {
            pluginsData = new List<DataStorage>();
        }

        public bool TryGetPluginData<T>(out T data)
        {
            data = default;

            try
            {
                var dataString = pluginsData.Find(storage => StringToType(storage.PluginTypeName) == typeof(T)).PluginData;
            
                data = JsonUtility.FromJson<T>(dataString);
                
                return data != null;
            }
            catch
            {
                return false;
            }
        }

        public T SaveOrUpdatePluginData<T>(T data)
        {
            EditorUtility.SetDirty(this);

            string pluginData = string.Empty;
            
            try
            {
                var existedStorage = GetPluginStorage<T>();

                existedStorage.PluginData = JsonUtility.ToJson(data);

                pluginData = existedStorage.PluginData;
            }
            catch
            {
                var dataStorage = new DataStorage
                {
                    PluginTypeName = typeof(T).AssemblyQualifiedName,
                    PluginData = JsonUtility.ToJson(data)
                };

                pluginData = dataStorage.PluginData;

                pluginsData.Add(dataStorage);
            }
            
            AssetDatabase.SaveAssetIfDirty(this);
            
            return JsonUtility.FromJson<T>(pluginData);
        }
        
        private DataStorage GetPluginStorage<T>()
        {
            return pluginsData.Find(storage => Type.GetType(storage.PluginTypeName) == typeof(T));
        }
        
        private Type StringToType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
            {
                return null;
            }

            Type type = Type.GetType(typeString);
            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeString);
                    if (type != null)
                    {
                        break;
                    }
                }
            }

            return type;
        }
    }
}

#endif