using System.Text.Json.Serialization;

namespace SayWhat.Bll.Yapi;

public class YapiTransAnswer {
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("lang")] public string Lang { get; set; }
    [JsonPropertyName("text")] public string[] Texts { get; set; }
}