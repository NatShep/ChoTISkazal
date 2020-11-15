using System;
using System.Threading;
using System.Threading.Tasks;
using Chotiskazal.LogicR.yapi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chotiskazal.WebApp
{
    public class YapiPingHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<YapiPingHostedService> _logger;
        private readonly YandexDictionaryApiClient _yadicapiClient;
        private readonly YandexTranslateApiClient _yaTransClient;
        private Timer _timer;

        public YapiPingHostedService(
            ILogger<YapiPingHostedService> logger, 
            YandexDictionaryApiClient yadicapiClient, YandexTranslateApiClient yaTransClient)
        {
            _logger = logger;
            _yadicapiClient = yadicapiClient;
            _yaTransClient = yaTransClient;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(
                callback: DoWork, 
                state: null, 
                dueTime: TimeSpan.Zero,
                period:  TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var p1 = _yadicapiClient.Ping();
            var p2 = _yaTransClient.Ping();
            Task.WaitAll(p1, p2);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
