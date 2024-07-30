using System;
using System.Collections.Generic;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Manages the registration and retrieval of IBuildData instances, allowing for centralized data access.
    /// </summary>
    public class DataBinder
    {
        /// <summary>
        /// Gets the collection of currently registered data types.
        /// </summary>
        public static IEnumerable<Type> RegisteredData => dataRepository.Keys;

        /// <summary>
        /// A repository holding registered data instances, keyed by their type.
        /// </summary>
        private static Dictionary<Type, IBuildData> dataRepository;
        
        /// <summary>
        /// Initializes a new instance of the DataBinder class and its repository.
        /// </summary>
        public DataBinder()
        {
            dataRepository = new Dictionary<Type, IBuildData>();
        }
        
        /// <summary>
        /// Registers an instance of IBuildData with the binder.
        /// </summary>
        /// <typeparam name="T">The type of data to register, must implement IBuildData.</typeparam>
        /// <param name="service">The instance of the service to register.</param>
        public void RegisterData<T>(T service) where T : IBuildData
        {
            dataRepository[typeof(T)] = service;
        }
        
        /// <summary>
        /// Retrieves a registered data instance by its type.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve, must implement IBuildData.</typeparam>
        /// <returns>The instance of the requested data type if registered; throws an exception otherwise.</returns>
        public static T GetData<T>() where T : IBuildData
        {
            return (T) dataRepository[typeof(T)];
        }
        
        /// <summary>
        /// Unregisters a data instance by its type.
        /// </summary>
        /// <typeparam name="T">The type of data to unregister, must implement IBuildData.</typeparam>
        public void UnregisterData<T>() where T : IBuildData
        {
            dataRepository.Remove(typeof(T));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
        
        /// <summary>
        /// Clears all registered data instances and calls their Clear method.
        /// </summary>
        private void Clear()
        {
            foreach (var dataKey in RegisteredData)
                dataRepository[dataKey].Clear();
            
            dataRepository.Clear();
        }
    }
}