using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ImverGames.CustomBuildSettings.Data;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Editor
{
    public static class BuildTypeVersionIncrementor
    {
        private const string VERSION_PATH = "Assets/Editor Default Resources/Build Incrementor/";
        private static readonly string VERSION_NAME = $"BuildVersion{EditorUserBuildSettings.activeBuildTarget}.txt";

        private static string fullPath => $"{VERSION_PATH}{VERSION_NAME}";
        
        
        public static string IncrementVersion(
            BuildIncrementorData buildIncrementorData,
            out CustomBuildData buildData,
            string versionPattern = null,
            string formatPattern = null)
        {

            var pattern = string.IsNullOrEmpty(versionPattern) ? CreateVersionPattern(formatPattern) : versionPattern;
            
            if (!TryLoadVersionFromFile(out var customBuildData))
                customBuildData = SaveVersionFile(pattern, buildIncrementorData.VersionTag.Value,
                    buildIncrementorData.VersionMeta.Value, buildIncrementorData.SelectedBuildType.Value);

            buildData = customBuildData;
            
            var match = Regex.Match(buildData.GetBuildVersion(buildIncrementorData.SelectedBuildType.Value), @"^(\d+)\.(\d+)\.(\d+)$");

            if (match.Success)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var patch = int.Parse(match.Groups[3].Value);

                switch (buildIncrementorData.SelectedBuildType.Value)
                {
                    case EBuildType.RELEASE:
                        major++;
                        minor = 0;
                        patch = 0;
                        
                        buildIncrementorData.VersionTag.Value = "";
                        buildIncrementorData.VersionMeta.Value = "";
                        break;
                    case EBuildType.MILESTONE:
                        minor++;
                        patch = 0;
                        break;
                    case EBuildType.DAILY:
                        patch++;
                        break;
                }
                
                var newVersion = $"{major}.{minor}.{patch}";
                
                var fullVersion = buildData.RegisterOrUpdateVersion(buildIncrementorData.SelectedBuildType.Value, newVersion,
                    buildIncrementorData.VersionTag.Value, buildIncrementorData.VersionMeta.Value);
            
                PlayerSettings.bundleVersion = fullVersion;
                
                SaveVersionFile(buildIncrementorData.Version.Value, buildIncrementorData.VersionTag.Value,
                    buildIncrementorData.VersionMeta.Value, buildIncrementorData.SelectedBuildType.Value);
            
                return fullVersion;
            }
            else
            {
                Debug.LogError("Invalid version format");
            }

            return string.Empty;
        }
        
        public static string CreateVersionPattern(string formatPattern)
        {
            var formatParts = formatPattern.Split('_');
            
            var pattern = "^";

            foreach (var part in formatParts)
            {
                switch (part)
                {
                    case "D1":
                        pattern += @"(\d)";
                        break;
                    case "D2":
                        pattern += @"(\d{2})";
                        break;
                    case "D3":
                        pattern += @"(\d{3})";
                        break;
                }
                pattern += @"\.?";
            }

            pattern = pattern.TrimEnd('\\', '.', '?');
            pattern += "$";

            return pattern;
        }

        private static string CreateVersionString(IReadOnlyList<string> formatParts)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;

            string formattedNum1 = num1.ToString(formatParts[0]);
            string formattedNum2 = num2.ToString(formatParts[1]);
            string formattedNum3 = num3.ToString(formatParts[2]);

            return $"{formattedNum1}.{formattedNum2}.{formattedNum3}";
        }

        public static CustomBuildData SaveVersionFile(string version, string tag, string meta, EBuildType eBuildType)
        {
            var directory = System.IO.Path.GetDirectoryName(fullPath);
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            CustomBuildData buildData;

            if (!TryLoadVersionFromFile(out buildData))
                buildData = new CustomBuildData(eBuildType);

            buildData.BuildType = eBuildType;

            buildData.RegisterOrUpdateVersion(eBuildType, version, tag, meta);

            var json = JsonUtility.ToJson(buildData, true);
            
            System.IO.File.WriteAllText(fullPath, json);
            
            AssetDatabase.Refresh();

            return buildData;
        }
        
        public static bool TryLoadVersionFromFile(out CustomBuildData buildData)
        {
            buildData = null;
            
            TextAsset versionFile = EditorGUIUtility.Load(fullPath) as TextAsset;

            if (versionFile != null)
            { 
                buildData = JsonUtility.FromJson<CustomBuildData>(versionFile.text);

                return true;
            }

            return false;
        }
        
        public static void LoadOrSaveVersionFromFile(out CustomBuildData buildData)
        {
            buildData = null;
            
            TextAsset versionFile = EditorGUIUtility.Load(fullPath) as TextAsset;

            if (versionFile != null)
            { 
                buildData = JsonUtility.FromJson<CustomBuildData>(versionFile.text);

                return;
            }

            var pattern = CreateVersionString(new[] { "D1", "D1", "D1" });
            
            buildData = new CustomBuildData(default);

            SaveVersionFile(buildData.GetBuildVersion(buildData.BuildType), null, null, buildData.BuildType);
        }
    }
}