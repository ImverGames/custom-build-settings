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
        private MainBuildData mainBuildData;
        private GlobalDataStorage globalDataStorage;
        
        private SRDebugPluginData srDebugPluginData;
        
        private BuildValue<bool> IsEnabled;
        private BuildValue<Settings.TriggerEnableModes> triggerEnableMode;

        public void InvokeSetupPlugin()
        {
            mainBuildData = DataBinder.GetData<MainBuildData>();
            globalDataStorage = DataBinder.GetData<GlobalDataStorage>();

            IsEnabled = new BuildValue<bool>();
            triggerEnableMode = new BuildValue<Settings.TriggerEnableModes>();

            SaveLoadData();
            
            IsEnabled.OnValueChanged += IsEnabledOnOnValueChanged;
            triggerEnableMode.OnValueChanged += TriggerEnableModeOnOnValueChanged;
            mainBuildData.SelectedBuildType.OnValueChanged += BuildTypeSettingsOnOnValueChanged;
        }

        private void BuildTypeSettingsOnOnValueChanged(EBuildType obj)
        {
            SaveLoadData();
        }

        private void SaveLoadData()
        {
            if (!globalDataStorage.TryGetPluginData<SRDebugPluginData>(GetType(), mainBuildData.SelectedBuildType.Value, out srDebugPluginData))
                srDebugPluginData = globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value,
                    new SRDebugPluginData()
                    {
                        IsEnabled = Settings.Instance.IsEnabled,
                        TriggerEnableMode = Settings.Instance.EnableTrigger
                    });

            IsEnabled.Value = srDebugPluginData.IsEnabled;
            triggerEnableMode.Value = srDebugPluginData.TriggerEnableMode;
        }

        public void InvokeOnFocusPlugin()
        {
            
        }

        private void TriggerEnableModeOnOnValueChanged(Settings.TriggerEnableModes triggerEnableModes)
        {
            if (globalDataStorage.TryGetPluginData<SRDebugPluginData>(GetType(), mainBuildData.SelectedBuildType.Value, out var debugPluginData))
            {
                debugPluginData.TriggerEnableMode = triggerEnableModes;
                
                globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value, debugPluginData);
            }

            Settings.Instance.EnableTrigger = triggerEnableModes;
        }

        private void IsEnabledOnOnValueChanged(bool value)
        {
            if (globalDataStorage.TryGetPluginData<SRDebugPluginData>(GetType(), mainBuildData.SelectedBuildType.Value, out var debugPluginData))
            {
                debugPluginData.IsEnabled = value;
                
                globalDataStorage.RegisterOrUpdatePluginData(this.GetType(), mainBuildData.SelectedBuildType.Value, debugPluginData);
            }
            
            Settings.Instance.IsEnabled = value;
        }

        public void InvokeGUIPlugin()
        {
            GUILayout.Space(5);

            IsEnabled.Value = EditorGUILayout.Toggle("Enable SRDebugger", IsEnabled.Value);
                    
            GUILayout.Space(5);
                    
            triggerEnableMode.Value =
                (Settings.TriggerEnableModes)EditorGUILayout.EnumPopup("Enable SRDebugger Trigger",
                    triggerEnableMode.Value);

            Settings.Instance.EnableTrigger = GetTriggerMode(mainBuildData.SelectedBuildType.Value);
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
            
            if(mainBuildData.SelectedBuildType != null)
                mainBuildData.SelectedBuildType.OnValueChanged -= BuildTypeSettingsOnOnValueChanged;
        }
    }
}
