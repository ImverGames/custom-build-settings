using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
using Random = UnityEngine.Random;

namespace ImverGames.CustomBuildSettings.Editor
{
    public class CustomBuildSettingsWindow : EditorWindow
    {
        private DataBinderFactory dataBinderFactory;

        private MainBuildData mainBuildData;
        private BuildPreferencesData buildPreferencesData;
        private CustomBuildReport customBuildReport;
        private GitAssistant gitAssistant;
        private GlobalDataStorage globalDataStorage;

        private Vector2 pageScrollPosition;
        private Vector2 sceneScrollPosition;

        private ReorderableList reorderableList;

        private bool formatChange;
        private bool expandChange;

        private GUIStyle centeredLabelStyle;
        private GUIStyle headerBoxStyle;
        private GUIStyle BGStyle;
        private GUIContent sceneDropGUIContent;
        private GUIContent copyIconGUIContent;
        private GUIContent gitIconGUIContent;
        private GUIContent gitPullIconGUIContent;
        private GUIContent gitCommitIconGUIContent;
        private GUIContent headerGUIContent;
        
        private float gitIconTextOffset = 0f;
        private float gitPullIconTextOffset = 0f;
        private float gitCommitIconTextOffset = 0f;
        
        private double time = 0;
        private Color lineColor;

        [MenuItem("File/Custom Build Settings &b")]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomBuildSettingsWindow>("Custom Build Settings");
            window.minSize = new Vector2(600, 300);
        }

        private async void OnEnable()
        {
            InitializeComponents();
            RegisterDataBinders();

            await UpdateBuildDataAndUI();
            
            SetupReorderableList();

            SubscribeToEvents();

            EnablePlugins();

            InitializeGUIContents();

            await gitAssistant.CheckAndUpdateGitInfo(this);
        }

        private async void OnFocus()
        {
            if (mainBuildData != null)
                await UpdateBuildDataAndUI();

            InvokeEditorPluginsOnFocus();

            if(gitAssistant != null)
                await gitAssistant.CheckAndUpdateGitInfo(this);
        }

        private void InitializeComponents()
        {
            dataBinderFactory = new DataBinderFactory();
            mainBuildData = new MainBuildData();
            customBuildReport = new CustomBuildReport();
            buildPreferencesData = new BuildPreferencesData();
            gitAssistant = new GitAssistant();
        }

        private void RegisterDataBinders()
        {
            dataBinderFactory.CreateDataBinder();
            dataBinderFactory
                .RegisterData(mainBuildData)
                .RegisterData(customBuildReport)
                .RegisterData(buildPreferencesData)
                .RegisterData(gitAssistant)
                .RegisterData(buildPreferencesData.GlobalDataStorage);

            globalDataStorage = DataBinder.GetData<GlobalDataStorage>();
        }

        private async Task UpdateBuildDataAndUI()
        {
            if (!globalDataStorage.TryGetMainData(globalDataStorage.CustomBuildData.BuildType, out var buildData))
                buildData = globalDataStorage.RegisterOrUpdateMainData(mainBuildData.SelectedBuildType.Value,
                    new BuildTypeVersion(mainBuildData.SelectedBuildType.Value));

            mainBuildData.Version.Value = buildData.Version;
            mainBuildData.VersionTag.Value = buildData.VersionTag;
            mainBuildData.VersionMeta.Value = buildData.VersionMeta;
            mainBuildData.AddHashToVersion.Value = buildData.AddHash;
            mainBuildData.SelectedBuildType.Value = buildData.BuildType;
            mainBuildData.SetSceneList(buildData.Scenes);

            if (mainBuildData.Scenes == null || mainBuildData.Scenes.Count == 0)
            {
                LoadScenes();
                buildData.Scenes = mainBuildData.GetSceneList();
                globalDataStorage.RegisterOrUpdateMainData(mainBuildData.SelectedBuildType.Value, buildData);
            }
            
            SetupReorderableList();
        }

        private void SubscribeToEvents()
        {
            mainBuildData.SelectedBuildType.OnValueChanged += OnChangeBuildTypeSettings;
            EditorApplication.update += OnUpdate;
        }

        private void InitializeGUIContents()
        {
            sceneDropGUIContent = ResourceManager.GetContentWithTitle("DropArea", "Drag scene here to add to list");
            copyIconGUIContent = ResourceManager.GetContentWithTooltip("Copy", "Copy and select path");
            gitIconGUIContent = ResourceManager.GetContentWithTitle("Git", "");
            gitPullIconGUIContent = ResourceManager.GetContentWithTitle("GitPull", "");
            gitCommitIconGUIContent = ResourceManager.GetContentWithTitle("GitCommit", "");
            headerGUIContent = new GUIContent("");
        }
        
        private void EnablePlugins()
        {
            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeSetupPlugin();
        }

        private void InvokeEditorPluginsOnFocus()
        {
            if (globalDataStorage.editorPlugins == null) return;

            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeOnFocusPlugin();
        }

        private void InitStyles()
        {
            centeredLabelStyle ??= new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            headerBoxStyle ??= new GUIStyle(GUI.skin.box)
            {
                normal = new GUIStyleState()
                {
                    background = ResourceManager.GetTexture2D("Header")
                }
            };
            
            BGStyle ??= new GUIStyle(GUI.skin.box)
            {
                normal = new GUIStyleState()
                {
                    background = ResourceManager.GetTexture2D("BG")
                }
            };
        }

        void OnGUI()
        {
            InitStyles();

            DrawGitInfo();

            #region Window scroll

            pageScrollPosition = GUILayout.BeginScrollView(pageScrollPosition, /*EditorStyles.helpBox,*/ GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)); //----------------------- Begin Window scroll -----------------------

            #region Window

            GUILayout.BeginVertical(); //----------------------- Window Vertical -----------------------

            Event evt = Event.current;

            #region Scene list

            DrawSceneManagement(evt);

            #endregion

            GUILayout.Space(10);

            #region Build option

            DrawBuildOption();

            #endregion

            DrawPluginPage();

            GUILayout.EndVertical(); //----------------------- End Window Vertical -----------------------

            #endregion

            GUILayout.EndScrollView(); //----------------------- End Window scroll -----------------------

            #endregion

            GUILayout.Space(10);

            #region Control Buttons

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(); //----------------------- Control Buttons Horizontal -----------------------

            if (GUILayout.Button("Open Player Settings", GUILayout.ExpandWidth(false)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            if (!EditorUserBuildSettings.development)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show build reports"))
                    CustomBuildReportsWindow.CreateOrFocusWindow();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build Game", EditorStyles.popup))
            {
                ShowDropdownMenu();
            }

            GUILayout.EndHorizontal(); //----------------------- End Control Buttons Horizontal -----------------------

            #endregion
        }

        private void SaveBuildData()
        {
            if (globalDataStorage.TryGetMainData(mainBuildData.SelectedBuildType.Value, out var buildData))
            {
                buildData.Version = mainBuildData.Version.Value;
                buildData.VersionTag = mainBuildData.VersionTag.Value;
                buildData.VersionMeta = mainBuildData.VersionMeta.Value;
                buildData.AddHash = mainBuildData.AddHashToVersion.Value;

                globalDataStorage.RegisterOrUpdateMainData(mainBuildData.SelectedBuildType.Value, buildData);

                PlayerSettings.bundleVersion =
                    globalDataStorage.CustomBuildData.GetFullBuildVersion(mainBuildData.SelectedBuildType.Value);
            }
        }

        #region Scenes management

        private void LoadScenes()
        {
            mainBuildData.Scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        }

        private void SetupReorderableList()
        {
            reorderableList = new ReorderableList(mainBuildData.Scenes, typeof(EditorBuildSettingsScene), true, true, false, true);
            
            reorderableList.multiSelect = true;

            reorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Scenes In Build:"); };
            reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 5;

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorBuildSettingsScene scene = mainBuildData.Scenes[index];

                var checkBoxWidth = 20;
                bool newEnabledValue = EditorGUI.Toggle(new Rect(rect.x, rect.y, checkBoxWidth - 2, rect.height),
                    scene.enabled);
                if (newEnabledValue != scene.enabled)
                {
                    scene.enabled = newEnabledValue;
                    UpdateBuildSettingsScenes();
                }

                float copyButtonWidth = 25;
                float labelWidth = rect.width - 20 - copyButtonWidth;
                EditorGUI.LabelField(new Rect(rect.x + checkBoxWidth, rect.y, labelWidth, rect.height), scene.path);

                if (GUI.Button(new Rect(rect.x + checkBoxWidth + labelWidth, rect.y, copyButtonWidth, rect.height), copyIconGUIContent))
                {
                    EditorGUIUtility.systemCopyBuffer = scene.path;
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
                }
            };

            reorderableList.onChangedCallback = (ReorderableList list) => UpdateBuildSettingsScenes();

            reorderableList.onAddCallback = (ReorderableList list) => { AddOpenScenes(); };

            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                mainBuildData.Scenes.RemoveAt(list.index);
                UpdateBuildSettingsScenes();
            };
            
            reorderableList.drawNoneElementCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "No scenes in build");
            };
            
            reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (isActive)
                {
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                }
            };
        }

        private void AddOpenScenes()
        {
            foreach (var scene in EditorSceneManager.GetActiveScene().GetRootGameObjects().Select(go => go.scene.path)
                         .Distinct())
            {
                if (!string.IsNullOrEmpty(scene) && !mainBuildData.Scenes.Any(s => s.path == scene))
                {
                    mainBuildData.Scenes.Add(new EditorBuildSettingsScene(scene, true));
                }
            }

            UpdateBuildSettingsScenes();
        }

        private void UpdateBuildSettingsScenes()
        {
            EditorBuildSettings.scenes = mainBuildData.Scenes.ToArray();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            if (globalDataStorage.TryGetMainData(mainBuildData.SelectedBuildType.Value, out var buildData))
            {
                buildData.Scenes = mainBuildData.GetSceneList();

                globalDataStorage.RegisterOrUpdateMainData(mainBuildData.SelectedBuildType.Value, buildData);
            }
        }

        #endregion

        #region OnGui Drawers section

        private void DropSceneArea(Event evt)
        {
            /*Rect dropArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, sceneDropGUIContent, GUI.skin.box);*/
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is SceneAsset)
                            {
                                string path = AssetDatabase.GetAssetPath(draggedObject);
                                if (!mainBuildData.Scenes.Any(s => s.path == path))
                                {
                                    mainBuildData.Scenes.Add(new EditorBuildSettingsScene(path, true));
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
            DropSceneArea(evt);

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
            mainBuildData.SelectedBuildType.Value =
                (EBuildType)EditorGUILayout.EnumPopup("Build Type", mainBuildData.SelectedBuildType.Value);

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

            mainBuildData.Version.Value = EditorGUILayout.TextField("Build Version:", mainBuildData.Version.Value);

            GUILayout.Label("-", GUILayout.Width(10));

            mainBuildData.VersionTag.Value =
                EditorGUILayout.TextField(mainBuildData.VersionTag.Value, GUILayout.Width(30));

            DrawVersionGitMeta();

            //buildIncrementorData.VersionFormat.Value = (EVersionFormatType)EditorGUILayout.EnumPopup(buildIncrementorData.VersionFormat.Value);

            GUILayout.Space(10);
            mainBuildData.AddHashToVersion.Value =
                EditorGUILayout.Toggle("Add commit hash", mainBuildData.AddHashToVersion.Value);

            if (EditorGUI.EndChangeCheck())
                formatChange = true;

            if (formatChange)
            {
                if (GUILayout.Button("Save Format"))
                {
                    SaveBuildData();

                    formatChange = false;
                }
            }

            GUILayout.EndHorizontal(); //----------------------- Build Version Management Horizontal -----------------------
        }

        private void DrawVersionGitMeta()
        {
            if (gitAssistant.gitAvailable && mainBuildData.AddHashToVersion.Value)
            {
                GUILayout.Label(".", GUILayout.Width(8));

                mainBuildData.VersionMeta.Value = gitAssistant.commitShortHash;

                GUILayout.Label(new GUIContent(gitAssistant.commitShortHash), GUILayout.Width(55));
            }
            else if (gitAssistant.gitAvailable && !mainBuildData.AddHashToVersion.Value)
            {
                mainBuildData.VersionMeta.Value = string.Empty;
            }
        }

        private void DrawGitInfo()
        {
            Rect headerRect = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
            GUI.Box(headerRect, headerGUIContent, headerBoxStyle);

            GUI.BeginGroup(headerRect);

            if (gitAssistant.gitAvailable)
            {
                if (gitAssistant.isFetching)
                {
                    Rect fetchingLabelRect = new Rect(0, (headerRect.height - 20) / 2, headerRect.width, 20);
                    GUI.Label(fetchingLabelRect, gitAssistant.fetchingText, centeredLabelStyle);
                }
                else
                {
                    gitIconGUIContent.text = gitAssistant.currentBranch;
                    gitPullIconGUIContent.text = gitAssistant.commitsBehind;
                    gitCommitIconGUIContent.text = gitAssistant.commitShortHash;

                    float labelWidth = headerRect.width / 3;
                    float labelHeight = 20;
                    float yPos = (headerRect.height - labelHeight) / 2;

                    Rect gitIconRect = new Rect(0, yPos, labelWidth, labelHeight);
                    Rect gitPullIconRect = new Rect(labelWidth, yPos, labelWidth, labelHeight);
                    Rect gitCommitIconRect = new Rect(labelWidth * 2, yPos, labelWidth, labelHeight);

                    AnimateLabel(gitIconRect, gitIconGUIContent, centeredLabelStyle, ref gitIconTextOffset);
                    AnimateLabel(gitPullIconRect, gitPullIconGUIContent, centeredLabelStyle, ref gitPullIconTextOffset);
                    AnimateLabel(gitCommitIconRect, gitCommitIconGUIContent, centeredLabelStyle, ref gitCommitIconTextOffset);
                }
            }
            GUI.EndGroup();
            
            if (time % 2 < 0.1f)
                lineColor = GetRandomColor();

            DrawUILine(Color.Lerp(Color.red, lineColor, Mathf.PingPong((float)time, 1)));
        }
        
        private Color GetRandomColor()
        {
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);

            return new Color(r, g, b);
        }
        
        private void AnimateLabel(Rect labelRect, GUIContent content, GUIStyle style, ref float textOffset)
        {
            float textWidth = style.CalcSize(new GUIContent(content.text)).x;
            float totalContentWidth = textWidth + style.padding.horizontal;

            GUI.BeginGroup(labelRect);
            
            if (totalContentWidth > labelRect.width)
            {
                textOffset = Mathf.Repeat((float)time * 20f, totalContentWidth);
                GUI.BeginClip(new Rect(-textOffset, 0, totalContentWidth + labelRect.width, labelRect.height));
                GUI.Label(new Rect(0, 0, totalContentWidth, labelRect.height), content, style);
                GUI.EndClip();
            }
            else
            {
                textOffset = 0f;
                GUI.Label(new Rect(0, 0, labelRect.width, labelRect.height), content, style);
            }

            GUI.EndGroup();
        }

        private void ShowDropdownMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Build"), false, () => BuildGame(BuildOptions.ShowBuiltPlayer));
            menu.AddItem(new GUIContent("Clean Build"), false,
                () => BuildGame(BuildOptions.ShowBuiltPlayer | BuildOptions.CleanBuildCache));

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

                        string menuItemName = attr != null && !string.IsNullOrEmpty(attr.NamePath)
                            ? attr.NamePath
                            : type.Name;
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

                pluginInstance.InvokeSetupPlugin();

                globalDataStorage.editorPlugins.Add(new PluginsStorage()
                    { BuildPluginEditor = pluginInstance, Expand = true });
            }
        }

        #endregion

        #region OValueChange setion

        private void OnChangeBuildTypeSettings(EBuildType eBuildType)
        {
            if (!globalDataStorage.TryGetMainData(eBuildType, out var buildData))
                buildData = globalDataStorage.RegisterOrUpdateMainData(eBuildType, new BuildTypeVersion(eBuildType));
            
            mainBuildData.SelectedBuildType.Value = eBuildType;
            
            mainBuildData.Version.Value = buildData.Version;
            mainBuildData.VersionTag.Value = buildData.VersionTag;
            mainBuildData.VersionMeta.Value = buildData.VersionMeta;
            mainBuildData.AddHashToVersion.Value = buildData.AddHash;
            mainBuildData.SetSceneList(buildData.Scenes);
            
            if(mainBuildData.Scenes == null || mainBuildData.Scenes.Count == 0)
                LoadScenes();
            
            globalDataStorage.CustomBuildData.BuildType = eBuildType;

            SetupReorderableList();
            UpdateBuildSettingsScenes();
            
            PlayerSettings.bundleVersion = globalDataStorage.CustomBuildData.GetFullBuildVersion(eBuildType);
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
            string defaultName = $"{Application.productName}_{mainBuildData.SelectedBuildType.Value}{extension}";
            string path = EditorUtility.SaveFilePanel("Choose Location and Name for Build", "", defaultName,
                extension.Replace(".", ""));

            if (string.IsNullOrEmpty(path)) return;

            mainBuildData.BuildPath = path;

            InvokePluginBeforeBuild();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = mainBuildData.Scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
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

            customBuildReport.LastBuildReport = report;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[CBS]: Build succeeded: {summary.totalSize} bytes at path {summary.outputPath}");

                InvokePluginAfterBuild();
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[CBS]: Build failed: {summary.totalErrors} errors, {summary.totalWarnings} warnings");
            }
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
            time = EditorApplication.timeSinceStartup;
            
            if (gitAssistant.gitAvailable && gitAssistant.isFetching)
                gitAssistant.AnimateLoadingText();
            
            Repaint();
        }

        private void OnLostFocus()
        {
            AssetDatabase.SaveAssetIfDirty(globalDataStorage);
        }

        private void OnDestroy()
        {
            CustomBuildPreferencesWindow.Instance?.Close();
            CustomBuildReportsWindow.Instance?.Close();

            AssetDatabase.SaveAssetIfDirty(globalDataStorage);

            mainBuildData.SelectedBuildType.OnValueChanged -= OnChangeBuildTypeSettings;

            EditorApplication.update -= OnUpdate;

            foreach (var editorPlugin in globalDataStorage.editorPlugins)
                editorPlugin.BuildPluginEditor.InvokeDestroyPlugin();

            gitAssistant.Dispose();
            dataBinderFactory.Cleanup();

            gitAssistant = null;
            mainBuildData = null;
            reorderableList = null;
        }
    }
}
