using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace MyLife.App
{
    public static class CommonExtensions
    {
        public static ApplicationDataContainer GetContainerOrDefault(this ApplicationDataContainer container, string key)
        {
            if (container.Containers.ContainsKey(key))
                return container.Containers[key];
            return null;
        }

    }
}
