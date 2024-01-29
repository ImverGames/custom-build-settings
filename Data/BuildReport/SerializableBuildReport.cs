using System.Collections.Generic;

namespace ImverGames.CustomBuildSettings.Data
{
    [System.Serializable]
    public class SerializableBuildReport
    {
        public List<SerializablePackedAsset> PackedAssets;
        public string ReportName;

        public SerializableBuildReport()
        {
            PackedAssets = new List<SerializablePackedAsset>();
            ReportName = string.Empty;
        }
    }
}