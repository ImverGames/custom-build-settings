using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    
    /// <summary>
    /// Holds information about a plugin editor within the build settings,
    /// including a reference to the plugin editor implementation and its expanded state in the UI.
    /// </summary>
    [System.Serializable]
    public class PluginsStorage
    {
        /// <summary>
        /// A reference to an implementation of the IBuildPluginEditor interface.
        /// This allows for polymorphic storage of different types of build plugin editors.
        /// </summary>
        [SerializeReference] public IBuildPluginEditor BuildPluginEditor;
        
        /// <summary>
        /// Indicates whether the plugin editor UI should be expanded or collapsed.
        /// </summary>
        public bool Expand;
    }
}