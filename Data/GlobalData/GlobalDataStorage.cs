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
        [SerializeField] public List<PluginsStorage> editorPlugins;

        public GlobalDataStorage()
        {
            pluginsData = new List<DataStorage>();
        }

        public bool TryGetPluginData<T>(out T data)
        {
            data = default;
            
            var storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == typeof(T));
            
            if (storage != null)
            {
                data = JsonUtility.FromJson<T>(storage.PluginData);
                return data != null;
            }
            return false;
        }

        public T SaveOrUpdatePluginData<T>(T data)
        {
            EditorUtility.SetDirty(this);

            var storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == typeof(T));
            
            if (storage == null)
            {
                storage = new DataStorage { PluginTypeName = typeof(T).AssemblyQualifiedName };
                pluginsData.Add(storage);
            }

            storage.PluginData = JsonUtility.ToJson(data);

            return data;
        }
    
        private Type StringToType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString)) return null;

            Type type = Type.GetType(typeString, false);
            
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeString, false);
                if (type != null) return type;
            }

            return null;
        }
    }
}

#endif
