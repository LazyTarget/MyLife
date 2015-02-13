using Newtonsoft.Json.Linq;

namespace CalendarCore.Http
{
    public static class JsonExtensions
    {
        public static T ToObjectOrDefault<T>(this JToken token)
        {
            if (token == null)
                return default (T);
            return token.ToObject<T>();
        }

        public static T GetPropertyValue<T>(this JObject obj, string property)
        {
            if (obj == null)
                return default(T);
            var prop = obj.Property(property);
            if (prop == null)
                return default (T);
            var value = prop.Value;
            var result = value.ToObjectOrDefault<T>();
            return result;
        }

    }
}
