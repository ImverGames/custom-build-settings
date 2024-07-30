using System;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase
{
    [Serializable]
    public class DistributeToFirebasePluginData
    {
        public ETestersGroup TestersGroup;
        public bool UploadAutomatically;
        public bool UseGitLogger;
        
        public string DistributionGroups;
    }
}