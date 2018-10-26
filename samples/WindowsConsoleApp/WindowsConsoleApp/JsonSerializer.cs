using Newtonsoft.Json;
using W8lessLabs.GraphAPI;

namespace WindowsConsoleApp
{
    class JsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value);
        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value);
    }
}
