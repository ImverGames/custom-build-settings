using System;
using ImverGames.CustomBuildSettings.Data;
using ImverGames.CustomBuildSettings.Invoker;
using SRDebugger;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.SRDebuggerSettings.Editor
{
    [PluginOrder(3, "SRDebug/SRDebugPlugin")]
    public class SRDebugPluginEditor : IBuildPluginEditor
    {
        private BuildDataProvider buildDataProvider;
        
        private bool manualy = true;
        
        private BuildValue<bool> IsEnabled;
        private BuildValue<Settings.TriggerEnableModes> triggerEnableMode;

        public void InvokeSetupPlugin(BuildDataProvider buildDataProvider)
        {
            this.buildDataProvider = buildDataProvider;

            IsEnabled = new BuildValue<bool>();
            triggerEnableMode = new BuildValue<Settings.TriggerEnableModes>();

            IsEnabled.Value = Settings.Instance.IsEnabled;
            triggerEnableMode.Value = Settings.Instance.EnableTrigger;
            
            IsEnabled.OnValueChanged += IsEnabledOnOnValueChanged;
            triggerEnableMode.OnValueChanged += TriggerEnableModeOnOnValueChanged;
            buildDataProvider.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;
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

                return;
            }
            else
            {
                GUILayout.Label("Activity SRDebugger Depends on the type of build at \"Release\" it will be disabled and unavailable");
            }

            Settings.Instance.EnableTrigger = GetTriggerMode(buildDataProvider.SelectedBuildType.Value);
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

        public void InvokeBeforeBuild()
        {
            
        }

        public void InvokeAfterBuild()
        {
            
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

            if (buildDataProvider != null)
            {
                buildDataProvider.SelectedBuildType.OnValueChanged += SelectedBuildTypeOnOnValueChanged;
                buildDataProvider = null;
            }
        }
    }
}
