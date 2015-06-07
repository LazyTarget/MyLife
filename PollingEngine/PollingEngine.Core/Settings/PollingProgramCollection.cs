using System.Configuration;

namespace PollingEngine.Core
{
    public class PollingProgramCollection : ConfigurationElementCollection
    {
        public PollingProgramConfigElement this[int index]
        {
            get { return (PollingProgramConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PollingProgramConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PollingProgramConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PollingProgramConfigElement)element).Type;
        }

        public void Remove(PollingProgramConfigElement serviceConfig)
        {
            BaseRemove(serviceConfig.Type);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}