using System.Collections.Generic;
using UnityEditor;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Represents the custom build configuration data for different build types.
    /// </summary>
    [System.Serializable]
    public class CustomBuildData
    {
        /// <summary>
        /// The type of the build this data applies to.
        /// </summary>
        public EBuildType BuildType;
        
        /// <summary>
        /// A list of version data associated with this build type.
        /// </summary>
        public List<BuildTypeVersion> typeVersion;
        
        /// <summary>
        /// Initializes a new instance of the CustomBuildData class for a specified build type.
        /// </summary>
        /// <param name="buildType">The build type this data is for.</param>
        public CustomBuildData(EBuildType buildType)
        {
            typeVersion = new List<BuildTypeVersion>();
            this.BuildType = buildType;
        }

        /// <summary>
        /// Gets the full build version string for a specified build type, including version, tag, and meta information.
        /// </summary>
        /// <param name="buildType">The build type to get the version string for.</param>
        /// <returns>A string representing the full version, or an empty string if no version is found.</returns>
        public string GetFullBuildVersion(EBuildType buildType)
        {
            var v = FindVersionByBuildType(buildType);
            if (v == null) return string.Empty;

            return string.IsNullOrEmpty(v.VersionTag) && string.IsNullOrEmpty(v.VersionMeta) ? v.Version :
                string.IsNullOrEmpty(v.VersionMeta) ? $"{v.Version}-{v.VersionTag}" :
                string.IsNullOrEmpty(v.VersionTag) ? $"{v.Version}.{v.VersionMeta}" :
                $"{v.Version}-{v.VersionTag}.{v.VersionMeta}";
        }
    
        /// <summary>
        /// Finds the version data for a specified build type.
        /// </summary>
        /// <param name="buildType">The build type to find the version data for.</param>
        /// <returns>The BuildTypeVersion associated with the specified build type, or null if not found.</returns>
        private BuildTypeVersion FindVersionByBuildType(EBuildType buildType) =>
            typeVersion.Find(v => v.BuildType == buildType);
    }

    /// <summary>
    /// Represents version information for a build, including the base version, tag, and meta information.
    /// </summary>
    [System.Serializable]
    public class BuildTypeVersion
    {
        /// <summary>
        /// The build type this version information applies to.
        /// </summary>
        public EBuildType BuildType;
        
        /// <summary>
        /// The base version string.
        /// </summary>
        public string Version;
        
        /// <summary>
        /// An optional tag to append to the version for additional context or labeling.
        /// </summary>
        public string VersionTag;
        
        /// <summary>
        /// Additional metadata to append to the version, typically used for build or revision identifiers.
        /// </summary>
        public string VersionMeta;
        
        /// <summary>
        /// Indicates whether a hash should be added to the version for uniqueness.
        /// </summary>
        public bool AddHash;

        /// <summary>
        /// A list of scenes to include in the build.
        /// </summary>
        public List<EditorBuildSettingsSceneReference> Scenes;

        /// <summary>
        /// Initializes a new instance of the BuildTypeVersion class with optional version information.
        /// </summary>
        /// <param name="buildType">The build type this version is for.</param>
        /// <param name="version">The base version string, defaulting to "0.0.0".</param>
        public BuildTypeVersion(EBuildType buildType, string version = "0.0.0")
        {
            BuildType = buildType;
            Version = version;
            VersionTag = string.Empty;
            VersionMeta = string.Empty;
            AddHash = false;
            Scenes = new List<EditorBuildSettingsSceneReference>();
        }
    }
    
    /// <summary>
    /// Represents a reference to a scene in the Unity Editor Build Settings.
    /// </summary>
    [System.Serializable]
    public class EditorBuildSettingsSceneReference
    {
        /// <summary>
        /// The path to the scene asset.
        /// </summary>
        public string Path;
        
        /// <summary>
        /// Indicates whether the scene is enabled in the build settings.
        /// </summary>
        public bool Enabled;

        public EditorBuildSettingsSceneReference(string path, bool enabled)
        {
            Path = path;
            Enabled = enabled;
        }
    }
}
