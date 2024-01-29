namespace ImverGames.CustomBuildSettings.Data
{
    [System.Serializable]
    public struct SimplePackedAssetInfo
    {
        public string SourceAssetPath;
        public ulong PackedSize;

        public SimplePackedAssetInfo(string path, ulong size)
        {
            SourceAssetPath = path;
            PackedSize = size;
        }
    }
}