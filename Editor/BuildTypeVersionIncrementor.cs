using System.Collections.Generic;
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
            EBuildType eBuildType,
            out CustomBuildData buildData,
            string versionPattern = null, 
            string formatPattern = null)
        {
            var formatParts = string.IsNullOrEmpty(formatPattern)
                ? new[] { "D1", "D1", "D1" }
                : formatPattern.Split('_');
            
            var pattern = string.IsNullOrEmpty(versionPattern) ? CreateVersionString(formatParts) : versionPattern;

            if (!TryLoadVersionFromFile(out var customBuildData))
                customBuildData = SaveVersionFile(pattern, eBuildType);

            buildData = customBuildData;

            var version = customBuildData.Version;

            string[] parts = string.IsNullOrEmpty(version) 
                ? string.IsNullOrEmpty(versionPattern) 
                    ? pattern.Split('.') 
                    : versionPattern.Split('.')
                : version.Split('.');

            switch (eBuildType)
            {
                case EBuildType.RELEASE:
                    parts[0] = IncrementAndFormatNumber(parts[0], formatParts[0]);
                    break;
                case EBuildType.MILESTONE:
                    parts[1] = IncrementAndFormatNumber(parts[1], formatParts[1]);
                    break;
                case EBuildType.DAILY:
                    parts[2] = IncrementAndFormatNumber(parts[2], formatParts[2]);
                    break;
                case EBuildType.DEVELOPMENT:
                    break;
            }

            var newVersion = string.Join(".", parts);

            buildData.Version = newVersion;
            
            PlayerSettings.bundleVersion = newVersion;
        
            SaveVersionFile(PlayerSettings.bundleVersion, eBuildType);
            
            return newVersion;
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

        private static string IncrementNumber(string numberPart)
        {
            int number = int.Parse(numberPart);
            number++;
            return number.ToString();
        }

        private static string IncrementAndFormatNumber(string dailyPart, string format)
        {
            int number = int.Parse(dailyPart);
            number++;

            return number.ToString(format);
        }

        public static CustomBuildData SaveVersionFile(string version, EBuildType eBuildType)
        {
            var directory = System.IO.Path.GetDirectoryName(fullPath);
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            CustomBuildData buildData = new CustomBuildData(eBuildType, version);

            var json = JsonUtility.ToJson(buildData);
            
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
            
            buildData = new CustomBuildData(default, pattern);

            SaveVersionFile(buildData.Version, buildData.BuildType);
        }
    }
}