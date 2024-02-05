namespace ImverGames.CustomBuildSettings.Data
{
    public class BuildIncrementorData
    {
        public BuildValue<EBuildType> SelectedBuildType;
        public BuildValue<EVersionFormatType> VersionFormat;
        public BuildValue<string> Version;
        public BuildValue<string> VersionTag;
        public BuildValue<string> VersionMeta;
        
        public BuildPreferencesData BuildPreferencesData;

        public BuildIncrementorData()
        {
            SelectedBuildType = new BuildValue<EBuildType>();
            VersionFormat = new BuildValue<EVersionFormatType>();
            Version = new BuildValue<string>();
            VersionTag = new BuildValue<string>();
            VersionMeta = new BuildValue<string>();
            BuildPreferencesData = new BuildPreferencesData();
        }
    }
}