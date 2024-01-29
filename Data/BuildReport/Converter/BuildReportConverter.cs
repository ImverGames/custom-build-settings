using System.Collections.Generic;
using UnityEditor.Build.Reporting;

namespace ImverGames.CustomBuildSettings.Data
{
    public static class BuildReportConverter
    {
        public static SerializableBuildReport ConvertToSerializable(BuildReport report)
        {
            var serializableReport = new SerializableBuildReport();

            foreach (var packedAsset in report.packedAssets)
            {
                foreach (var asset in packedAsset.contents)
                {
                    string category = CategoryHelper.DetermineCategory(asset.sourceAssetPath);
                    var categoryAsset = serializableReport.PackedAssets.Find(x => x.Category == category);
                    if (categoryAsset == null)
                    {
                        categoryAsset = new SerializablePackedAsset(category);
                        serializableReport.PackedAssets.Add(categoryAsset);
                    }
                    categoryAsset.AssetPaths.Add(asset.sourceAssetPath);
                    categoryAsset.AssetSizes.Add(asset.packedSize);
                }
            }

            return serializableReport;
        }
    }
}