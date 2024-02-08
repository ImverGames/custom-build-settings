using System;

namespace ImverGames.CustomBuildSettings.Invoker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginOrderAttribute : Attribute
    {
        public int Order { get; private set; }
        public string NamePath { get; private set; }

        public PluginOrderAttribute(int order, string namePath = "")
        {
            Order = order;
            NamePath = namePath;
        }
    }
}