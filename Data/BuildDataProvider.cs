namespace ImverGames.CustomBuildSettings.Data
{
    public class BuildDataProvider
    {
        public BuildValue<EBuildType> SelectedBuildType;
        public BuildValue<EVersionFormatType> VersionFormat;
        public BuildValue<string> Version;
        public BuildValue<string> VersionTag;
        public BuildValue<string> VersionMeta;
        public BuildValue<bool> AddHashToVersion;

        public BuildPreferencesData BuildPreferencesData;
        public GitAssistant GitAssistant;

        public string BuildPath;

        public BuildDataProvider()
        {
            SelectedBuildType = new BuildValue<EBuildType>();
            VersionFormat = new BuildValue<EVersionFormatType>();
            Version = new BuildValue<string>();
            VersionTag = new BuildValue<string>();
            VersionMeta = new BuildValue<string>();
            AddHashToVersion = new BuildValue<bool>();
            
            BuildPreferencesData = new BuildPreferencesData();
            GitAssistant = new GitAssistant();
            
            BuildPath = string.Empty;
        }
    }
}
