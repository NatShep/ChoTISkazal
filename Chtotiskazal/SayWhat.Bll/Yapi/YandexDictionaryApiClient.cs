using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SayWhat.Bll.Yapi
{
    public class YandexDictionaryApiClient
    {
        private readonly string _key;
        private readonly TimeSpan _timeout;
        public bool IsOnline { get; private set; }
        public YandexDictionaryApiClient(string key, TimeSpan timeout)
        {
            _key = key;
            _timeout = timeout;
        }

        private string MakeQuery(string word, string langFrom, string langTo) =>
            $@"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key={_key}&lang={langFrom}-{langTo}&text={HttpUtility.UrlEncode(word)}";

        public async Task<bool> Ping()
        {
            using (var client = new HttpClient(){Timeout = _timeout})
            {
                try
                {
                    var ans = await client.GetAsync($"https://ya.ru/");
                    IsOnline = true;
                    return ans.StatusCode== HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    IsOnline = false;
                    return false;
                }
            }
        }

        public Task<YaDefenition[]> EnRuTranslateAsync(string word) => TranslateAsync(word,"en","ru");
        public Task<YaDefenition[]> RuEnTranslateAsync(string word) => TranslateAsync(word,"ru","en");

        private async Task<YaDefenition[]> TranslateAsync(string word,string langFrom, string langTo)
        {
            using var client = new HttpClient(){Timeout = _timeout};
            
            try
            {
                var query = MakeQuery(word,langFrom,langTo);
                var ans = await client.GetStringAsync(query);
                IsOnline = true;
                var deserialized = JsonSerializer.Deserialize<YapiDicAnswer>(ans);
                return deserialized.Defenitions;
            }
            catch (Exception)
            {
                IsOnline = false;
                return null;
            }
        }
    }
}
