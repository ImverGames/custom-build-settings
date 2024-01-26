using System;
using ImverGames.CustomBuildSettings.Data;
using SRDebugger;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.SRDebuggerSettings.Editor
{
    public class SRDebugPluginEditor : IBuildPluginEditor
    {
        private BuildIncrementorData buildIncrementorData;
        
        private bool foldout = true;
        private bool manualy = true;
        
        private BuildValue<bool> IsEnabled;
        private BuildValue<Settings.TriggerEnableModes> triggerEnableMode;

        public void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData)
        {
            this.buildIncrementorData = buildIncrementorData;

            IsEnabled = new BuildValue<bool>();
            triggerEnableMode = new BuildValue<Settings.TriggerEnableModes>();

            IsEnabled.Value = Settings.Instance.IsEnabled;
            triggerEnableMode.Value = Settings.Instance.EnableTrigger;
            
            IsEnabled.OnValueChanged += IsEnabledOnOnValueChanged;
            triggerEnableMode.OnValueChanged += TriggerEnableModeOnOnValueChanged;
            buildIncrementorData.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;
        }

        public void InvokeOnFocusPlugin()
        {
            IsEnabled.Value = Settings.Instance.IsEnabled;
            triggerEnableMode.Value = Settings.Instance.EnableTrigger;
        }

        private void SelectedBuildTypeOnOnValueChanged(EBuildType buildType)
        {
            if (buildType == EBuildType.RELEASE)
                manualy = false;
        }

        private void TriggerEnableModeOnOnValueChanged(Settings.TriggerEnableModes triggerEnableModes)
        {
            Settings.Instance.EnableTrigger = triggerEnableModes;
        }

        private void IsEnabledOnOnValueChanged(bool value)
        {
            Settings.Instance.IsEnabled = value;
        }

        public void InvokeGUIPlugin()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "SRDebugger activity");

            if (foldout)
            {
                GUILayout.Space(5);
                
                manualy = GUILayout.Toggle(manualy, "Manually change settings");
                
                GUILayout.Space(10);

                if (manualy)
                {
                    IsEnabled.Value = GUILayout.Toggle(IsEnabled.Value, "Enable SRDebugger");
                    
                    GUILayout.Space(5);
                    
                    triggerEnableMode.Value =
                        (Settings.TriggerEnableModes)EditorGUILayout.EnumPopup("Enable SRDebugger Trigger",
                            triggerEnableMode.Value);

                    EditorGUILayout.EndFoldoutHeaderGroup();
                    
                    GUILayout.EndVertical();

                    return;
                }
                else
                {
                    GUILayout.Label("Activity SRDebugger Depends on the type of build at \"Release\" it will be disabled and unavailable");
                }

            }

            Settings.Instance.EnableTrigger = GetTriggerMode(buildIncrementorData.SelectedBuildType.Value);
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            GUILayout.EndVertical();
        }

        private Settings.TriggerEnableModes GetTriggerMode(EBuildType buildType)
        {
            switch (buildType)
            {
                case EBuildType.RELEASE:
                    Settings.Instance.IsEnabled = false;
                    return Settings.TriggerEnableModes.Off;
                case EBuildType.MILESTONE:
                    Settings.Instance.IsEnabled = true;
                    return Settings.TriggerEnableModes.DevelopmentBuildsOnly;
                case EBuildType.DAILY:
                    Settings.Instance.IsEnabled = true;
                    return Settings.TriggerEnableModes.DevelopmentBuildsOnly;
                case EBuildType.DEVELOPMENT:
                    Settings.Instance.IsEnabled = true;
                    return Settings.TriggerEnableModes.DevelopmentBuildsOnly;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildType), buildType, null);
            }
        }
        
        public void InvokeDestroyPlugin()
        {
            if (IsEnabled != null)
            {
                IsEnabled.OnValueChanged -= IsEnabledOnOnValueChanged;
                IsEnabled = null;
            }

            if (triggerEnableMode != null)
            {
                triggerEnableMode.OnValueChanged -= TriggerEnableModeOnOnValueChanged;
                triggerEnableMode = null;
            }

            if (buildIncrementorData != null)
            {
                buildIncrementorData.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;

                buildIncrementorData = null;
            }
        }
    }
}