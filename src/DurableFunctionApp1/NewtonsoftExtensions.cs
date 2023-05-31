using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionApp1
{
    public static class NewtonsoftExtensions
    {
        public static string ToJson<T>(this T obj, bool indented = false)
        {
            Formatting formatting = Formatting.None;
            if (indented)
                formatting |= Formatting.Indented;
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, };
            return JsonConvert.SerializeObject(obj, formatting, settings);
        }

        public static string ToJson<T>(this T obj, JsonSerializerSettings settings, bool indented)
        {
            Formatting formatting = Formatting.None;
            if (indented)
                formatting |= Formatting.Indented;
            return JsonConvert.SerializeObject(obj, formatting, settings);
        }

        public static T FromJson<T>(this string json, bool throwOnNullRet = true)
        {
            T ret = default(T);

            try
            {
                ret = JsonConvert.DeserializeObject<T>(json);
                if (null == ret // usually happens when json == ""
                    && throwOnNullRet)
                {
                    throw new Exception(BuildToJsonExceptionMessage<T>(json));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(BuildToJsonExceptionMessage<T>(json), ex);
            }

            return ret;
        }

        private static string BuildToJsonExceptionMessage<T>(string json)
        {
            string ret = "FromJson() tried to convert to [" + typeof(T).FullName.ToString()
                    + "] and returned null. The json string was: [" + (json ?? "(null)") + "].";
            return ret;

        }

        public static T GetValue<T>(this JObject obj, string key, T defaultValue = default(T))
        {
            T ret;
            JToken data;
            if (!obj.TryGetValue(key, out data))
                ret = defaultValue;
            else if (null == data.Value<T>())
                ret = defaultValue;
            else
                ret = data.ToObject<T>();
            return ret;
        }
    }
}
