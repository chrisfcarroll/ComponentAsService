using Newtonsoft.Json;

namespace ComponentAsService2.UseComponentAsService
{
    public static class ToJsonExtensions
    {
        public static string ToJson<T>(this T @this)
        {
            return 
                JsonConvert.SerializeObject(
                    @this, 
                    JsonSerializerSettings);
        }
        
        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }
}