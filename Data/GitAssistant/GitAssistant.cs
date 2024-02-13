using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    public class GitAssistant
    {
        private bool _gitAvailable;
        private bool _isFetching;
        private string _commitShortHash;
        private string _commitFullHash;
        private string _currentBranch;
        private string _commitsBehind;
        private string _fetchingText;
        private string _projectPath;
        private DateTime _lastUpdateTime;
        private int _dotCount;
        private CancellationTokenSource _cancellationTokenSource;

        public bool gitAvailable => _gitAvailable;
        public bool isFetching => _isFetching;
        public string commitShortHash => _commitShortHash;
        public string commitFullHash => _commitFullHash;
        public string currentBranch => _currentBranch;
        public string commitsBehind => _commitsBehind;
        public string fetchingText => _fetchingText;

        public GitAssistant()
        {
            _lastUpdateTime = DateTime.Now;
            _fetchingText = "Git fetching";
            _dotCount = 3;
            _projectPath = Application.dataPath;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task CheckAndUpdateGitInfo(EditorWindow window = null)
        {
            if (await CheckGitAvailable())
            {
                await UpdateGitInfoAsync(window);
            }
        }

        public void AnimateLoadingText()
        {
            if (_isFetching && (DateTime.Now - _lastUpdateTime).TotalSeconds > 0.5)
            {
                _dotCount = (_dotCount + 1) % 4;
                _fetchingText = "Git Fetching" + new string('.', _dotCount);
                _lastUpdateTime = DateTime.Now;
            }
        }

        public async Task<bool> CheckGitAvailable()
        {
            _gitAvailable = await CheckGitInstallation() && await CheckIfInsideWorkTree();
            return _gitAvailable;
        }

        public async Task<string> ExecuteGitCommandAsync(string command)
        {
            return await Task.Run(() => ExecuteGitCommand(command), _cancellationTokenSource.Token);
        }

        private string ExecuteGitCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo("git", command)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _projectPath
            };

            using (var process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                return string.IsNullOrEmpty(output) ? error : output;
            }
        }

        private async Task UpdateGitInfoAsync(EditorWindow window = null)
        {
            _isFetching = true;
            try
            {
                await Task.Run(() => UpdateGitInfo(), _cancellationTokenSource.Token);
            }
            finally
            {
                _isFetching = false;
                if (window != null)
                    window.Repaint();
            }
        }

        private void UpdateGitInfo()
        {
            UpdateCommitHash().Wait(_cancellationTokenSource.Token);
            UpdateCurrentBranch().Wait(_cancellationTokenSource.Token);
            UpdateCommitsBehind().Wait(_cancellationTokenSource.Token);
        }

        private async Task UpdateCommitHash()
        {
            var str = await ExecuteGitCommandAsync("rev-parse HEAD");
            _commitFullHash = str.Trim();
            _commitShortHash = _commitFullHash.Substring(0, Math.Min(7, _commitFullHash.Length));
        }

        private async Task UpdateCurrentBranch()
        {
            var str = await ExecuteGitCommandAsync("rev-parse --abbrev-ref HEAD");
            _currentBranch = str.Trim();
        }

        private async Task UpdateCommitsBehind()
        {
            await ExecuteGitCommandAsync("fetch");
            var str = await ExecuteGitCommandAsync($"rev-list --count HEAD...origin/{_currentBranch}");
            _commitsBehind = str.Trim();
        }

        private async Task<bool> CheckGitInstallation()
        {
            var str = await ExecuteGitCommandAsync("--version");
            return str.Length > 0;
        }

        private async Task<bool> CheckIfInsideWorkTree()
        {
            var str = await ExecuteGitCommandAsync("rev-parse --is-inside-work-tree");
            return str.Trim() == "true";
        }
    }
}
