using System;
using Newtonsoft.Json;

namespace YHCJB.Util
{
    public class Json<T> where T : Json<T>
    {
        public string ToJson()
        {
            return ToJson((T)this);
        }

        public static string ToJson(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static TResult FromJson<TResult>(string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json);
        }        
    }

    public static class JsonExtension
    {
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
