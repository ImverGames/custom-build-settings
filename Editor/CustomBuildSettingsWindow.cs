using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImverGames.BuildIncrementor.Editor;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace ImverGames.CustomBuildSettings.Editor
{
    public class CustomBuildSettingsWindow : EditorWindow
    {
        private CustomBuildPreferencesWindow customBuildPreferences;
        private BuildDataProvider buildDataProvider;
        private CustomBuildReport customBuildReport;
        private CustomBuildReportsWindow customBuildReportsWindow;

        private GitAssistant gitAssistant;
        private GlobalDataStorage globalDataStorage;
        
        private Vector2 pageScrollPosition;
        private Vector2 sceneScrollPosition;

        private ReorderableList reorderableList;
        private List<EditorBuildSettingsScene> scenes;

        private bool formatChange;
        private bool expandChange;

        private GUIStyle centeredLabelStyle;

        [MenuItem("File/Custom Build Settings #b")]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomBuildSettingsWindow>("Custom Build Settings");
            window.minSize = new Vector2(600, 300);
        }

        private void OnEnable()
        {
            customBuildPreferences = new CustomBuildPreferencesWindow();
            buildDataProvider = new BuildDataProvider();
            customBuildReport = new CustomBuildReport();
            customBuildReportsWindow = new CustomBuildReportsWindow();

            gitAssistant = buildDataProvider.GitAssistant;
            
            customBuildReportsWindow.Initialize(customBuildReport);
            customBuildPreferences.Initialize(buildDataProvider.BuildPreferencesData);

            globalDataStorage = buildDataProvider.BuildPreferencesData.GlobalDataStorage;
            
            /*globalDataStorage.editorPlugins = InterfaceImplementationsInvoker.FindAllPluginsEditor<IBuildPluginEditor>();
            globalDataStorage.editorPlugins = InterfaceImplementationsInvoker.GetOrderedPlugins<IBuildPluginEditor>(globalDataStorage.editorPlugins);*/
            
            LoadScenes();
            SetupReorderableList();

            //var globalDataStorage = buildDataProvider.BuildPreferencesData.GlobalDataStorage;

            if (!globalDataStorage.TryGetPluginData<CustomBuildData>(out var buildData))
                buildData = globalDataStorage.SaveOrUpdatePluginData(new CustomBuildData(buildDataProvider.SelectedBuildType.Value));
            
            buildDataProvider.Version.Value = buildData.GetBuildVersion(buildData.BuildType);
            buildDataProvider.VersionTag.Value = buildData.GetBuildVersionTag(buildData.BuildType);
            buildDataProvider.VersionMeta.Value = buildData.GetBuildVersionMeta(buildData.BuildType);
            buildDataProvider.VersionFormat.Value = GetFormatTypeFromString(buildDataProvider.Version.Value);
            buildDataProvider.SelectedBuildType.Value = buildData.BuildType;
            
            buildDataProvider.SelectedBuildType.OnValueChanged += OnChangeBuildTypeSettings;
            buildDataProvider.Version.OnValueChanged += OnChangeVersion;
            buildDataProvider.VersionFormat.OnValueChanged += OnChangeVersionFormat;

            EnablePlugins();
            
            buildDataProvider.GitAssistant.CheckAndUpdateGitInfo(this);
            
            EditorApplication.update += OnUpdate;
        }

        private void OnFocus()
        {
            if (buildDataProvider != null)
            {
                buildDataProvider.GitAssistant.CheckAndUpdateGitInfo(this);
            
                var globalDataStorage = buildDataProvider.BuildPreferencesData.GlobalDataStorage;
                globalDataStorage.TryGetPluginData<CustomBuildData>(out var buildData);
                
                buildDataProvider.Version.Value = buildData.GetBuildVersion(buildData.BuildType);
                buildDataProvider.VersionTag.Value = buildData.GetBuildVersionTag(buildData.BuildType);
                buildDataProvider.VersionMeta.Value = buildData.GetBuildVersionMeta(buildData.BuildType);
                buildDataProvider.VersionFormat.Value = GetFormatTypeFromString(buildDataProvider.Version.Value);
                buildDataProvider.SelectedBuildType.Value = buildData.BuildType;
            }

            if (globalDataStorage.editorPlugins != null)
            {
                foreach (var editorPlugin in globalDataStorage.editorPlugins)
                    editorPlugin.BuildPluginEditor.InvokeOnFocusPlugin();
            }
            
            if(customBuildReportsWindow != null)
                customBuildReportsWindow.Initialize(customBuildReport);
            else
            {
                customBuildReportsWindow = new CustomBuildReportsWindow();
                customBuildReportsWindow.Initialize(customBuildReport);
            }
        }

        private void EnablePlugins()
        {
            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeSetupPlugin(buildDataProvider);
        }

        #region BuildVersion Formating

        public static EVersionFormatType GetFormatTypeFromString(string versionString)
        {
            var parts = versionString.Split('.');

            if (parts.Length != 3 || string.IsNullOrEmpty(versionString)) return default;

            int length1 = parts[0].Length;
            int length2 = parts[1].Length;
            int length3 = parts[2].Length;

            if (length1 == 1 && length2 == 1 && length3 == 1) return EVersionFormatType.D1_D1_D1;
            if (length1 == 1 && length2 == 1 && length3 == 2) return EVersionFormatType.D1_D1_D2;
            if (length1 == 1 && length2 == 1 && length3 == 3) return EVersionFormatType.D1_D1_D3;
            if (length1 == 1 && length2 == 1 && length3 == 3) return EVersionFormatType.D1_D2_D2;
            if (length1 == 1 && length2 == 2 && length3 == 3) return EVersionFormatType.D1_D2_D3;
            if (length1 == 1 && length2 == 3 && length3 == 3) return EVersionFormatType.D1_D3_D3;
            if (length1 == 2 && length2 == 2 && length3 == 3) return EVersionFormatType.D2_D2_D3;
            if (length1 == 2 && length2 == 3 && length3 == 3) return EVersionFormatType.D2_D3_D3;
            
            return default;
        }
        
        public static string ConvertVersionFormat(string version, EVersionFormatType newFormat)
        {
            var parts = version.Split('.');
            
            if (parts.Length != 3) return version;

            int part1 = int.Parse(parts[0]);
            int part2 = int.Parse(parts[1]);
            int part3 = int.Parse(parts[2]);

            switch (newFormat)
            {
                case EVersionFormatType.D1_D1_D1:
                    return $"{part1:D1}.{part2:D1}.{part3:D1}";
                case EVersionFormatType.D1_D1_D2:
                    return $"{part1:D1}.{part2:D1}.{part3:D2}";
                case EVersionFormatType.D1_D1_D3:
                    return $"{part1:D1}.{part2:D1}.{part3:D3}";
                case EVersionFormatType.D1_D2_D2:
                    return $"{part1:D1}.{part2:D2}.{part3:D2}";
                case EVersionFormatType.D1_D2_D3:
                    return $"{part1:D1}.{part2:D2}.{part3:D3}";
                case EVersionFormatType.D1_D3_D3:
                    return $"{part1:D1}.{part2:D3}.{part3:D3}";
                case EVersionFormatType.D2_D2_D3:
                    return $"{part1:D2}.{part2:D2}.{part3:D3}";
                case EVersionFormatType.D2_D3_D3:
                    return $"{part1:D2}.{part2:D3}.{part3:D3}";
                
                default:
                    return version;
            }
        }

        #endregion

        private void InitStyles()
        {
            centeredLabelStyle = GUI.skin.GetStyle("Label");
            centeredLabelStyle.alignment = TextAnchor.UpperCenter;
            centeredLabelStyle.fontStyle = FontStyle.Bold;
        }

        void OnGUI()
        {
            InitStyles();
            
            DrawGitInfo();

            #region Window scroll
            
            pageScrollPosition = 
                GUILayout.BeginScrollView(
                    pageScrollPosition,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));   //----------------------- Begin Window scroll -----------------------

            #region Window

            GUILayout.BeginVertical(GUI.skin.box);  //----------------------- Window Vertical -----------------------

            GUILayout.Space(10);
            
            Event evt = Event.current;
            
            #region Scene list
            
            DrawSceneManagement(evt);

            #endregion

            GUILayout.Space(10);

            #region Build option
            
            DrawBuildOption();

            #endregion

            DrawPluginPage();

            GUILayout.EndVertical();  //----------------------- End Window Vertical -----------------------
            
            #endregion
            
            GUILayout.EndScrollView();   //----------------------- End Window scroll -----------------------
            
            #endregion

            GUILayout.Space(10);

            #region Control Buttons

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();  //----------------------- Control Buttons Horizontal -----------------------

            if (GUILayout.Button("Open Player Settings", GUILayout.ExpandWidth(false)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            if (!EditorUserBuildSettings.development)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show build reports"))
                    customBuildReportsWindow.ShowCustomBuildReport();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build Game", EditorStyles.popup))
            {
                ShowDropdownMenu();
            }
            
            GUILayout.EndHorizontal();  //----------------------- End Control Buttons Horizontal -----------------------
            
            #endregion
        }
        
        #region Scenes management

        private void LoadScenes()
        {
            scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        }

        private void SetupReorderableList()
        {
            reorderableList = new ReorderableList(scenes, typeof(EditorBuildSettingsScene), true, true, false, true);

            reorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Scenes In Build:"); };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorBuildSettingsScene scene = scenes[index];
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                var checkBoxWidth = 20;
                bool newEnabledValue = EditorGUI.Toggle(new Rect(rect.x, rect.y, checkBoxWidth - 2, rect.height), scene.enabled);
                if (newEnabledValue != scene.enabled)
                {
                    scene.enabled = newEnabledValue;
                    UpdateBuildSettingsScenes();
                }

                float copyButtonWidth = 15;
                float labelWidth = rect.width - 20 - copyButtonWidth;
                EditorGUI.LabelField(new Rect(rect.x + checkBoxWidth, rect.y, labelWidth, rect.height), scene.path);

                var copyButtonContent = new GUIContent("⁝", "Copy path");
                
                if (GUI.Button(new Rect(rect.x + checkBoxWidth + labelWidth, rect.y, copyButtonWidth, rect.height), copyButtonContent))
                {
                    EditorGUIUtility.systemCopyBuffer = scene.path;
                }
            };

            reorderableList.onChangedCallback = (ReorderableList list) =>
            {
                UpdateBuildSettingsScenes();
            };

            reorderableList.onAddCallback = (ReorderableList list) => { AddOpenScenes(); };

            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                scenes.RemoveAt(list.index);
                UpdateBuildSettingsScenes();
            };
        }
        
        private void AddOpenScenes()
        {
            foreach (var scene in EditorSceneManager.GetActiveScene().GetRootGameObjects().Select(go => go.scene.path)
                         .Distinct())
            {
                if (!string.IsNullOrEmpty(scene) && !scenes.Any(s => s.path == scene))
                {
                    scenes.Add(new EditorBuildSettingsScene(scene, true));
                }
            }

            UpdateBuildSettingsScenes();
        }
        
        private void UpdateBuildSettingsScenes()
        {
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        #endregion

        #region OnGui Drawers section

        private void DrawDropSceneArea(Event evt)
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, 25f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag scene here to add to list");
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is SceneAsset)
                            {
                                string path = AssetDatabase.GetAssetPath(draggedObject);
                                if (!scenes.Any(s => s.path == path))
                                {
                                    scenes.Add(new EditorBuildSettingsScene(path, true));
                                    UpdateBuildSettingsScenes();
                                }
                            }
                        }
                    }

                    break;
            }
        }
        private void DrawSceneManagement(Event evt)
        {
            DrawDropSceneArea(evt);

            sceneScrollPosition =
                GUILayout.BeginScrollView(sceneScrollPosition, GUILayout.ExpandWidth(true),
                    GUILayout.Height(200)); //----------------------- Begin Scene list scroll -----------------------
            reorderableList.DoLayoutList();
            GUILayout.EndScrollView(); //----------------------- End Scene list scroll -----------------------

            if (GUILayout.Button("Add Open Scenes"))
            {
                AddOpenScenes();
            }
        }

        private void DrawBuildOption()
        {
            GUILayout.BeginVertical(EditorStyles
                .helpBox); //----------------------- Build option Vertical -----------------------
            GUILayout.Label("Build Options", centeredLabelStyle);

            GUILayout.Space(10);

            #region Build Type Management

            GUILayout.BeginHorizontal(); //----------------------- Build Type Management Horizontal -----------------------
            buildDataProvider.SelectedBuildType.Value =
                (EBuildType)EditorGUILayout.EnumPopup("Build Type", buildDataProvider.SelectedBuildType.Value);

            GUILayout.Label($"Platform: {EditorUserBuildSettings.activeBuildTarget}", EditorStyles.boldLabel);
            GUILayout.EndHorizontal(); //----------------------- Build Type Management Horizontal -----------------------

            #endregion

            #region Build Version Management

            DrawBuildVersionManagement();

            #endregion

            GUILayout.EndVertical(); //----------------------- End Build option Vertical -----------------------
        }

        private void DrawBuildVersionManagement()
        {
            GUILayout.BeginHorizontal(); //----------------------- Build Version Management Horizontal -----------------------

            EditorGUI.BeginChangeCheck();

            buildDataProvider.Version.Value = EditorGUILayout.TextField("Build Version:", buildDataProvider.Version.Value);

            GUILayout.Label("-", GUILayout.Width(10));

            buildDataProvider.VersionTag.Value =
                EditorGUILayout.TextField(buildDataProvider.VersionTag.Value, GUILayout.Width(30));

            DrawVersionGitMeta();

            //buildIncrementorData.VersionFormat.Value = (EVersionFormatType)EditorGUILayout.EnumPopup(buildIncrementorData.VersionFormat.Value);

            GUILayout.Space(10);
            gitAssistant.addHeshToVersion = EditorGUILayout.Toggle("Add commit hash", gitAssistant.addHeshToVersion);

            if (EditorGUI.EndChangeCheck())
                formatChange = true;

            if (formatChange)
            {
                if (GUILayout.Button("Save Format"))
                {
                    var globalDataStorage = buildDataProvider.BuildPreferencesData.GlobalDataStorage;

                    if (globalDataStorage.TryGetPluginData<CustomBuildData>(out var buildData))
                    {
                        buildData.BuildType = buildDataProvider.SelectedBuildType.Value;

                        buildData.RegisterOrUpdateVersion(
                            buildDataProvider.SelectedBuildType.Value,
                            buildDataProvider.Version.Value,
                            buildDataProvider.VersionTag.Value,
                            buildDataProvider.VersionMeta.Value);

                        globalDataStorage.SaveOrUpdatePluginData(buildData);

                        PlayerSettings.bundleVersion = buildData.GetFullBuildVersion(buildData.BuildType);
                    }

                    formatChange = false;
                }
            }

            GUILayout.EndHorizontal(); //----------------------- Build Version Management Horizontal -----------------------
        }

        private void DrawVersionGitMeta()
        {
            if (gitAssistant.gitAvailable && gitAssistant.addHeshToVersion)
            {
                GUILayout.Label(".", GUILayout.Width(8));

                buildDataProvider.VersionMeta.Value = gitAssistant.commitShortHash;

                GUILayout.Label(new GUIContent(gitAssistant.commitShortHash), GUILayout.Width(50));
            }
            else if (gitAssistant.gitAvailable && !gitAssistant.addHeshToVersion)
            {
                buildDataProvider.VersionMeta.Value = string.Empty;
            }
        }

        private void DrawGitInfo()
        {
            if (gitAssistant.gitAvailable)
            {
                if (gitAssistant.isFetching)
                {
                    GUILayout.Label(gitAssistant.fetchingText, centeredLabelStyle);
                }
                else
                {
                    GUILayout.Label(
                        "Branch: " + gitAssistant.currentBranch + " | " + "↑↓ " + gitAssistant.commitsBehind + " | " + "#" + gitAssistant.commitShortHash,
                        centeredLabelStyle);
                }
            }
        }

        private void ShowDropdownMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Build"), false, () => BuildGame(BuildOptions.ShowBuiltPlayer));
            menu.AddItem(new GUIContent("Clean Build"), false, () => BuildGame(BuildOptions.ShowBuiltPlayer | BuildOptions.CleanBuildCache));

            menu.ShowAsContext();
        }
        
        void DrawUILine(Color color, int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private void DrawPluginPage()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Plugins", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            for (int i = 0; i < globalDataStorage.editorPlugins.Count; i++)
            {
                GUILayout.Space(5);
                DrawUILine(Color.gray);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal("Box");
                
                EditorGUI.BeginChangeCheck();
                globalDataStorage.editorPlugins[i].Expand = EditorGUILayout.Foldout(
                    globalDataStorage.editorPlugins[i].Expand,
                    $"{globalDataStorage.editorPlugins[i].BuildPluginEditor.GetType().Name}", true);
                expandChange = EditorGUI.EndChangeCheck();

                if (expandChange)
                {
                    EditorUtility.SetDirty(globalDataStorage);
                    
                    expandChange = false;
                }

                if (i > 0)
                {
                    if (GUILayout.Button("↑", GUILayout.MaxWidth(30)))
                    {
                        EditorUtility.SetDirty(globalDataStorage);
                        
                        var item = globalDataStorage.editorPlugins[i];
                        globalDataStorage.editorPlugins.RemoveAt(i);
                        globalDataStorage.editorPlugins.Insert(i - 1, item);
                    }
                }
                else
                {
                    GUILayout.Space(34);
                }

                if (i < globalDataStorage.editorPlugins.Count - 1)
                {
                    if (GUILayout.Button("↓", GUILayout.MaxWidth(30)))
                    {
                        EditorUtility.SetDirty(globalDataStorage);
                        
                        var item = globalDataStorage.editorPlugins[i];
                        globalDataStorage.editorPlugins.RemoveAt(i);
                        globalDataStorage.editorPlugins.Insert(i + 1, item);
                    }
                }
                else
                {
                    GUILayout.Space(34);
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
                {
                    EditorUtility.SetDirty(globalDataStorage);
                    
                    globalDataStorage.editorPlugins[i].BuildPluginEditor.InvokeDestroyPlugin();
                    
                    globalDataStorage.editorPlugins.RemoveAt(i);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.EndVertical();

                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (globalDataStorage.editorPlugins[i].Expand)
                {
                    globalDataStorage.editorPlugins[i].BuildPluginEditor.InvokeGUIPlugin();
                }
                
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add Custom Plugin"))
            {
                ShowAddPluginMenu();
            }

            /*foreach (var editorPlugin in globalDataStorage.editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeGUIPlugin", null);*/
            
            GUILayout.Space(10);
        }
        
        private void ShowAddPluginMenu()
        {
            var menu = new GenericMenu();
            
            Type interfaceType = typeof(IBuildPluginEditor);
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        var attr = type.GetCustomAttribute<PluginOrderAttribute>();
                        
                        string menuItemName = attr != null && !string.IsNullOrEmpty(attr.NamePath) ? attr.NamePath : type.Name;
                        menu.AddItem(new GUIContent(menuItemName), false, () => AddPlugin(type));
                    }
                }
            }

            menu.ShowAsContext();
        }
        
        private void AddPlugin(Type pluginType)
        {
            if (globalDataStorage.editorPlugins.Exists(p => p.BuildPluginEditor.GetType() == pluginType))
                return;
            
            if (Activator.CreateInstance(pluginType) is IBuildPluginEditor pluginInstance)
            {
                EditorUtility.SetDirty(globalDataStorage);
                    
                pluginInstance.InvokeSetupPlugin(buildDataProvider);
                
                globalDataStorage.editorPlugins.Add(new PluginsStorage() {BuildPluginEditor = pluginInstance, Expand = true});
            }
        }

        #endregion

        #region OValueChange setion

        private void OnChangeVersion(string version)
        {
            buildDataProvider.VersionFormat.Value = GetFormatTypeFromString(buildDataProvider.Version.Value);
        }
        
        private void OnChangeVersionFormat(EVersionFormatType eVersionFormatType)
        {
            buildDataProvider.Version.Value = ConvertVersionFormat(buildDataProvider.Version.Value, eVersionFormatType);
        }

        private void OnChangeBuildTypeSettings(EBuildType eBuildType)
        {
            switch (buildDataProvider.SelectedBuildType.Value)
            {
                case EBuildType.RELEASE:
                    gitAssistant.addHeshToVersion = false;
                    break;
                case EBuildType.MILESTONE:
                    gitAssistant.addHeshToVersion = false;
                    break;
                case EBuildType.DAILY:
                    gitAssistant.addHeshToVersion = false;
                    break;
                case EBuildType.DEVELOPMENT:
                    gitAssistant.addHeshToVersion = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var globalDataStorage = buildDataProvider.BuildPreferencesData.GlobalDataStorage;

            if (globalDataStorage.TryGetPluginData<CustomBuildData>(out var buildData))
            {
                buildDataProvider.Version.Value = buildData.GetBuildVersion(eBuildType);
                buildDataProvider.VersionTag.Value = buildData.GetBuildVersionTag(eBuildType);
                buildDataProvider.VersionMeta.Value = buildData.GetBuildVersionMeta(eBuildType);
                
                buildData.BuildType = eBuildType;
                        
                buildData.RegisterOrUpdateVersion(
                    eBuildType,
                    buildDataProvider.Version.Value,
                    buildDataProvider.VersionTag.Value, 
                    buildDataProvider.VersionMeta.Value);

                globalDataStorage.SaveOrUpdatePluginData(buildData);
                
                PlayerSettings.bundleVersion = buildData.GetFullBuildVersion(eBuildType);
            }
        }

        #endregion

        #region Build section
        
        private void InvokePluginBeforeBuild()
        {
            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeBeforeBuild();
        }
        
        private void InvokePluginAfterBuild()
        {
            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeAfterBuild();
        }

        private void BuildGame(BuildOptions options)
        {
            string extension = GetExtensionForTarget(EditorUserBuildSettings.activeBuildTarget);
            string defaultName = $"{Application.productName}_{buildDataProvider.SelectedBuildType.Value}{extension}";
            string path = EditorUtility.SaveFilePanel("Choose Location and Name for Build", "", defaultName, extension);

            if (string.IsNullOrEmpty(path)) return;

            InvokePluginBeforeBuild();
            
            IncrementBuildVersion();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = options,
            };

            if (EditorUserBuildSettings.development)
            {
                buildPlayerOptions.options |= BuildOptions.Development;
            }

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            customBuildReportsWindow.SetLastBuildReport(report);

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes at path {summary.outputPath}");

                InvokePluginAfterBuild();
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }

        private void IncrementBuildVersion(bool refresh = false)
        {
            /*BuildTypeVersionIncrementor.IncrementVersion(buildIncrementorData, out var buildData,
                buildIncrementorData.Version.Value,
                Enum.GetName(typeof(EVersionFormatType), buildIncrementorData.VersionFormat.Value));

            buildIncrementorData.Version.Value = buildData.GetBuildVersion(buildIncrementorData.SelectedBuildType.Value);

            if (refresh)
            {
                AssetDatabase.Refresh();
            }*/
        }

        private string GetExtensionForTarget(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.StandaloneOSX:
                    return ".app";
                case BuildTarget.StandaloneLinux64:
                    return "";
                case BuildTarget.iOS:
                    return "";
                case BuildTarget.Android:
                    return EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                case BuildTarget.WebGL:
                    return "";
                case BuildTarget.WSAPlayer:
                    return ".appx";
                default:
                    return "";
            }
        }
        
        #endregion
        
        private void OnUpdate()
        {
            if (gitAssistant.gitAvailable && gitAssistant.isFetching)
            {
                gitAssistant.AnimateLoadingText();
                Repaint();
            }
        }

        private void OnLostFocus()
        {
            AssetDatabase.SaveAssetIfDirty(globalDataStorage);
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssetIfDirty(globalDataStorage);
            
            buildDataProvider.SelectedBuildType.OnValueChanged -= OnChangeBuildTypeSettings;
            buildDataProvider.Version.OnValueChanged -= OnChangeVersion;
            buildDataProvider.VersionFormat.OnValueChanged -= OnChangeVersionFormat;
            
            EditorApplication.update -= OnUpdate;

            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeDestroyPlugin();

            gitAssistant = null;
            buildDataProvider = null;
            reorderableList = null;
            scenes = null;
            
            if(customBuildReportsWindow != null)
                DestroyImmediate(customBuildReportsWindow);
        }
    }
}
