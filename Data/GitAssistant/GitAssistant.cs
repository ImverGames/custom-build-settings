using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ImverGames.CustomBuildSettings.Data
{
    
    /// <summary>
    /// Provides asynchronous Git operations and information retrieval for Unity Editor tools,
    /// including checking Git availability, fetching current branch, commit hashes, and commits behind the origin.
    /// </summary>
    public class GitAssistant : IBuildData
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
        private CancellationTokenSource cancellationTokenSource;

        public bool gitAvailable => _gitAvailable;
        public bool isFetching => _isFetching;
        public string commitShortHash => _commitShortHash;
        public string commitFullHash => _commitFullHash;
        public string currentBranch => _currentBranch;
        public string commitsBehind => _commitsBehind;
        public string fetchingText => _fetchingText;

        /// <summary>
        /// Initializes a new instance of the GitAssistant, setting up initial values and preparing for Git operations.
        /// </summary>
        public GitAssistant()
        {
            _lastUpdateTime = DateTime.Now;
            _fetchingText = "Git fetching";
            _dotCount = 3;
            _projectPath = Application.dataPath;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Checks if Git is available and updates Git information asynchronously.
        /// </summary>
        /// <param name="window">The editor window to repaint after updating Git information. Optional.</param>
        public async Task CheckAndUpdateGitInfo(EditorWindow window = null)
        {
            if(!IsGitInstalled())
                return;
            
            if (await CheckGitAvailable())
            {
                await UpdateGitInfoAsync(window);
            }
        }

        /// <summary>
        /// Animates the loading text by updating the number of dots in the fetching text, indicating an ongoing operation.
        /// </summary>
        public void AnimateLoadingText()
        {
            if (_isFetching && (DateTime.Now - _lastUpdateTime).TotalSeconds > 0.5)
            {
                _dotCount = (_dotCount + 1) % 4;
                _fetchingText = "Git Fetching" + new string('.', _dotCount);
                _lastUpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Checks if Git is available and the project is within a Git work tree.
        /// </summary>
        /// <returns>True if Git is available and the project is in a Git work tree; otherwise, false.</returns>
        public async Task<bool> CheckGitAvailable()
        {
            _gitAvailable = await CheckGitInstallation() && await CheckIfInsideWorkTree();
            return _gitAvailable;
        }

        /// <summary>
        /// Checks if Git is installed on the system.
        /// </summary>
        /// <returns>True if Git is installed on the system</returns>
        private bool IsGitInstalled()
        {
            string[] commonPaths = {
                @"C:\Program Files\Git\bin\git.exe",
                @"C:\Program Files (x86)\Git\bin\git.exe",
                @"/usr/local/bin/git",
                @"/usr/bin/git",
                @"/bin/git"
            };

            return commonPaths.Any(File.Exists);
        }

        /// <summary>
        /// Executes a Git command asynchronously and returns the output.
        /// </summary>
        /// <param name="command">The Git command to execute.</param>
        /// <param name="timeoutMilliseconds">The timeout for the command execution in milliseconds. Default is 30000 (30 seconds).</param>
        /// <returns>The output of the Git command.</returns>
        public async Task<string> ExecuteGitCommandAsync(string command, int timeoutMilliseconds = 30000)
        {
            var processStartInfo = new ProcessStartInfo("git", command)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _projectPath
            };

            var tcs = new TaskCompletionSource<string>();

            using (var process = new Process { StartInfo = processStartInfo })
            using (var cancellationToken = new CancellationTokenSource(timeoutMilliseconds))
            {
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) =>
                {
                    if (cancellationToken.Token.IsCancellationRequested)
                    {
                        tcs.TrySetResult("Command was cancelled due to timeout.");
                    }
                    else
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        tcs.TrySetResult(string.IsNullOrEmpty(output) ? error : output);
                    }
                    process.Close();
                };

                cancellationTokenSource.Token.Register(() =>
                {
                    try
                    {
                        cancellationToken?.Cancel();
                        cancellationToken?.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignored
                    }
                });

                cancellationToken.Token.Register(() =>
                {
                    try
                    {
                        process.Kill();
                        Debug.Log("Git command was killed.");
                    }
                    catch (InvalidOperationException)
                    {
                        // ignored
                    }
                });

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

                return await tcs.Task;
            }
        }

        private async Task UpdateGitInfoAsync(EditorWindow window = null)
        {
            _isFetching = true;
            try
            {
                await UpdateCommitHash();
                await UpdateCurrentBranch();
                await UpdateCommitsBehind();
            }
            finally
            {
                _isFetching = false;
                if (window != null)
                    window.Repaint();
            }
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
        
        
        /// <summary>
        /// Cancels any ongoing operations and disposes of resources.
        /// </summary>
        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        
        /// <summary>
        /// Clears all Git information from the assistant.
        /// </summary>
        public void Clear()
        {
            _gitAvailable = false;
            _isFetching = false;
            _commitShortHash = string.Empty;
            _commitFullHash = string.Empty;
            _currentBranch = string.Empty;
            _commitsBehind = string.Empty;
            _fetchingText = string.Empty;
        }
    }
}
