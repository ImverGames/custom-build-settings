using System;
using System.Collections.Generic;

namespace ImverGames.CustomBuildSettings.Data
{
    public class CustomBuildData
    {
        public EBuildType BuildType;
        public List<BuildTypeVersion> typeVersion;
        
        public CustomBuildData(EBuildType buildType)
        {
            typeVersion = new List<BuildTypeVersion>();
            this.BuildType = buildType;
        }

        public string RegisterOrUpdateVersion(EBuildType buildType, string version, string tag, string meta)
        {
            var v = typeVersion.Find(v => v.BuildType == buildType);
            if (v == null)
            {
                v = new BuildTypeVersion(buildType, version, tag, meta);
                typeVersion.Add(v);
            }
            else
            {
                v.Version = version;
                v.VersionTag = tag;
                v.VersionMeta = meta;
            }

            return GetFullBuildVersion(buildType);
        }

        public string GetBuildVersion(EBuildType buildType)
        {
            var v = FindVersionByBuildType(buildType);
            return v != null
                ? v.Version
                : RegisterOrUpdateVersion(buildType, CreateVersionString(new[] { "D1", "D1", "D1" }), string.Empty,
                    string.Empty);
        }
    
        public string GetBuildVersionTag(EBuildType buildType) =>
            FindVersionByBuildType(buildType)?.VersionTag ?? string.Empty;

        public string GetFullBuildVersion(EBuildType buildType)
        {
            var v = FindVersionByBuildType(buildType);
            if (v == null) return string.Empty;

            return string.IsNullOrEmpty(v.VersionTag) && string.IsNullOrEmpty(v.VersionMeta) ? v.Version :
                string.IsNullOrEmpty(v.VersionMeta) ? $"{v.Version}-{v.VersionTag}" :
                string.IsNullOrEmpty(v.VersionTag) ? $"{v.Version}.{v.VersionMeta}" :
                $"{v.Version}-{v.VersionTag}.{v.VersionMeta}";
        }

        public string GetBuildVersionMeta(EBuildType buildType) =>
            FindVersionByBuildType(buildType)?.VersionMeta ?? string.Empty;
    
        private BuildTypeVersion FindVersionByBuildType(EBuildType buildType) =>
            typeVersion.Find(v => v.BuildType == buildType);

        private string CreateVersionString(IReadOnlyList<string> formatParts)
        {
            int num1 = 0, num2 = 0, num3 = 0;
            return $"{num1.ToString(formatParts[0])}.{num2.ToString(formatParts[1])}.{num3.ToString(formatParts[2])}";
        }
    }

    [System.Serializable]
    public class BuildTypeVersion
    {
        public EBuildType BuildType;
        public string Version;
        public string VersionTag;
        public string VersionMeta;

        public BuildTypeVersion(EBuildType buildType, string version, string versionTag, string versionMeta)
        {
            BuildType = buildType;
            Version = version;
            VersionTag = versionTag;
            VersionMeta = versionMeta;
        }
    }
}