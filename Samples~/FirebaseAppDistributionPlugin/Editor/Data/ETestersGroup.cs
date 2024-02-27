using System;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase
{
    [Flags, Serializable]
    public enum ETestersGroup
    {
        None = 0,
        Dev = 1,
        Testers = 2,
        Internal_QA = 4,
    }
}