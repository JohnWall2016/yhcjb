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

        public static TO FromJson<TO>(string json)
        {
            return JsonConvert.DeserializeObject<TO>(json);
        }        
    }
}
