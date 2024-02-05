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
            try
            {
                var v = typeVersion.Find(v => v.BuildType == buildType);
                v.Version = version;
                v.VersionTag = tag;
                v.VersionMeta = meta;
            }
            catch
            {
                typeVersion.Add(new BuildTypeVersion(buildType, version, tag, meta));
            }

            return GetFullBuildVersion(buildType);
        }

        public string GetBuildVersion(EBuildType buildType)
        {
            try
            {
                return typeVersion.Find(v => v.BuildType == buildType).Version;
            }
            catch
            {
                return RegisterOrUpdateVersion(buildType, CreateVersionString(new[] { "D1", "D1", "D1" }), string.Empty, string.Empty);
            }
        }
        
        public string GetBuildVersionTag(EBuildType buildType)
        {
            try
            {
                return typeVersion.Find(v => v.BuildType == buildType).VersionTag;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetFullBuildVersion(EBuildType buildType)
        {
            try
            {
                var v = typeVersion.Find(v => v.BuildType == buildType);

                if (string.IsNullOrEmpty(v.VersionTag) && string.IsNullOrEmpty(v.VersionMeta))
                    return v.Version;
                else if(string.IsNullOrEmpty(v.VersionMeta))
                    return $"{v.Version}-{v.VersionTag}";
                else if (string.IsNullOrEmpty(v.VersionTag))
                    return $"{v.Version}.{v.VersionMeta}";
                else
                    return $"{v.Version}-{v.VersionTag}.{v.VersionMeta}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetBuildVersionMeta(EBuildType buildType)
        {
            try
            {
                return typeVersion.Find(v => v.BuildType == buildType).VersionMeta;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        private string CreateVersionString(IReadOnlyList<string> formatParts)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;

            string formattedNum1 = num1.ToString(formatParts[0]);
            string formattedNum2 = num2.ToString(formatParts[1]);
            string formattedNum3 = num3.ToString(formatParts[2]);

            return $"{formattedNum1}.{formattedNum2}.{formattedNum3}";
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