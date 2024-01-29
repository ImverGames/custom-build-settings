using System.Collections.Generic;

namespace ImverGames.CustomBuildSettings.Data
{
    [System.Serializable]
    public class SerializablePackedAsset
    {
        public string Category;
        public List<string> AssetPaths;
        public List<ulong> AssetSizes;

        public SerializablePackedAsset(string category)
        {
            Category = category;
            AssetPaths = new List<string>();
            AssetSizes = new List<ulong>();
        }
    }
}