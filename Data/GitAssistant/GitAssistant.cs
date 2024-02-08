using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    public class GitAssistant
    {
        public bool addHeshToVersion;
        public bool gitAvailable { get; private set; }
        public bool isFetching { get; private set; }

        public string commitShortHash { get; private set; }
        public string commitFullHash { get; private set; }
        public string currentBranch { get; private set; }
        public string commitsBehind { get; private set; }
        public string fetchingText { get; private set; }
        
        private string projectPath;
        private DateTime lastUpdateTime;
        private int dotCount;

        public GitAssistant()
        {
            lastUpdateTime = DateTime.Now;
            fetchingText = "Git fetching";
            dotCount = 3;
            
            projectPath = Application.dataPath;
        }

        public void CheckAndUpdateGitInfo(EditorWindow window = null)
        {
            if (CheckGitAvailable())
            {
                UpdateGitInfoAsync(window);
            }
        }
        
        public void AnimateLoadingText()
        {
            if (isFetching && (DateTime.Now - lastUpdateTime).TotalSeconds > 0.5)
            {
                dotCount = (dotCount + 1) % 4;
                fetchingText = "Git Fetching" + new string('.', dotCount);
                lastUpdateTime = DateTime.Now;
            }
        }

        public bool CheckGitAvailable()
        {
            gitAvailable = CheckGitInstallation() && CheckIfInsideWorkTree();

            return gitAvailable;
        }
        
        public string ExecuteGitCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo("git", command)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = projectPath
            };

            using (var process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                return string.IsNullOrEmpty(output) ? error : output;
            }
        }

        private async void UpdateGitInfoAsync(EditorWindow window = null)
        {
            isFetching = true;
            await Task.Run(UpdateGitInfo);
            isFetching = false;
            
            if(window != null)
                window.Repaint();
        }
        
        private void UpdateGitInfo()
        {
            UpdateCommitHash();
            UpdateCurrentBranch();
            UpdateCommitsBehind();
        }
        
        private bool CheckGitInstallation()
        {
            return ExecuteGitCommand("--version").Length > 0;
        }

        private bool CheckIfInsideWorkTree()
        {
            return ExecuteGitCommand("rev-parse --is-inside-work-tree").Trim() == "true";
        }

        private void UpdateCommitHash()
        {
            commitFullHash = ExecuteGitCommand("rev-parse HEAD").Trim();
            commitShortHash = commitFullHash.Substring(0, Math.Min(7, commitFullHash.Length));
        }
        
        private void UpdateCurrentBranch()
        {
            currentBranch = ExecuteGitCommand("rev-parse --abbrev-ref HEAD").Trim();
        }
        
        private void UpdateCommitsBehind()
        {
            ExecuteGitCommand("fetch");
            commitsBehind = ExecuteGitCommand($"rev-list --count HEAD...origin/{currentBranch}").Trim();
        }
    }
}