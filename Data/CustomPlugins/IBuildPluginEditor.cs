namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Defines the required methods for a build plugin editor, outlining the lifecycle and operational hooks
    /// necessary for integrating plugins into a build process or editor environment.
    /// </summary>
    public interface IBuildPluginEditor
    {
        /// <summary>
        /// Invoked to set up the plugin initially. Use this method for initialization tasks.
        /// </summary>
        void InvokeSetupPlugin();
        
        /// <summary>
        /// Invoked when the plugin gains focus in the editor. Use this for focus-related tasks or updates.
        /// </summary>
        void InvokeOnFocusPlugin();
        
        /// <summary>
        /// Invoked to render the plugin's GUI within the editor. Implement the plugin's GUI drawing code here.
        /// </summary>
        void InvokeGUIPlugin();
        
        /// <summary>
        /// Invoked before the build process starts. Use this to implement any pre-build logic necessary for the plugin.
        /// </summary>
        void InvokeBeforeBuild();
        
        /// <summary>
        /// Invoked after the build process completes. Use this for any post-build cleanup or operations.
        /// </summary>
        void InvokeAfterBuild();
        
        /// <summary>
        /// Invoked when the plugin is being destroyed or unloaded. Implement cleanup and resource release logic here.
        /// </summary>
        void InvokeDestroyPlugin();
    }
}
