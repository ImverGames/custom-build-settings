using System.Collections.Generic;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    [System.Serializable]
    public class ReportSerialization<T>
    {
        [SerializeField]
        private List<T> target;
        public List<T> ToList() { return target; }

        public ReportSerialization(List<T> target)
        {
            this.target = target;
        }
    }
}