namespace ImverGames.CustomBuildSettings.Data
{
    public interface IBuildPluginEditor
    {
        void InvokeSetupPlugin(BuildDataProvider buildDataProvider);
        void InvokeOnFocusPlugin();
        void InvokeGUIPlugin();
        void InvokeBeforeBuild();
        void InvokeAfterBuild();
        void InvokeDestroyPlugin();
    }
}
