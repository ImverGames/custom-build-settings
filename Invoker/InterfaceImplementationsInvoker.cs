using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Invoker
{
    public class InterfaceImplementationsInvoker
    {
        public static List<TInterface> FindAllPluginsEditor<TInterface>() where TInterface : class
        {
            var interfaceType = typeof(TInterface);

            var plugins = new List<TInterface>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        var instance = Activator.CreateInstance(type) as TInterface;
                        
                        plugins.Add(instance);
                    }
                }
            }

            return plugins;
        }

        public static void InvokeMethodOnAllImplementations<TInterface>(TInterface plugin, string methodName, object[] args) where TInterface : class
        {
            var interfaceType = typeof(TInterface);
            var methodInfo = interfaceType.GetMethod(methodName);

            if (methodInfo == null)
            {
                Debug.LogError($"Method '{methodName}' not found in interface '{interfaceType}'.");
                return;
            }

            methodInfo.Invoke(plugin, args);
        }
        
        public static List<TInterface> GetOrderedMethods<TInterface>(IEnumerable<TInterface> plugins, string methodName) where TInterface : class
        {
            var orderedPlugins = plugins
                .Select(plugin => new
                {
                    Plugin = plugin,
                    Order = plugin.GetType()
                        .GetMethod(methodName)
                        ?.GetCustomAttribute<MethodOrderAttribute>()?.Order ?? int.MaxValue
                })
                .OrderBy(item => item.Order)
                .Select(item => item.Plugin)
                .ToList();

            return orderedPlugins;
        }
        
        public static List<TInterface> GetOrderedPlugins<TInterface>(IEnumerable<TInterface> plugins) where TInterface : class
        {
            var orderedPlugins = plugins
                .Select(plugin => new
                {
                    Plugin = plugin,
                    Order = plugin.GetType()
                        .GetCustomAttribute<PluginOrderAttribute>()?.Order ?? int.MaxValue
                })
                .OrderBy(item => item.Order)
                .Select(item => item.Plugin)
                .ToList();

            return orderedPlugins;
        }
    }
}