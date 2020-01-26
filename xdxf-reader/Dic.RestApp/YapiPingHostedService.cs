using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dic.Logic.yapi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dic.RestApp
{
    public class YapiPingHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<YapiPingHostedService> _logger;
        private readonly YandexApiClient _yadicapiClient;
        private Timer _timer;

        public YapiPingHostedService(ILogger<YapiPingHostedService> logger, YandexApiClient yadicapiClient)
        {
            _logger = logger;
            _yadicapiClient = yadicapiClient;
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

        private void DoWork(object state) => _yadicapiClient.Ping().Wait();

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
