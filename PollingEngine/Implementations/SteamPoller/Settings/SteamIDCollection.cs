using System.Configuration;

namespace SteamPoller
{
    public class SteamIDCollection : ConfigurationElementCollection
    {
        public SteamIDConfigElement this[int index]
        {
            get { return (SteamIDConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(SteamIDConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SteamIDConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SteamIDConfigElement)element).SteamID;
        }

        public void Remove(SteamIDConfigElement serviceConfig)
        {
            BaseRemove(serviceConfig.SteamID);
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