using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace ImverGames.CustomBuildSettings.Data
{
    public class MainBuildData : IBuildData
    {
        public List<EditorBuildSettingsScene> Scenes;
        
        public BuildValue<EBuildType> SelectedBuildType;
        public BuildValue<EVersionFormatType> VersionFormat;
        public BuildValue<string> Version;
        public BuildValue<string> VersionTag;
        public BuildValue<string> VersionMeta;
        public BuildValue<bool> AddHashToVersion;

        public string BuildPath;

        public MainBuildData()
        {
            Scenes = new List<EditorBuildSettingsScene>();
            
            SelectedBuildType = new BuildValue<EBuildType>();
            VersionFormat = new BuildValue<EVersionFormatType>();
            Version = new BuildValue<string>();
            VersionTag = new BuildValue<string>();
            VersionMeta = new BuildValue<string>();
            AddHashToVersion = new BuildValue<bool>();
            
            BuildPath = string.Empty;
        }
        
        public void SetSceneList(List<EditorBuildSettingsSceneReference> scenes)
        {
            Scenes = scenes.Select(s => new EditorBuildSettingsScene {path = s.Path, enabled = s.Enabled}).ToList();
        }

        public List<EditorBuildSettingsSceneReference> GetSceneList()
        {
            return Scenes.Select(s => new EditorBuildSettingsSceneReference(s.path, s.enabled)).ToList();
        }

        public void Clear()
        {
            SelectedBuildType = null;
            VersionFormat = null;
            Version.Value = null;
            VersionTag.Value = null;
            VersionMeta.Value = null;
            AddHashToVersion = null;
            
            BuildPath = string.Empty;
        }
    }
}
