using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase.Editor
{
    [PluginOrder(3, "Distribute/Distribute Apk To Firebase Plugin")]
    public class DistributeApkToFirebasePluginEditor : IBuildPluginEditor
    {
        private GlobalDataStorage globalDataStorage;
        private MainBuildData mainBuildData;
        private GitAssistant gitAssistant;
        private DistributeToFirebasePluginData distributeToFirebasePluginData;
        private DistributeNotesGitLogger distributeNotesGitLogger;

        private BuildValue<ETestersGroup> testersGroup;
        private BuildValue<bool> uploadAutomatically;
        private BuildValue<bool> useGitLogger;

        private string firebaseAppId = string.Empty;
        private string firebaseToken = string.Empty;
        private string releaseNotes = string.Empty;
        private string buildPath = string.Empty;
        
        private string outputText = string.Empty;
        private MessageType messageType;

        private Vector2 scrollPosition;

        private bool processing = false;
        private bool available = false;
        private bool executingСommandFirebase = false;
        private string loadingText = string.Empty;
        private string gitTag = string.Empty;
        
        private DateTime lastUpdateTime;
        private int dotCount;
        
        private CancellationTokenSource cancellationTokenSource;

        public void InvokeSetupPlugin()
        {
            mainBuildData = DataBinder.GetData<MainBuildData>();
            globalDataStorage = DataBinder.GetData<GlobalDataStorage>();
            gitAssistant = DataBinder.GetData<GitAssistant>();

            testersGroup = new BuildValue<ETestersGroup>();
            uploadAutomatically = new BuildValue<bool>();
            useGitLogger = new BuildValue<bool>();
            
            cancellationTokenSource = new CancellationTokenSource();

            SaveLoadData();

            testersGroup.OnValueChanged += TestersGroupOnOnValueChanged;
            uploadAutomatically.OnValueChanged += UploadAutomaticallyOnOnValueChanged;
            useGitLogger.OnValueChanged += UseGitLoggerOnOnValueChanged;
            mainBuildData.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;
            
            EditorApplication.update += AnimateLoadingText;

            CheckFirebase();
        }

        private async void CheckFirebase()
        {
            var output = await ExecuteFirebaseCommandAsync("firebase --version");
            OutputMessage(output.OutputMessage, output.MessageType);
            available = output.IsSuccess;
        }

        private void SaveLoadData()
        {
            if (!globalDataStorage.TryGetPluginData<DistributeToFirebasePluginData>(GetType(), mainBuildData.SelectedBuildType.Value, out distributeToFirebasePluginData))
                distributeToFirebasePluginData = globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value,
                    new DistributeToFirebasePluginData());
            
            if (globalDataStorage.TryGetStorage(GetType(), out var storage))
                distributeNotesGitLogger = storage.SharedBuildTypePluginData as DistributeNotesGitLogger ?? new DistributeNotesGitLogger();
            
            globalDataStorage.RegisterOrUpdateStorage(GetType(), storage);

            testersGroup.Value = distributeToFirebasePluginData.TestersGroup;
            uploadAutomatically.Value = distributeToFirebasePluginData.UploadAutomatically;
            useGitLogger.Value = distributeToFirebasePluginData.UseGitLogger;
        }

        private void SelectedBuildTypeOnOnValueChanged(EBuildType obj)
        {
            if (!globalDataStorage.TryGetPluginData<DistributeToFirebasePluginData>(this.GetType(), mainBuildData.SelectedBuildType.Value, out distributeToFirebasePluginData))
                distributeToFirebasePluginData = globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value,
                    new DistributeToFirebasePluginData());
                
            testersGroup.Value = distributeToFirebasePluginData.TestersGroup;
            uploadAutomatically.Value = distributeToFirebasePluginData.UploadAutomatically;
            useGitLogger.Value = distributeToFirebasePluginData.UseGitLogger;
        }

        private void UploadAutomaticallyOnOnValueChanged(bool obj)
        {
            if (globalDataStorage.TryGetPluginData<DistributeToFirebasePluginData>(this.GetType(), mainBuildData.SelectedBuildType.Value, out var data))
            {
                data.UploadAutomatically = uploadAutomatically.Value;
                
                globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value, data);
            }
        }

        private void TestersGroupOnOnValueChanged(ETestersGroup obj)
        {
            if (globalDataStorage.TryGetPluginData<DistributeToFirebasePluginData>(this.GetType(), mainBuildData.SelectedBuildType.Value, out var data))
            {
                data.TestersGroup = testersGroup.Value;
                data.DistributionGroups = GetGroupsCommandPart(testersGroup.Value);

                globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value, data);
            }
        }
        
        private void UseGitLoggerOnOnValueChanged(bool obj)
        {
            if (globalDataStorage.TryGetPluginData<DistributeToFirebasePluginData>(this.GetType(), mainBuildData.SelectedBuildType.Value, out var data))
            {
                data.UseGitLogger = obj;

                globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value, data);
            }
        }

        string GetGroupsCommandPart(ETestersGroup groups)
        {
            if (groups == ETestersGroup.None) return string.Empty;

            var groupsArray = groups.ToString().Split(',');
            for (int i = 0; i < groupsArray.Length; i++)
            {
                groupsArray[i] = groupsArray[i].Trim().ToLower().Replace("_", "-");
                groupsArray[i] = $"\"{groupsArray[i]}\"";
            }

            return string.Join(",", groupsArray);
        }

        public void InvokeOnFocusPlugin()
        {
            
        }

        public void AnimateLoadingText()
        {
            if (executingСommandFirebase && (DateTime.Now - lastUpdateTime).TotalSeconds > 0.5)
            {
                dotCount = (dotCount + 1) % 4;

                loadingText = "Executing Firebase CLI command Do Not Close" + new string('.', dotCount);
                lastUpdateTime = DateTime.Now;
            }
        }

        public void InvokeGUIPlugin()
        {
            if (executingСommandFirebase)
            {
                GUILayout.Label(loadingText, EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            if (!available)
            {
                loadingText = "Firebase CLI is not installed";
                GUILayout.Label(loadingText, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (!uploadAutomatically.Value)
            {
                if(GUILayout.Button("Brows build path"))
                    buildPath = EditorUtility.OpenFilePanel("Select build path", "", "apk");
                
                if (!string.IsNullOrEmpty(buildPath))
                    GUILayout.Label("Build path: " + buildPath);
            }
            else
            {
                if (!string.IsNullOrEmpty(mainBuildData.BuildPath))
                {
                    buildPath = mainBuildData.BuildPath;
                    
                    GUILayout.Label("Build path: " + mainBuildData.BuildPath);
                }
            }
            GUILayout.Space(10);

            uploadAutomatically.Value = EditorGUILayout.Toggle("Upload automatically", uploadAutomatically.Value);
            firebaseAppId = EditorGUILayout.TextField("Firebase App ID", firebaseAppId);
            firebaseToken = EditorGUILayout.TextField("Firebase Token", firebaseToken);
            testersGroup.Value = (ETestersGroup)EditorGUILayout.EnumFlagsField("Testers Group", testersGroup.Value);

            GUILayout.Space(10);
            
            useGitLogger.Value = EditorGUILayout.Toggle("Use Git Logger", useGitLogger.Value);

            if (useGitLogger.Value)
            {
                GUILayout.BeginHorizontal();

                gitTag = EditorGUILayout.TextField("Commit Tag:", gitTag);

                if (!processing)
                {
                    if (GUILayout.Button("Get Notes", EditorStyles.popup))
                        ShowDropdownMenu(gitTag);
                }
                else
                {
                    GUILayout.Label("Processing...", EditorStyles.centeredGreyMiniLabel);
                }

                if (GUILayout.Button("Clear Notes", EditorStyles.miniButton))
                    releaseNotes = string.Empty;

                if (distributeNotesGitLogger.HasData)
                {
                    if (GUILayout.Button("Save Notes", EditorStyles.miniButton))
                        distributeNotesGitLogger.SaveToTextFile();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Release notes:", EditorStyles.centeredGreyMiniLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox,GUILayout.Height(200));
            releaseNotes = EditorGUILayout.TextArea(releaseNotes, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            if (!uploadAutomatically.Value)
            {
                if(GUILayout.Button("Upload to Firebase"))
                    DistributeApkToFirebase();
            }
            
            GUILayout.Space(10);
            
            if(!string.IsNullOrEmpty(outputText))
                EditorGUILayout.HelpBox(outputText, messageType);
        }

        private void ShowDropdownMenu(string tag)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Get New Notes"), false, () => GetCommitsWithMessageTag(tag));
            menu.AddItem(new GUIContent("Get All Notes"), false, () => GetAllCommitsWithMessageTag(tag));

            menu.ShowAsContext();
        }
        
        private async void GetCommitsWithMessageTag(string tag)
        {
            processing = true;
            releaseNotes = string.Empty;
            var notes = await GetCommitsWithMessageTagAsync(tag);
            
            distributeNotesGitLogger.AddOrUpdateData(tag, notes, out releaseNotes);

            SaveStorage();

            processing = false;
        }

        private void SaveStorage()
        {
            if (globalDataStorage.TryGetStorage(GetType(), out var storage))
            {
                storage.SharedBuildTypePluginData = distributeNotesGitLogger;
                globalDataStorage.RegisterOrUpdateStorage(GetType(), storage);
            }
        }

        private async void GetAllCommitsWithMessageTag(string tag)
        {
            processing = true;
            releaseNotes = string.Empty;
            var notes = await GetCommitsWithMessageTagAsync(tag);
            
            distributeNotesGitLogger.AddOrUpdateData(tag, notes, out var newNotes);
            
            distributeNotesGitLogger.TryGetNotes(tag, out releaseNotes);

            SaveStorage();

            processing = false;
        }
        
        private async Task<string> GetCommitsWithMessageTagAsync(string tag)
        {
            string command = $"log --grep=\"{tag}\" -i --pretty=format:\"%h - %s\"";
            return await gitAssistant.ExecuteGitCommandAsync(command);
        }

        public void InvokeBeforeBuild()
        {
            
        }

        public void InvokeAfterBuild()
        {
            if(uploadAutomatically.Value)
                DistributeApkToFirebase();
        }

        private async void DistributeApkToFirebase()
        {
            if (string.IsNullOrEmpty(buildPath)) return;
            if (string.IsNullOrEmpty(firebaseAppId)) return;
            if (string.IsNullOrEmpty(firebaseToken)) return;
            if (string.IsNullOrEmpty(distributeToFirebasePluginData.DistributionGroups)) return;
            
            var notes = releaseNotes.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n");

            string command =
                $"firebase appdistribution:distribute \"{buildPath}\" --app \"{firebaseAppId}\" --release-notes \"{notes}\" --groups {distributeToFirebasePluginData.DistributionGroups} --token \"{firebaseToken}\"";

            var output = await ExecuteFirebaseCommandAsync(command);
            OutputMessage(output.OutputMessage, output.MessageType);
        }

        private async Task<FirebaseOutput> ExecuteFirebaseCommandAsync(string command, int timeoutMilliseconds = 300000)
        {
            executingСommandFirebase = true;
            
            FirebaseOutput result = new FirebaseOutput(false, string.Empty, MessageType.Error);

            string fileName;
            string arguments;

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                fileName = "cmd.exe";
                arguments = $"/c {command}";
            }
            else
            {
                fileName = "/bin/bash";
                arguments = $"-c \"{command}\"";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var tcs = new TaskCompletionSource<bool>();

            using (var process = new Process { StartInfo = startInfo })
            using (var tokenSource = new CancellationTokenSource(timeoutMilliseconds))
            {
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) =>
                {
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        tcs.TrySetResult(false);
                        
                        result.OutputMessage = "Command was cancelled due to timeout.";
                        result.MessageType = MessageType.Warning;
                        
                        OutputMessage(result.OutputMessage, result.MessageType);
                    }
                    else
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        int exitCode = process.ExitCode;

                        if (exitCode == 0)
                        {
                            var successMessage = "firebase --version"
                                .Equals(command) ? "Success: Firebase CLI is installed " + output
                                : "Success: " + output;

                            result.OutputMessage = successMessage;
                            result.MessageType = MessageType.Info;

                            tcs.TrySetResult(true);
                        }
                        else
                        {
                            result.OutputMessage = $"Error: {command} " + error;
                            result.MessageType = MessageType.Error;

                            tcs.TrySetResult(false);
                        }
                    }

                    process.Close();
                };

                cancellationTokenSource.Token.Register(() =>
                {
                    try
                    {
                        tokenSource?.Cancel();
                        tokenSource?.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignored
                    }
                });
                
                tokenSource.Token.Register(() =>
                {
                    try
                    {
                        process.Kill();
                        Debug.Log("Firebase command was killed.");
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
                
                var tcsResult = await tcs.Task;
                
                result.IsSuccess = tcsResult;
                
                executingСommandFirebase = false;

                return result;
            }
        }

        private void OutputMessage(string successMessage, MessageType outputMessageType)
        {
            outputText = successMessage;
            messageType = outputMessageType;
        }

        public void InvokeDestroyPlugin()
        {
            testersGroup.OnValueChanged -= TestersGroupOnOnValueChanged;
            uploadAutomatically.OnValueChanged -= UploadAutomaticallyOnOnValueChanged;
            useGitLogger.OnValueChanged -= UseGitLoggerOnOnValueChanged;
            mainBuildData.SelectedBuildType.OnValueChanged -= SelectedBuildTypeOnOnValueChanged;
            
            EditorApplication.update -= AnimateLoadingText;
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            
            distributeToFirebasePluginData = null;
            testersGroup = null;
            uploadAutomatically = null;
            distributeNotesGitLogger = null;

            firebaseAppId = string.Empty;
            firebaseToken = string.Empty;
            releaseNotes = string.Empty;
            buildPath = string.Empty;
            outputText = string.Empty;
        }
    }
    
    public struct FirebaseOutput
    {
        public bool IsSuccess;
        public string OutputMessage;
        public MessageType MessageType;

        public FirebaseOutput(bool isSuccess, string outputMessage, MessageType messageType)
        {
            IsSuccess = isSuccess;
            OutputMessage = outputMessage;
            MessageType = messageType;
        }
    }
}