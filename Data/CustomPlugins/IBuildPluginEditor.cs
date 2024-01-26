namespace ImverGames.CustomBuildSettings.Data
{
    public interface IBuildPluginEditor
    {
        void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData);
        void InvokeOnFocusPlugin();
        void InvokeGUIPlugin();

        void InvokeDestroyPlugin();
    }
}