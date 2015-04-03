using Newtonsoft.Json.Linq;

namespace XbmcPoller
{
    public static class JsonExtensions
    {
        public static T SelectTokenOrDefault<T>(this JToken token, string path)
            where T : JToken
        {
            if (token == null)
                return null;
            var t = token.SelectToken(path);
            var res = t != null ? t as T : null;
            return res;
        }

        public static T GetPropertyValue<T>(this JObject obj, string propertyName)
        {
            if (obj == null)
                return default (T);
            var prop = obj.Property(propertyName);
            if (prop != null)
            {
                if (prop.Value != null)
                    return prop.Value.ToObject<T>();
            }
            return default(T);
        }


    }
}