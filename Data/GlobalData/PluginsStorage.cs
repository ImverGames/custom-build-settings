using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    [System.Serializable]
    public class PluginsStorage
    {
        [SerializeReference] public IBuildPluginEditor BuildPluginEditor;
        public bool Expand;
    }
}