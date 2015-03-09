using System;
using System.Collections.Generic;
using OdbcWrapper;

namespace MyLife.Core
{
    public class FeedArgs : Dictionary<string, object>
    {
        public FeedArgs()
        {
            Page = 1;
            PageSize = 20;
            Channels = new List<Guid>();
        }


        public T Get<T>(string key)
        {
            var obj = ContainsKey(key) ? this[key] : default(T);
            var res = obj.SafeConvert<T>();
            return res;
        }

        public FeedArgs Set(string key, object value)
        {
            this[key] = value;
            return this;
        }


        
        public int Page
        {
            get { return Get<int>("Page"); }
            set { Set("Page", value); }
        }

        public int PageSize
        {
            get { return Get<int>("PageSize"); }
            set { Set("PageSize", value); }
        }

        public DateTime? StartTime
        {
            get { return Get<DateTime?>("StartTime"); }
            set { Set("StartTIme", value); }
        }

        public DateTime? EndTime
        {
            get { return Get<DateTime?>("EndTime"); }
            set { Set("EndTIme", value); }
        }

        public List<Guid> Channels
        {
            get { return Get<List<Guid>>("Channels"); }
            set { Set("Channels", value); }
        }

    }
}
