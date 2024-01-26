namespace ImverGames.CustomBuildSettings.Data
{
    public class CustomBuildData
    {
        public EBuildType BuildType;
        public string Version;
        
        public CustomBuildData(EBuildType buildType, string version)
        {
            this.BuildType = buildType;
            this.Version = version;
        }
    }
}