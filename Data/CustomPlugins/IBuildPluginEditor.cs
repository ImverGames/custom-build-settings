namespace ImverGames.BuildIncrementor
{
    public interface IBuildPluginEditor
    {
        void InvokeSetupPlugin(BuildIncrementorData buildIncrementorData);
        void InvokeOnFocusPlugin();
        void InvokeGUIPlugin();

        void InvokeDestroyPlugin();
    }
}