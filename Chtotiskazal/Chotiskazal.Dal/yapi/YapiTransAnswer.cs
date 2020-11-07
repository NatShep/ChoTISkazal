using System.Text.Json.Serialization;

namespace Chotiskazal.LogicR.yapi
{
    public class YapiTransAnswer
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("lang")]
        public string Lang { get; set; }
        [JsonPropertyName("text")]
        public string[] Texts { get; set; }
    }
}