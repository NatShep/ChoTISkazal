using System;
using Microsoft.Extensions.Configuration;

namespace Chotiskazal.Bot
{
    public class BotSettings
    {
        public BotSettings(IConfigurationRoot configuration)
        {
            YadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            YadicapiTimeout = TimeSpan.FromSeconds(5);
            YatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            YatransapiTimeout = TimeSpan.FromSeconds(5);
            
            TelegramToken = configuration["telegramToken"];
            MongoConnectionString = configuration["dbString"];
            HelpMessage = configuration["help-msg"];
            WelcomeMessage = configuration["welcome-msg"];
        }
        public string HelpMessage { get; }
        public string WelcomeMessage { get; }
        public string YadicapiKey { get; }
        public TimeSpan YadicapiTimeout { get; }
        public string YatransapiKey { get; }
        public TimeSpan YatransapiTimeout { get; }
        public string TelegramToken { get; }
        public string MongoConnectionString { get; }
    }
}