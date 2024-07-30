using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Represents a storage container for data associated with a specific build plugin.
    /// </summary>
    [System.Serializable]
    public class BuildPluginDataStorage
    {
        /// <summary>
        /// The fully qualified name of the plugin type.
        /// </summary>
        public string PluginTypeName;
        
        /// <summary>
        /// A list of build-specific data for the plugin.
        /// </summary>
        public List<BuildPluginData> PluginData;
        
        /// <summary>
        /// Shared data for the plugin that is applicable across all build types.
        /// This field supports polymorphic data types at runtime.
        /// </summary>
        [SerializeReference] public Object SharedBuildTypePluginData;
        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildPluginDataStorage"/> class.
        /// </summary>
        public BuildPluginDataStorage()
        {
            PluginData = new List<BuildPluginData>();
        }
    }
    
    
    /// <summary>
    /// Contains data specific to a build type for a plugin.
    /// </summary>
    [System.Serializable]
    public class BuildPluginData
    {
        /// <summary>
        /// The type of build this data is associated with.
        /// </summary>
        public EBuildType BuildType;
        
        /// <summary>
        /// The specific data for this build type, stored as a Object.
        /// /// This field supports polymorphic data types at runtime.
        /// </summary>
        [SerializeReference] public Object BuildTypeData;
    }
}