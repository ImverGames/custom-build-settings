using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase.Editor
{
    [System.Serializable]
    public class DistributeNotesGitLogger
    {
        [SerializeField] private List<DistributeLoggerData> loggerData;
        
        public IEnumerable<DistributeLoggerData> LoggerData => loggerData;
        public bool HasData => loggerData.Count > 0;

        public DistributeNotesGitLogger()
        {
            loggerData = new List<DistributeLoggerData>();
        }
        
        public void AddOrUpdateData(string id, string notes, out string newLinesAdded)
        {
            var data = loggerData.Find(d => d.ID == id);
            if (data != null)
            {
                newLinesAdded = UpdateNotes(data, notes);
            }
            else
            {
                loggerData.Add(new DistributeLoggerData { ID = id, Notes = notes });
                newLinesAdded = notes;
            }
        }
        
        public bool TryGetNotes(string id, out string notes)
        {
            var data = loggerData.Find(d => d.ID == id);
            notes = data?.Notes;
            return data != null;
        }
        
        public void SaveToTextFile()
        {
            var path = EditorUtility.SaveFilePanel("Choose Location and Name for Release Notes", "", "ReleaseNotes", "txt");
            
            if (string.IsNullOrEmpty(path)) return;
            
            using (var file = new StreamWriter(path))
            {
                foreach (var data in LoggerData)
                {
                    file.WriteLine($"## {data.ID}");
                    file.WriteLine($"{data.Notes}");
                    file.WriteLine();
                }
            }
        }

        private string UpdateNotes(DistributeLoggerData data, string newNotes)
        {
            var existingLines = new HashSet<string>(data.Notes.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
            var newLines = newNotes.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
            var addedLines = new HashSet<string>();
            foreach (var line in newLines)
            {
                if (existingLines.Add(line))
                {
                    addedLines.Add(line);
                }
            }
        
            data.Notes = string.Join("\n", existingLines);
            return string.Join("\n", addedLines);
        }
    }
    
    [System.Serializable]
    public class DistributeLoggerData
    {
        public string ID;
        public string Notes;
    }
}