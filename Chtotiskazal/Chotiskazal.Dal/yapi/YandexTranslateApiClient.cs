using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Chotiskazal.LogicR.yapi
{
    public class YandexTranslateApiClient
    {
        private readonly string _key;
        private readonly TimeSpan _timeout;
        public bool IsOnline { get; private set; }
        public YandexTranslateApiClient(string key, TimeSpan timeout)
        {
            _key = key;
            _timeout = timeout;
        }

        private string MakeQuery(string word) =>
            $@"https://translate.yandex.net/api/v1.5/tr.json/translate?key={_key}&text={HttpUtility.UrlEncode(word)}&lang=en-ru";

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

        public async Task<string> Translate(string word)
        {
            using var client = new HttpClient(){Timeout = _timeout};
            
            try
            {
                var query = MakeQuery(word);
                var ans = await client.GetStringAsync(query);
                IsOnline = true;
                var deserialized = JsonSerializer.Deserialize<YapiTransAnswer>(ans);
                return deserialized.Texts?.FirstOrDefault();
            }
            catch (Exception e)
            {
                IsOnline = false;
                return null;
            }
        }
    }
}
