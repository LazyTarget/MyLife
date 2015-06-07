using System;
using System.Configuration;

namespace PollingEngine.Core
{
    public class PollingEngineConfigSection : ConfigurationSection
    {
        public static PollingEngineConfigSection LoadFromConfig()
        {
            var settings = ConfigurationManager.GetSection("PollingEngine") as PollingEngineConfigSection;
            settings = settings ?? new PollingEngineConfigSection();
            return settings;
        }


        [ConfigurationProperty("Pollers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PollingProgramCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public PollingProgramCollection Pollers
        {
            get
            {
                var o = base["Pollers"];
                var res = (PollingProgramCollection)o;
                return res;
            }
        }

    }
}
