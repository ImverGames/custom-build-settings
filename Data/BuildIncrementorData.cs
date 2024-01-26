namespace ImverGames.CustomBuildSettings.Data
{
    public class BuildIncrementorData
    {
        public BuildValue<EBuildType> SelectedBuildType;
        public BuildValue<EVersionFormatType> VersionFormat;
        public BuildValue<string> Version;

        public BuildIncrementorData()
        {
            SelectedBuildType = new BuildValue<EBuildType>();
            VersionFormat = new BuildValue<EVersionFormatType>();
            Version = new BuildValue<string>();
        }
    }
}