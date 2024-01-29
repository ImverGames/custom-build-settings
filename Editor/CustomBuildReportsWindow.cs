using System.Collections.Generic;
using System.Linq;
using ImverGames.CustomBuildSettings.Data;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Editor
{
    public class CustomBuildReportsWindow : EditorWindow
    {
        private CustomBuildReport customBuildReport;

        private Vector2 lastReportScrollPosition;
        private Vector2 loadedReportScrollPosition;

        private bool lastReportExpand;
        private bool loadedReportExpand;

        private bool created;

        private List<SerializableBuildReport> loadedReports;

        private string reportName = "LastBuildReport";

        public void ShowCustomBuildReport()
        {
            var window = GetWindow<CustomBuildReportsWindow>("Custom Build Report");
            window.minSize = new Vector2(600, 300);

            loadedReports = BuildReportManager.LoadReports();
        }

        public void Initialize(CustomBuildReport customBuildReport)
        {
            this.customBuildReport = customBuildReport;
        }

        public void SetLastBuildReport(BuildReport buildReport)
        {
            customBuildReport.LastBuildReport = buildReport;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            if (loadedReports is null or { Count: 0 } && customBuildReport.LastBuildReport == null)
            {
                GUILayout.Label(
                    "First you need to get the build data, to do this, click the \"BuildGame\" button in the main window",
                    EditorStyles.boldLabel);
                
                return;
            }
            
            if (GUILayout.Button("Load Reports"))
                loadedReports = BuildReportManager.LoadReports();
            
            GUILayout.Space(10);

            DrawLastBuildReportSection();

            if (loadedReports is { Count: > 0 })
            {
                loadedReportExpand = EditorGUILayout.Foldout(loadedReportExpand, "Loaded Build Reports", true);

                if (loadedReportExpand)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Analyze Saved Build Reports", EditorStyles.boldLabel);

                    foreach (var report in loadedReports)
                    {
                        if (GUILayout.Button($"Analyze Report: {report.ReportName}"))
                        {
                            customBuildReport?.AnalyzeSerializedAssets(report);
                        }
                    }

                    GUILayout.Space(5);

                    DrawBuildReport(ref loadedReportScrollPosition, true);
                }
            }
        }

        private void DrawLastBuildReportSection()
        {
            if (customBuildReport != null && customBuildReport.LastBuildReport != null)
            {
                lastReportExpand = EditorGUILayout.Foldout(lastReportExpand, "Last Build Report", true);

                if (lastReportExpand)
                {
                    GUILayout.Label("Save Last Build Report", EditorStyles.boldLabel);
                    reportName = EditorGUILayout.TextField("Report Name:", reportName);

                    if (created)
                    {
                        if (GUILayout.Button("Save Report"))
                        {
                            var serializableReport =
                                BuildReportConverter.ConvertToSerializable(customBuildReport.LastBuildReport);
                            serializableReport.ReportName = reportName;
                            BuildReportManager.SaveReport(serializableReport);

                            created = false;
                        }
                    }

                    GUILayout.Space(5);

                    if (GUILayout.Button("Show Build Report"))
                    {
                        customBuildReport.GenerateLastBuildReport();

                        created = true;
                    }

                    GUILayout.Space(5);

                    DrawBuildReport(ref lastReportScrollPosition, false);
                }
            }
        }

        private void DrawBuildReport(ref Vector2 scrollVector, bool loaded)
        {
            if(customBuildReport == null)
                return;
            
            scrollVector = GUILayout.BeginScrollView(scrollVector);

            IEnumerable<string> keys = loaded
                ? customBuildReport.LoadedAssetsByCategory.Keys
                : customBuildReport.AssetsByCategory.Keys;

            foreach (var category in keys)
            {
                if (!customBuildReport.Foldouts.ContainsKey(category))
                    customBuildReport.Foldouts[category] = false;

                string foldoutLabel = $"{category} - {customBuildReport.CategorySizes[category]:F2} MB";
                customBuildReport.Foldouts[category] =
                    EditorGUILayout.Foldout(customBuildReport.Foldouts[category], foldoutLabel, true);

                if (customBuildReport.Foldouts[category])
                {
                    GUILayout.BeginVertical();

                    DrawPagination(category, loaded);

                    if (loaded)
                    {
                        foreach (var asset in customBuildReport.LoadedAssetsByCategory[category]
                                     .Skip(customBuildReport.CurrentPage[category] * customBuildReport.AssetsPerPage)
                                     .Take(customBuildReport.AssetsPerPage))
                        {
                            RenderAssetButton(asset, loaded);
                        }
                    }
                    else
                    {
                        foreach (var asset in customBuildReport.AssetsByCategory[category]
                                     .Skip(customBuildReport.CurrentPage[category] * customBuildReport.AssetsPerPage)
                                     .Take(customBuildReport.AssetsPerPage))
                        {
                            RenderAssetButton(asset, loaded);
                        }
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawPagination(string category, bool loaded)
        {
            int totalAssets = loaded
                ? customBuildReport.LoadedAssetsByCategory[category].Count
                : customBuildReport.AssetsByCategory[category].Count;
            int pages = Mathf.CeilToInt((float)totalAssets / customBuildReport.AssetsPerPage);

            if (!customBuildReport.CurrentPage.ContainsKey(category))
                customBuildReport.CurrentPage[category] = 0;

            int currentPage = customBuildReport.CurrentPage[category];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev") && currentPage > 0)
            {
                customBuildReport.CurrentPage[category]--;
            }

            GUILayout.Label($"Page {currentPage + 1} of {pages}", GUILayout.Width(100));

            if (GUILayout.Button("Next") && currentPage < pages - 1)
            {
                customBuildReport.CurrentPage[category]++;
            }

            GUILayout.EndHorizontal();
        }

        private void RenderAssetButton(object asset, bool loaded)
        {
            string label;
            string path;
            ulong size;

            if (loaded)
            {
                var loadedAsset = (SimplePackedAssetInfo)asset;
                label = customBuildReport.FormatSize(loadedAsset.PackedSize);
                path = loadedAsset.SourceAssetPath;
                size = loadedAsset.PackedSize;
            }
            else
            {
                var regularAsset = (PackedAssetInfo)asset;
                label = customBuildReport.FormatSize(regularAsset.packedSize);
                path = regularAsset.sourceAssetPath;
                size = regularAsset.packedSize;
            }

            if (GUILayout.Button($"{label} {path}"))
            {
                EditorGUIUtility.systemCopyBuffer = path;
                var assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                EditorGUIUtility.PingObject(assetObject);
            }
        }
    }
}