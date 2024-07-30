namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Defines a contract for build data objects, ensuring they can be cleared of their current state.
    /// </summary>
    public interface IBuildData
    {
        /// <summary>
        /// Clears the current state of the build data, resetting its properties or fields as necessary.
        /// </summary>
        public void Clear();
    }
}