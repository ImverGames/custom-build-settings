using System;
using System.Collections.Generic;
using System.Linq;
using ImverGames.BuildIncrementor.Editor;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImverGames.CustomBuildSettings.Editor
{
    public class CustomBuildSettingsWindow : EditorWindow
    {
        private CustomBuildPreferencesWindow customBuildPreferences;
        private BuildIncrementorData buildIncrementorData;
        private CustomBuildReport customBuildReport;
        private CustomBuildReportsWindow customBuildReportsWindow;
        private List<IBuildPluginEditor> editorPlugins;
        
        private Vector2 pageScrollPosition;
        private Vector2 sceneScrollPosition;

        private ReorderableList reorderableList;
        private List<EditorBuildSettingsScene> scenes;

        private bool formatChange;
        private bool incrementVersionManualy;
        private bool expandVersionSettings;

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
            buildIncrementorData = new BuildIncrementorData();
            customBuildReport = new CustomBuildReport();
            customBuildReportsWindow = new CustomBuildReportsWindow();
            editorPlugins = InterfaceImplementationsInvoker.FindAllPluginsEditor<IBuildPluginEditor>();
            editorPlugins = InterfaceImplementationsInvoker.GetOrderedPlugins<IBuildPluginEditor>(editorPlugins);
            
            LoadScenes();
            SetupReorderableList();
            
            BuildTypeVersionIncrementor.LoadOrSaveVersionFromFile(out var buildData);
            buildIncrementorData.Version.Value = buildData.GetBuildVersion(buildData.BuildType);
            buildIncrementorData.VersionTag.Value = buildData.GetBuildVersionTag(buildData.BuildType);
            buildIncrementorData.VersionMeta.Value = buildData.GetBuildVersionMeta(buildData.BuildType);
            buildIncrementorData.VersionFormat.Value = GetFormatTypeFromString(buildIncrementorData.Version.Value);
            buildIncrementorData.SelectedBuildType.Value = buildData.BuildType;
            
            buildIncrementorData.SelectedBuildType.OnValueChanged += OnChangeBuildTypeSettings;
            buildIncrementorData.Version.OnValueChanged += OnChangeVersion;
            buildIncrementorData.VersionFormat.OnValueChanged += OnChangeVersionFormat;
            
            customBuildReportsWindow.Initialize(customBuildReport);
            customBuildPreferences.Initialize(buildIncrementorData.BuildPreferencesData);

            EnablePlugins();
        }

        private void OnFocus()
        {
            BuildTypeVersionIncrementor.LoadOrSaveVersionFromFile(out var buildData);

            if (buildIncrementorData != null)
            {
                buildIncrementorData.Version.Value = buildData.GetBuildVersion(buildData.BuildType);
                buildIncrementorData.VersionTag.Value = buildData.GetBuildVersionTag(buildData.BuildType);
                buildIncrementorData.VersionMeta.Value = buildData.GetBuildVersionMeta(buildData.BuildType);
                buildIncrementorData.VersionFormat.Value = GetFormatTypeFromString(buildIncrementorData.Version.Value);
                buildIncrementorData.SelectedBuildType.Value = buildData.BuildType;
            }

            if (editorPlugins != null)
            {
                foreach (var editorPlugin in editorPlugins)
                    InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                        "InvokeOnFocusPlugin", null);
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
            foreach (var editorPlugin in editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeSetupPlugin", new object[] { buildIncrementorData });
        }

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

        private void InitStyles()
        {
            centeredLabelStyle = GUI.skin.GetStyle("Label");
            centeredLabelStyle.alignment = TextAnchor.UpperCenter;
            centeredLabelStyle.fontStyle = FontStyle.Bold;
        }

        void OnGUI()
        {
            InitStyles();

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
            
            DrawDropSceneArea(evt);

            sceneScrollPosition =
                GUILayout.BeginScrollView(sceneScrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(200));  //----------------------- Begin Scene list scroll -----------------------
            reorderableList.DoLayoutList();
            GUILayout.EndScrollView();  //----------------------- End Scene list scroll -----------------------

            if (GUILayout.Button("Add Open Scenes"))
            {
                AddOpenScenes();
            }
            
            #endregion

            GUILayout.Space(10);

            #region Build option
            
            GUILayout.BeginVertical(EditorStyles.helpBox);  //----------------------- Build option Vertical -----------------------
            GUILayout.Label("Build Options", centeredLabelStyle);
            
            GUILayout.Space(10);
            
            #region Build Type Management
            
            GUILayout.BeginHorizontal();  //----------------------- Build Type Management Horizontal -----------------------
            buildIncrementorData.SelectedBuildType.Value = (EBuildType)EditorGUILayout.EnumPopup("Build Type", buildIncrementorData.SelectedBuildType.Value);

            GUILayout.Label($"Platform: {EditorUserBuildSettings.activeBuildTarget}", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();  //----------------------- Build Type Management Horizontal -----------------------
            
            #endregion
            
            #region Build Version Management
            
            GUILayout.BeginHorizontal();  //----------------------- Build Version Management Horizontal -----------------------
            
            EditorGUI.BeginChangeCheck();
        
            buildIncrementorData.Version.Value = EditorGUILayout.TextField("Build Version:", buildIncrementorData.Version.Value);
        
            GUILayout.Label("-", GUILayout.Width(12));
        
            buildIncrementorData.VersionTag.Value = EditorGUILayout.TextField(buildIncrementorData.VersionTag.Value, GUILayout.Width(30));

            GUILayout.Label(".", GUILayout.Width(12));
        
            buildIncrementorData.VersionMeta.Value = EditorGUILayout.TextField(buildIncrementorData.VersionMeta.Value, GUILayout.Width(30));

            buildIncrementorData.VersionFormat.Value = (EVersionFormatType)EditorGUILayout.EnumPopup(buildIncrementorData.VersionFormat.Value);
            if (EditorGUI.EndChangeCheck())
                formatChange = true;
            
            if (formatChange)
            {
                if (GUILayout.Button("Save Format"))
                {
                    var data = BuildTypeVersionIncrementor.SaveVersionFile(buildIncrementorData.Version.Value,
                        buildIncrementorData.VersionTag.Value, buildIncrementorData.VersionMeta.Value,
                        buildIncrementorData.SelectedBuildType.Value);

                    PlayerSettings.bundleVersion = data.GetFullBuildVersion(data.BuildType);
                    
                    formatChange = false;
                }
            }

            GUILayout.EndHorizontal();  //----------------------- Build Version Management Horizontal -----------------------

            if (incrementVersionManualy)
            {
                if (GUILayout.Button("Increment build version manually"))
                    IncrementBuildVersion(true);
            }

            #endregion
            
            GUILayout.EndVertical();  //----------------------- End Build option Vertical -----------------------
            
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
        
        private void ShowDropdownMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Build"), false, () => BuildGame(BuildOptions.ShowBuiltPlayer));
            menu.AddItem(new GUIContent("Clean Build"), false, () => BuildGame(BuildOptions.ShowBuiltPlayer | BuildOptions.CleanBuildCache));

            menu.ShowAsContext();
        }

        private void DrawPluginPage()
        {
            GUILayout.Space(10);
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Plugins", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            foreach (var editorPlugin in editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeGUIPlugin", null);
            
            GUILayout.Space(10);
        }

        private void OnChangeVersion(string version)
        {
            buildIncrementorData.VersionFormat.Value = GetFormatTypeFromString(buildIncrementorData.Version.Value);
        }
        
        private void OnChangeVersionFormat(EVersionFormatType eVersionFormatType)
        {
            buildIncrementorData.Version.Value = ConvertVersionFormat(buildIncrementorData.Version.Value, eVersionFormatType);
        }

        private void OnChangeBuildTypeSettings(EBuildType eBuildType)
        {
            switch (buildIncrementorData.SelectedBuildType.Value)
            {
                case EBuildType.RELEASE:
                    break;
                case EBuildType.MILESTONE:
                    break;
                case EBuildType.DAILY:
                    break;
                case EBuildType.DEVELOPMENT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            BuildTypeVersionIncrementor.TryLoadVersionFromFile(out var buildData);
            
            buildIncrementorData.Version.Value = buildData.GetBuildVersion(eBuildType);
            buildIncrementorData.VersionTag.Value = buildData.GetBuildVersionTag(eBuildType);
            buildIncrementorData.VersionMeta.Value = buildData.GetBuildVersionMeta(eBuildType);

            BuildTypeVersionIncrementor.SaveVersionFile(
                buildIncrementorData.Version.Value,
                buildIncrementorData.VersionTag.Value,
                buildIncrementorData.VersionMeta.Value,
                eBuildType);
            
            PlayerSettings.bundleVersion = buildData.GetFullBuildVersion(eBuildType);
        }

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

        private void InvokePluginBeforeBuild()
        {
            foreach (var editorPlugin in editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeBeforeBuild", null);
        }
        
        private void InvokePluginAfterBuild()
        {
            foreach (var editorPlugin in editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeAfterBuild", null);
        }

        private void BuildGame(BuildOptions options)
        {
            string extension = GetExtensionForTarget(EditorUserBuildSettings.activeBuildTarget);
            string defaultName = $"{Application.productName}_{buildIncrementorData.SelectedBuildType.Value}.{extension}";
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
                    return "exe";
                case BuildTarget.StandaloneOSX:
                    return "app";
                case BuildTarget.StandaloneLinux64:
                    return "";
                case BuildTarget.iOS:
                    return "";
                case BuildTarget.Android:
                    return EditorUserBuildSettings.buildAppBundle ? "aab" : "apk";
                case BuildTarget.WebGL:
                    return "";
                case BuildTarget.WSAPlayer:
                    return "appx";
                default:
                    return "";
            }
        }

        private void OnDestroy()
        {
            buildIncrementorData.SelectedBuildType.OnValueChanged -= OnChangeBuildTypeSettings;
            buildIncrementorData.Version.OnValueChanged -= OnChangeVersion;
            buildIncrementorData.VersionFormat.OnValueChanged -= OnChangeVersionFormat;
            
            foreach (var editorPlugin in editorPlugins)
                InterfaceImplementationsInvoker.InvokeMethodOnAllImplementations<IBuildPluginEditor>(editorPlugin,
                    "InvokeDestroyPlugin", null);

            editorPlugins = null;
            buildIncrementorData = null;
            reorderableList = null;
            scenes = null;
            
            if(customBuildReportsWindow != null)
                DestroyImmediate(customBuildReportsWindow);
        }
    }
}
