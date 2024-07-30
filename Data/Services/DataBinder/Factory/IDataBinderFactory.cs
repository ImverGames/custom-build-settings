namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Defines a contract for a factory responsible for managing data binders,
    /// including the registration and unregistration of data services.
    /// </summary>
    public interface IDataBinderFactory
    {
        /// <summary>
        /// Registers a data service with the data binder.
        /// </summary>
        /// <typeparam name="T">The type of the data service to register, which must implement IBuildData.</typeparam>
        /// <param name="service">The instance of the data service to register.</param>
        /// <returns>The IDataBinderFactory instance, allowing for method chaining.</returns>
        public IDataBinderFactory RegisterData<T>(T service) where T : IBuildData;
        
        /// <summary>
        /// Unregisters a previously registered data service from the data binder.
        /// </summary>
        /// <typeparam name="T">The type of the data service to unregister, which must implement IBuildData.</typeparam>
        /// <param name="data">The instance of the data service to unregister.</param>
        /// <returns>The IDataBinderFactory instance, allowing for method chaining.</returns>
        public IDataBinderFactory UnregisterData<T>(T data) where T : IBuildData;
    }
}