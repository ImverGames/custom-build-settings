namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Factory class for creating and managing a DataBinder instance, 
    /// facilitating the registration and unregistration of IBuildData instances.
    /// </summary>
    public class DataBinderFactory : IDataBinderFactory
    {
        /// <summary>
        /// Singleton instance of the DataBinderFactory.
        /// </summary>
        public static IDataBinderFactory Instance { get; private set; }

        private DataBinder dataBinder;

        /// <summary>
        /// Initializes a new instance of the DataBinderFactory class and sets it as the singleton instance.
        /// </summary>
        public DataBinderFactory()
        {
            Instance = this;
        }

        /// <summary>
        /// Creates a new DataBinder instance for managing IBuildData instances.
        /// </summary>
        public void CreateDataBinder()
        {
            dataBinder = new DataBinder();
        }
        
        /// <summary>
        /// Registers a data object with the DataBinder.
        /// </summary>
        /// <typeparam name="T">The type of data object to register, must implement IBuildData.</typeparam>
        /// <param name="data">The data object instance to register.</param>
        /// <returns>The instance of the DataBinderFactory, allowing for method chaining.</returns>
        public IDataBinderFactory RegisterData<T>(T data) where T : IBuildData
        {
            dataBinder.RegisterData<T>(data);
            return this;
        }

        /// <summary>
        /// Unregisters a data object from the DataBinder.
        /// </summary>
        /// <typeparam name="T">The type of data object to unregister, must implement IBuildData.</typeparam>
        /// <param name="data">The data object instance to unregister.</param>
        /// <returns>The instance of the DataBinderFactory, allowing for method chaining.</returns>
        public IDataBinderFactory UnregisterData<T>(T data) where T : IBuildData
        {
            dataBinder.UnregisterData<T>();
            return this;
        }

        /// <summary>
        /// Performs cleanup operations, disposing of the DataBinder and its managed resources.
        /// </summary>
        public void Cleanup()
        {
            dataBinder.Dispose();
        }
    }
}