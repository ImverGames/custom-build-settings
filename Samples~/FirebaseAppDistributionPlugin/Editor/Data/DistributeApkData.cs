using System;
using System.Collections.Generic;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.DistributeToFirebase.Editor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase
{
    [System.Serializable]
    public class DistributeApkData
    {
        [SerializeField] private List<DistributeData> distributeDatas;
        public DistributeLogger DistributeLogger;

        public DistributeApkData()
        {
            distributeDatas = new List<DistributeData>();
            DistributeLogger = new DistributeLogger();
        }

        public bool TryGetData(EBuildType buildType, out DistributeData distributeData)
        {
            distributeData = null;
            
            distributeData = distributeDatas.Find(d => d.BuildType == buildType);

            return distributeData != null;
        }

        public DistributeData RegisterOrUpdateData(EBuildType buildType, DistributeData distributeData)
        {
            var data = distributeDatas.Find(d => d.BuildType == buildType);

            if (data == null)
            {
                data = distributeData;
                distributeDatas.Add(data);
            }
            else
            {
                var indexOf = distributeDatas.IndexOf(data);
                distributeDatas[indexOf] = distributeData;
            }

            return distributeData;
        }
    }

    [Serializable]
    public class DistributeData
    {
        public EBuildType BuildType;
        public ETestersGroup TestersGroup;
        public bool UploadAutomatically;
        public bool UseGitLogger;
        
        public string DistributionGroups;
    }
}