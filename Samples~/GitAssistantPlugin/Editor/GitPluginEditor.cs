using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.GitPlugin.Editor
{
    [PluginOrder(3, "Git/GitPlugin")]
    public class GitPluginEditor : IBuildPluginEditor
    {
        private string gitCommand = string.Empty;
        private string gitCommandOutput = "Command output will be shown here...";
        private Vector2 scrollPosition;

        private bool available = false;

        private MainBuildData mainBuildData;
        private GitAssistant gitAssistant;
        
        public async void InvokeSetupPlugin()
        {
            mainBuildData = DataBinder.GetData<MainBuildData>();
            gitAssistant = DataBinder.GetData<GitAssistant>();

            available = await gitAssistant.CheckGitAvailable();
        }

        public async void InvokeOnFocusPlugin()
        {
            if(gitAssistant != null)
                available = await gitAssistant.CheckGitAvailable();
        }

        public async void InvokeGUIPlugin()
        {
            if (!available)
            {
                GUILayout.Label("Git not Available", EditorStyles.boldLabel);
                return;
            }
            GUILayout.Label("Execute Git Command", EditorStyles.boldLabel);

            gitCommand = EditorGUILayout.TextField("Git Command Without 'git'", gitCommand);

            if (GUILayout.Button("Execute Command"))
            {
                gitCommandOutput = await gitAssistant.ExecuteGitCommandAsync(gitCommand);
                return;
            }

            GUILayout.Label("Command Output:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

            GUIStyle textStyle = GUI.skin.textArea;
            GUIContent content = new GUIContent(gitCommandOutput);
            float height = textStyle.CalcHeight(content, EditorGUIUtility.currentViewWidth - 30);

            EditorGUILayout.TextArea(gitCommandOutput, textStyle, GUILayout.ExpandHeight(true),
                GUILayout.Height(height < 150 ? 150 : height));
            EditorGUILayout.EndScrollView();
        }

        public void InvokeBeforeBuild()
        {
            
        }

        public void InvokeAfterBuild()
        {
            
        }

        public void InvokeDestroyPlugin()
        {
            
        }
    }
}
