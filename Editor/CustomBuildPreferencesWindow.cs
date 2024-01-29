using ImverGames.CustomBuildSettings.Data;
using UnityEditor;

namespace ImverGames.BuildIncrementor.Editor
{
    public class CustomBuildPreferencesWindow : EditorWindow
    {
        public BuildPreferencesData BuildPreferencesData { get; private set; }

        public CustomBuildPreferencesWindow ShowWindow()
        {
            return GetWindow<CustomBuildPreferencesWindow>("Custom Build Preferences");
        }
        
        public void Initialize()
        {
            BuildPreferencesData = new BuildPreferencesData();
        }

        private void OnGUI()
        {
            
        }
    }
}