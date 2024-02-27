using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.DistributeToFirebase.Editor
{
    [PluginOrder(3, "Distribute/Distribute Apk To Firebase Plugin")]
    public class DistributeApkToFirebasePluginEditor : IBuildPluginEditor
    {
        private BuildDataProvider buildDataProvider;
        private DistributeApkData distributeApkData;
        private DistributeData distributeData;
        private DistributeLogger distributeLogger;

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

        public void InvokeSetupPlugin(BuildDataProvider buildDataProvider)
        {
            this.buildDataProvider = buildDataProvider;

            testersGroup = new BuildValue<ETestersGroup>();
            uploadAutomatically = new BuildValue<bool>();
            useGitLogger = new BuildValue<bool>();
            
            cancellationTokenSource = new CancellationTokenSource();

            SaveLoadData();

            testersGroup.OnValueChanged += TestersGroupOnOnValueChanged;
            uploadAutomatically.OnValueChanged += UploadAutomaticallyOnOnValueChanged;
            useGitLogger.OnValueChanged += UseGitLoggerOnOnValueChanged;
            buildDataProvider.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;
            
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
            if (!buildDataProvider.BuildPreferencesData.GlobalDataStorage.TryGetPluginData<DistributeApkData>(
                    out distributeApkData))
            {
                distributeApkData = new DistributeApkData();

                if (!distributeApkData.TryGetData(buildDataProvider.SelectedBuildType.Value, out distributeData))
                    distributeData = distributeApkData.RegisterOrUpdateData(buildDataProvider.SelectedBuildType.Value,
                        new DistributeData() { BuildType = buildDataProvider.SelectedBuildType.Value });

                distributeApkData =
                    buildDataProvider.BuildPreferencesData.GlobalDataStorage.SaveOrUpdatePluginData(distributeApkData);
            }
            
            distributeLogger = distributeApkData.DistributeLogger;

            if (distributeApkData.TryGetData(buildDataProvider.SelectedBuildType.Value, out distributeData))
            {
                testersGroup.Value = distributeData.TestersGroup;
                uploadAutomatically.Value = distributeData.UploadAutomatically;
                useGitLogger.Value = distributeData.UseGitLogger;
            }
        }

        private void SelectedBuildTypeOnOnValueChanged(EBuildType obj)
        {
            if (!distributeApkData.TryGetData(obj, out distributeData))
                distributeData = distributeApkData.RegisterOrUpdateData(obj, new DistributeData() { BuildType = obj });
                
            testersGroup.Value = distributeData.TestersGroup;
            uploadAutomatically.Value = distributeData.UploadAutomatically;
            useGitLogger.Value = distributeData.UseGitLogger;
        }

        private void UploadAutomaticallyOnOnValueChanged(bool obj)
        {
            if (distributeApkData.TryGetData(buildDataProvider.SelectedBuildType.Value, out var data))
            {
                data.BuildType = buildDataProvider.SelectedBuildType.Value;
                data.UploadAutomatically = uploadAutomatically.Value;

                distributeApkData.RegisterOrUpdateData(buildDataProvider.SelectedBuildType.Value, data);
            }

            buildDataProvider.BuildPreferencesData.GlobalDataStorage.SaveOrUpdatePluginData(distributeApkData);
        }

        private void TestersGroupOnOnValueChanged(ETestersGroup obj)
        {
            if (distributeApkData.TryGetData(buildDataProvider.SelectedBuildType.Value, out var data))
            {
                data.TestersGroup = testersGroup.Value;
                data.DistributionGroups = GetGroupsCommandPart(testersGroup.Value);

                distributeApkData.RegisterOrUpdateData(buildDataProvider.SelectedBuildType.Value, data);
            }

            buildDataProvider.BuildPreferencesData.GlobalDataStorage.SaveOrUpdatePluginData(distributeApkData);
        }
        
        private void UseGitLoggerOnOnValueChanged(bool obj)
        {
            if (distributeApkData.TryGetData(buildDataProvider.SelectedBuildType.Value, out var data))
            {
                data.UseGitLogger = obj;

                distributeApkData.RegisterOrUpdateData(buildDataProvider.SelectedBuildType.Value, data);
            }

            buildDataProvider.BuildPreferencesData.GlobalDataStorage.SaveOrUpdatePluginData(distributeApkData);
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
                if (!string.IsNullOrEmpty(buildDataProvider.BuildPath))
                {
                    buildPath = buildDataProvider.BuildPath;
                    
                    GUILayout.Label("Build path: " + buildDataProvider.BuildPath);
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

                if (distributeLogger.HasData)
                {
                    if (GUILayout.Button("Save Notes", EditorStyles.miniButton))
                        distributeLogger.SaveToTextFile();
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
            menu.AddItem(new GUIContent("Get All Notes"), false, () => distributeLogger.TryGetNotes(tag, out releaseNotes));

            menu.ShowAsContext();
        }
        
        private async void GetCommitsWithMessageTag(string tag)
        {
            processing = true;
            releaseNotes = string.Empty;
            var notes = await GetCommitsWithMessageTagAsync(tag);
            
            distributeLogger.AddOrUpdateData(tag, notes, out releaseNotes);
            
            buildDataProvider.BuildPreferencesData.GlobalDataStorage.SaveOrUpdatePluginData(distributeApkData);
            processing = false;
        }
        
        private async Task<string> GetCommitsWithMessageTagAsync(string tag)
        {
            string command = $"log --grep=\"{tag}\" -i --pretty=format:\"%h - %s\"";
            return await buildDataProvider.GitAssistant.ExecuteGitCommandAsync(command);
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
            if (string.IsNullOrEmpty(distributeData.DistributionGroups)) return;
            
            var notes = releaseNotes.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n");

            string command =
                $"firebase appdistribution:distribute \"{buildPath}\" --app \"{firebaseAppId}\" --release-notes \"{notes}\" --groups {distributeData.DistributionGroups} --token \"{firebaseToken}\"";

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
                    tokenSource?.Dispose();
                };

                cancellationTokenSource.Token.Register(() =>
                {
                    tokenSource?.Cancel();
                    tokenSource?.Dispose();
                });
                
                tokenSource.Token.Register(() =>
                {
                    process.Kill();
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
            buildDataProvider.SelectedBuildType.OnValueChanged -= SelectedBuildTypeOnOnValueChanged;
            
            EditorApplication.update -= AnimateLoadingText;
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            
            distributeApkData = null;
            distributeData = null;
            testersGroup = null;
            uploadAutomatically = null;
            distributeLogger = null;

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