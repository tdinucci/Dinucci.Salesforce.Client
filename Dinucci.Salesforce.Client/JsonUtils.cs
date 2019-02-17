using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client
{
    public static class JsonUtils
    {
        public static void EnsurePropertyExists(JObject jObject, string property)
        {
            if (!jObject.ContainsKey(property))
                throw new ArgumentException($"'{nameof(jObject)}' does not contain '{property}' field");
        }

        public static T GetJObjectProperty<T>(JObject jObject, string property, bool isRequired = true)
        {
            if (!isRequired && !jObject.ContainsKey(property))
                return default(T);

            EnsurePropertyExists(jObject, property);

            return jObject[property].Value<T>();
        }

        public static T[] GetJObjectArrayProperty<T>(JObject jObject, string property, bool isRequired = true)
        {
            if (!isRequired && !jObject.ContainsKey(property))
                return default(T[]);

            EnsurePropertyExists(jObject, property);

            if (!(jObject[property] is JArray jArray))
                return default(T[]);

            return jArray.ToObject<T[]>();
        }
    }
}