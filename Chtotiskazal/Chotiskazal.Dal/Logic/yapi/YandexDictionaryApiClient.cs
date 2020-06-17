using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Chotiskazal.LogicR.yapi
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

        private string MakeQuery(string word) =>
            $@"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key={_key}&lang=en-ru&text={HttpUtility.UrlEncode(word)}";

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

        public async Task<YaDefenition[]> Translate(string word)
        {
            using var client = new HttpClient(){Timeout = _timeout};
            
            try
            {
                var query = MakeQuery(word);
                var ans = await client.GetStringAsync(query);
                IsOnline = true;
                var deserialized = JsonSerializer.Deserialize<YapiDicAnswer>(ans);
                return deserialized.Defenitions;
            }
            catch (Exception e)
            {
                IsOnline = false;

                return null;
            }
        }
    }
}
