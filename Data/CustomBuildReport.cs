using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace ImverGames.CustomBuildSettings.Data
{
    public class CustomBuildReport : IBuildData
    {
        public BuildReport LastBuildReport { get; set; }
        public Dictionary<string, List<PackedAssetInfo>> AssetsByCategory { get; set; }
        
        public Dictionary<string, List<SimplePackedAssetInfo>> LoadedAssetsByCategory;
        public Dictionary<string, bool> Foldouts { get; set; }
        public Dictionary<string, float> CategorySizes { get; set; }
        public Dictionary<string, int> CurrentPage { get; set; }
        
        public int AssetsPerPage = 100;

        public CustomBuildReport()
        {
            AssetsByCategory = new Dictionary<string, List<PackedAssetInfo>>();
            Foldouts = new Dictionary<string, bool>();
            CategorySizes = new Dictionary<string, float>();
            CurrentPage = new Dictionary<string, int>();

            LoadedAssetsByCategory = new Dictionary<string, List<SimplePackedAssetInfo>>();
        }
        
        public void GenerateLastBuildReport()
        {
            AnalyzeAssets();
        }
        
        public void GenerateLoadedBuildReport(SerializableBuildReport serializableBuildReport)
        {
            AnalyzeSerializedAssets(serializableBuildReport);
        }

        private void AnalyzeAssets()
        {
            AssetsByCategory.Clear();
            CategorySizes.Clear();

            foreach (var packedAsset in LastBuildReport.packedAssets)
            {
                foreach (var asset in packedAsset.contents)
                {
                    string category = CategoryHelper.DetermineCategory(asset.sourceAssetPath);
                    if (!AssetsByCategory.ContainsKey(category))
                    {
                        AssetsByCategory[category] = new List<PackedAssetInfo>();
                        CategorySizes[category] = 0f;
                    }

                    if (!AssetsByCategory[category].Any(a => a.sourceAssetPath == asset.sourceAssetPath))
                    {
                        AssetsByCategory[category].Add(asset);
                        CategorySizes[category] += asset.packedSize / 1024f / 1024f;
                    }
                }
            }

            foreach (var category in AssetsByCategory.Keys.ToList())
            {
                AssetsByCategory[category] = AssetsByCategory[category].OrderByDescending(a => a.packedSize).ToList();
            }
        }
        
        public void AnalyzeSerializedAssets(SerializableBuildReport serializedReport)
        {
            LoadedAssetsByCategory.Clear();
            CategorySizes.Clear();

            foreach (var packedAsset in serializedReport.PackedAssets)
            {
                string category = packedAsset.Category;
                LoadedAssetsByCategory[category] = new List<SimplePackedAssetInfo>();
                CategorySizes[category] = 0f;

                for (int i = 0; i < packedAsset.AssetPaths.Count; i++)
                {
                    var assetPath = packedAsset.AssetPaths[i];
                    var assetSize = packedAsset.AssetSizes[i];

                    if (!LoadedAssetsByCategory[category].Any(a => a.SourceAssetPath == assetPath))
                    {
                        LoadedAssetsByCategory[category].Add(new SimplePackedAssetInfo(assetPath, assetSize));
                        CategorySizes[category] += assetSize / 1024f / 1024f;
                    }
                }

                LoadedAssetsByCategory[category] = LoadedAssetsByCategory[category].OrderByDescending(a => a.PackedSize).ToList();
            }
        }
        
        public string FormatSize(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public void Clear()
        {
            LastBuildReport = null;
            AssetsByCategory.Clear();
            Foldouts.Clear();
            CategorySizes.Clear();
            CurrentPage.Clear();
            LoadedAssetsByCategory.Clear();
        }
    }
}