using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PureVocabBuilder;

public class YandexTranslateApiClientIAM
{
    private readonly string _iaMkey;
    private readonly TimeSpan _timeout;
    public bool IsOnline { get; private set; }
    public YandexTranslateApiClientIAM(string iaMkey, TimeSpan timeout)
    {
        _iaMkey = iaMkey;
        _timeout = timeout;
    }
    public async Task<string> Translate(string word)
    {
        using var client = new HttpClient(){Timeout = _timeout};
            
        try
        {
            var uri = new Uri("https://translate.api.cloud.yandex.net/translate/v2/translate");
            var iamToken =
                "<key>";

            var query = new YaTransQuery()
            {
                FolderId = "<key>",
                Texts = new []{word},
                TargetLanguageCode = "ru"
            };

            var body = JsonSerializer.Serialize(query);
            
            
            var data = new StringContent(body, Encoding.UTF8, "application/json");
            
            var request= new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization",$"Bearer {iamToken}");
            request.Content = data;

            var response = await client.SendAsync(request);
            var ans = await response
                            .Content
                            .ReadAsStringAsync();
            
             IsOnline = true;
            var deserialized = JsonSerializer.Deserialize<YaTransAnswer>(ans);

            return deserialized.Translations?.FirstOrDefault().Text;
        }
        catch (Exception)
        {
            IsOnline = false;

            return null;
        }
    }
}

public class YaTransQuery
{
    [JsonPropertyName("folderId")] 
    public string FolderId { get; set; }
    
    [JsonPropertyName("texts")] 
    public string[] Texts { get; set; }
    
    [JsonPropertyName("targetLanguageCode")] 
    public string TargetLanguageCode { get; set; }
}

public class YaTranslation {
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("detectedLanguageCode")]
    public string DetectedLanguageCode { get; set; }
}

public class YaTransAnswer
{
    [JsonPropertyName("translations")] 
    public YaTranslation[] Translations { get; set; }
}