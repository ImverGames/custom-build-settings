using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    public static class BuildReportManager
    {
        private static List<SerializableBuildReport> savedReports = new List<SerializableBuildReport>();
        private static readonly string SaveFolderPath = Path.Combine(Application.persistentDataPath, "CustomBuildReports");
        private static readonly string SaveFilePath = Path.Combine(SaveFolderPath, "buildReports.json");

        static BuildReportManager()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        public static void SaveReport(SerializableBuildReport report)
        {
            if (savedReports.Count >= 5) 
            {
                savedReports.RemoveAt(0);
            }

            savedReports.Add(report);
            string json = JsonUtility.ToJson(new ReportSerialization<SerializableBuildReport>(savedReports), true);
            File.WriteAllText(SaveFilePath, json);
        }

        public static List<SerializableBuildReport> LoadReports()
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                var loadedReports = JsonUtility.FromJson<ReportSerialization<SerializableBuildReport>>(json).ToList();
                savedReports = loadedReports ?? new List<SerializableBuildReport>();
            }
            return savedReports;
        }
    }
}