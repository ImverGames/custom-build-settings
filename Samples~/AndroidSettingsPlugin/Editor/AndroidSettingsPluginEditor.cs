using System;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.AndroidSettings.Editor
{
    [PluginOrder(0)]
    public class AndroidSettingsPluginEditor : IBuildPluginEditor
    {
        private BuildIncrementorData buildIncrementorData;
        
        private BuildValue<bool> developmentBuild;
        private BuildValue<string> packageName;
        private BuildValue<int> bundleVersion;
        
        private bool buildAppBundle = false;
        private AndroidCreateSymbols createSymbolZip;
        
        private string keystorePass;
        private string keyaliasPass;

        
        
        public void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData)
        {
            this.buildIncrementorData = buildIncrementorData;

            developmentBuild = new BuildValue<bool>();
            packageName = new BuildValue<string>();
            bundleVersion = new BuildValue<int>();

            developmentBuild.Value = EditorUserBuildSettings.development;
            
            keystorePass = PlayerSettings.Android.keystorePass;
            keyaliasPass = PlayerSettings.Android.keyaliasPass;

            packageName.Value = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            bundleVersion.Value = PlayerSettings.Android.bundleVersionCode;
            
            developmentBuild.OnValueChanged += DevelopmentBuildOnOnValueChanged;
            packageName.OnValueChanged += PackageNameOnOnValueChanged;
            bundleVersion.OnValueChanged += BundleVersionOnOnValueChanged;
            buildIncrementorData.SelectedBuildType.OnValueChanged += BuildTypeSettingsOnOnValueChanged;
        }

        public void InvokeOnFocusPlugin()
        {
            developmentBuild.Value = EditorUserBuildSettings.development;
            
            keystorePass = PlayerSettings.Android.keystorePass;
            keyaliasPass = PlayerSettings.Android.keyaliasPass;

            packageName.Value = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            bundleVersion.Value = PlayerSettings.Android.bundleVersionCode;
        }

        private void AndroidSDKVersionOnOnValueChanged(AndroidSdkVersions sdkVersions)
        {
            PlayerSettings.Android.targetSdkVersion = sdkVersions;
        }

        private void BundleVersionOnOnValueChanged(int version)
        {
            PlayerSettings.Android.bundleVersionCode = version;
        }

        private void PackageNameOnOnValueChanged(string name)
        {
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, name);
        }

        private void DevelopmentBuildOnOnValueChanged(bool value)
        {
            EditorUserBuildSettings.development = value;
        }
        
        private void BuildTypeSettingsOnOnValueChanged(EBuildType eBuildType)
        {
            switch (buildIncrementorData.SelectedBuildType.Value)
            {
                case EBuildType.RELEASE:
                    developmentBuild.Value = false;
                    break;
                case EBuildType.MILESTONE:
                    developmentBuild.Value = false;
                    break;
                case EBuildType.DAILY:
                    developmentBuild.Value = false;
                    break;
                case EBuildType.DEVELOPMENT:
                    developmentBuild.Value = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void InvokeGUIPlugin()
        {
            developmentBuild.Value = EditorGUILayout.Toggle("Development Build", developmentBuild.Value);
            
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                buildAppBundle = EditorGUILayout.Toggle("Build App Bundle", buildAppBundle);
                createSymbolZip = (AndroidCreateSymbols)EditorGUILayout.EnumPopup("Create Symbols Zip", createSymbolZip);
                
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                EditorUserBuildSettings.androidCreateSymbols = createSymbolZip;

                DrawPackageName();

                DrawBundleVersion();

                DrawKeystoreSettings();
            }
        }

        private void DrawKeystoreSettings()
        {
            GUILayout.Space(10);

            if (PlayerSettings.Android.useCustomKeystore)
            {
                GUILayout.Label("Android Keystore Settings", EditorStyles.boldLabel);
                keystorePass = EditorGUILayout.PasswordField("Keystore Password", keystorePass);
                keyaliasPass = EditorGUILayout.PasswordField("Key Alias Password", keyaliasPass);

                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }
        }

        private void DrawPackageName()
        {
            GUILayout.Space(5);
            
            packageName.Value = EditorGUILayout.TextField("PackageName:", packageName.Value);
        }
        
        private void DrawBundleVersion()
        {
            bundleVersion.Value = EditorGUILayout.IntField("Bundle version:", bundleVersion.Value);

            GUILayout.Space(5);
        }

        public void InvokeBeforeBuild()
        {
            
        }

        public void InvokeAfterBuild()
        {
            
        }
        
        public void InvokeDestroyPlugin()
        {
            if (developmentBuild != null)
            {
                developmentBuild.OnValueChanged -= DevelopmentBuildOnOnValueChanged;
                developmentBuild = null;
            }

            if (packageName != null)
            {
                packageName.OnValueChanged -= PackageNameOnOnValueChanged;
                packageName = null;
            }

            if (bundleVersion != null)
            {
                bundleVersion.OnValueChanged -= BundleVersionOnOnValueChanged;
                bundleVersion = null;
            }

            if (buildIncrementorData != null)
            {
                buildIncrementorData.SelectedBuildType.OnValueChanged -= BuildTypeSettingsOnOnValueChanged;
                buildIncrementorData = null;
            }
        }
    }
}
