using System;

namespace ImverGames.CustomBuildSettings.Invoker
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodOrderAttribute : Attribute
    {
        public int Order { get; private set; }

        public MethodOrderAttribute(int order)
        {
            Order = order;
        }
    }
}