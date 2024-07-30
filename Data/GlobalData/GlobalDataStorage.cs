#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Manages global data storage for custom build settings, including main and plugin-specific build data.
    /// </summary>
    public class GlobalDataStorage : ScriptableObject, IBuildData
    {
        [SerializeField] private CustomBuildData customBuildData;
        [SerializeField] private List<BuildPluginDataStorage> pluginsData;
        [SerializeField] public List<PluginsStorage> editorPlugins;
        
        /// <summary>
        /// Gets the custom build data.
        /// </summary>
        public CustomBuildData CustomBuildData => customBuildData;

        /// <summary>
        /// Initializes a new instance of the GlobalDataStorage class.
        /// </summary>
        public GlobalDataStorage()
        {
            pluginsData = new List<BuildPluginDataStorage>();
        }
        
        /// <summary>
        /// Registers or updates the main build data for a specified build type.
        /// </summary>
        /// <param name="buildType">The type of the build.</param>
        /// <param name="data">The build type version data to register or update.</param>
        /// <returns>The updated or newly registered build type version.</returns>
        public BuildTypeVersion RegisterOrUpdateMainData(EBuildType buildType, BuildTypeVersion data)
        {
            EditorUtility.SetDirty(this);
            
            var v = customBuildData.typeVersion.Find(v => v.BuildType == buildType);
            if (v == null)
            {
                v = data;
                customBuildData.typeVersion.Add(v);
            }
            else
            {
                v = data;
            }

            return v;
        }
        
        /// <summary>
        /// Attempts to get the main build data for a specified build type.
        /// </summary>
        /// <param name="buildType">The build type to query.</param>
        /// <param name="data">Output parameter for the found build type version data.</param>
        /// <returns>True if the data is found, otherwise false.</returns>
        public bool TryGetMainData(EBuildType buildType, out BuildTypeVersion data)
        {
            data = customBuildData.typeVersion.Find(v => v.BuildType == buildType);
            return data != null;
        }

        /// <summary>
        /// Attempts to get plugin-specific build data for a given type and build type.
        /// </summary>
        /// <typeparam name="T">The expected data type.</typeparam>
        /// <param name="pluginType">The plugin type.</param>
        /// <param name="buildType">The build type.</param>
        /// <param name="data">Output parameter for the found data.</param>
        /// <returns>True if the data is found and successfully deserialized, otherwise false.</returns>
        public bool TryGetPluginData<T>(Type pluginType, EBuildType buildType, out T data)
        {
            data = default;
            
            var storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == pluginType);

            var buildPluginData = storage?.PluginData.Find(d => d.BuildType == buildType);

            if (buildPluginData == null) return false;

            data = (T)buildPluginData.BuildTypeData;
                    
            return data != null;
        }
        
        /// <summary>
        /// Attempts to get plugin-specific build data for a given type and build type, including the plugin storage.
        /// </summary>
        /// <typeparam name="T">The expected data type.</typeparam>
        /// <param name="pluginType">The plugin type.</param>
        /// <param name="buildType">The build type.</param>
        /// <param name="data">Output parameter for the found data.</param>
        /// <param name="pluginStorage">Output parameter for the found plugin storage.</param>
        /// <returns>True if the data is found and successfully deserialized, otherwise false.</returns>
        public bool TryGetPluginData<T>(Type pluginType, EBuildType buildType, out T data, out BuildPluginDataStorage pluginStorage)
        {
            data = default;
            
            var storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == pluginType);
            
            pluginStorage = storage;

            var buildPluginData = storage?.PluginData.Find(d => d.BuildType == buildType);

            if (buildPluginData == null) return false;
            
            data = (T)buildPluginData.BuildTypeData;
                    
            return data != null;
        }
        
        /// <summary>
        /// Attempts to find the storage for a given plugin type.
        /// </summary>
        /// <param name="pluginType">The type of the plugin.</param>
        /// <param name="storage">Output parameter for the found storage.</param>
        /// <returns>True if the storage is found, otherwise false.</returns>
        public bool TryGetStorage(Type pluginType, out BuildPluginDataStorage storage)
        {
            storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == pluginType);
            return storage != null;
        }
        
        /// <summary>
        /// Registers or updates storage for a specific plugin type.
        /// </summary>
        /// <param name="pluginType">The plugin type.</param>
        /// <param name="storage">The plugin data storage to register or update.</param>
        /// <returns>The updated or newly registered plugin data storage.</returns>
        public BuildPluginDataStorage RegisterOrUpdateStorage(Type pluginType, BuildPluginDataStorage storage)
        {
            EditorUtility.SetDirty(this);
            
            var s = pluginsData.Find(st => StringToType(st.PluginTypeName) == pluginType);
            if (s == null)
            {
                s = storage;
                pluginsData.Add(s);
            }
            else
            {
                s = storage;
            }

            return s;
        }

        /// <summary>
        /// Registers or updates plugin-specific build data for a given type and build type.
        /// </summary>
        /// <typeparam name="T">The data type to register or update.</typeparam>
        /// <param name="pluginType">The plugin type.</param>
        /// <param name="buildType">The build type.</param>
        /// <param name="data">The data to register or update.</param>
        /// <returns>The data after being registered or updated.</returns>
        public T RegisterOrUpdatePluginData<T>(Type pluginType, EBuildType buildType, T data)
        {
            EditorUtility.SetDirty(this);

            var storage = pluginsData.Find(s => StringToType(s.PluginTypeName) == pluginType);
            
            if (storage == null)
                storage = RegisterOrUpdateStorage(pluginType, new BuildPluginDataStorage { PluginTypeName = pluginType.AssemblyQualifiedName });

            var pluginData = storage.PluginData.Find(d => d.BuildType == buildType);
            
            if (pluginData == null)
            {
                pluginData = new BuildPluginData { BuildType = buildType };
                storage.PluginData.Add(pluginData);
            }
            
            pluginData.BuildTypeData = data;

            return data;
        }
    
        /// <summary>
        /// Converts a string representation of a type to a Type instance.
        /// </summary>
        /// <param name="typeString">The string representation of the type.</param>
        /// <returns>The Type instance if found; null otherwise.</returns>
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

        /// <summary>
        /// Clears all stored data. Implementation should be provided as needed.
        /// </summary>
        public void Clear()
        {
            //Do not clear the data
        }
    }
}

#endif
