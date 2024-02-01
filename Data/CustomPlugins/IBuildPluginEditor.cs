namespace ImverGames.CustomBuildSettings.Data
{
    public interface IBuildPluginEditor
    {
        void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData);
        void InvokeOnFocusPlugin();
        void InvokeGUIPlugin();
        void InvokeBeforeBuild();
        void InvokeAfterBuild();
        void InvokeDestroyPlugin();
    }
}
