using System.Threading.Tasks;
using Chotiskazal.Dal.yapi;
using System.Timers;


namespace Chotiskazal.Bot.Services
{
    public class YaService
    {
        private readonly YandexDictionaryApiClient _yaDicClient;
        private readonly YandexTranslateApiClient _yaTransClient;
        
        public YaService(YandexDictionaryApiClient yaDicClient,
            YandexTranslateApiClient yaTransClient)
        {
            _yaDicClient = yaDicClient;
            _yaTransClient = yaTransClient;
        }

        public (bool isYaDicOnline, bool isYaTransOnline) PingYandex()
        {
            var dicPing = _yaDicClient.Ping();
            var transPing = _yaTransClient.Ping();
            Task.WaitAll(dicPing, transPing);
            //todo ЧТА?!?!?! Тут утечка памяти и вакханалия!! Юра - протрезвей!
            var timer = new Timer(5000) {AutoReset = false, Enabled = true};
            timer.Elapsed += (s, e) =>
            {
                var pingDicApi = _yaDicClient.Ping();
                var pingTransApi = _yaTransClient.Ping();
                Task.WaitAll(pingDicApi, pingTransApi);
                timer.Enabled = true;
            };

            return (_yaDicClient.IsOnline, _yaTransClient.IsOnline);
        }
    }
}