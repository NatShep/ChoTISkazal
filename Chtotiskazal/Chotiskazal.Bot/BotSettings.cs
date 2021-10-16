using System;
using Microsoft.Extensions.Configuration;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot
{
    public class BotSettings
    {
        public BotSettings() {
            
        }
        public BotSettings(IConfigurationRoot configuration)
        {
            YadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            YadicapiTimeout = TimeSpan.FromSeconds(5);
            YatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            YatransapiTimeout = TimeSpan.FromSeconds(5);
            
            ExamSettings = ReadExamSettings(configuration.GetSection("examSettings")); 
            TelegramToken = configuration["telegramToken"];
            BotHelperToken = configuration.GetSection("botHelper").GetSection("botHelperToken").Value;
            ControlPanelChatId = configuration.GetSection("botHelper").GetSection("controlPanelChatId").Value;
            MongoConnectionString = configuration["dbString"];
            MongoDbName = configuration["dbName"];
            HelpMessage = configuration["help-msg"];
            WelcomeMessage = configuration["welcome-msg"];
        }

        private ExamSettings ReadExamSettings(IConfigurationSection configuration) =>
            new ExamSettings
            {
                MinAdvancedExamMinQuestionAskedCount = int.Parse(configuration["MinAdvancedExamMinQuestionAskedCount"]),
                MaxAdvancedExamMinQuestionAskedCount = int.Parse(configuration["MaxAdvancedExamMinQuestionAskedCount"]),
                MaxAdvancedQuestionsCount = int.Parse(configuration["MaxAdvancedQuestionsCount"]),
                MinLearningWordsCountInOneExam = int.Parse(configuration["MinLearningWordsCountInOneExam"]),
                MaxLearningWordsCountInOneExam = int.Parse(configuration["MaxLearningWordsCountInOneExam"]),
                MinTimesThatLearningWordAppearsInExam = int.Parse(configuration["MinTimesThatLearningWordAppearsInExam"]),
                MaxTimesThatLearningWordAppearsInExam = int.Parse(configuration["MaxTimesThatLearningWordAppearsInExam"]),
                MaxExamSize = int.Parse(configuration["MaxExamSize"]),
            };


        public ExamSettings ExamSettings { get; }
        public string HelpMessage { get; }
        public string WelcomeMessage { get; }
        public string YadicapiKey { get; set; }
        public TimeSpan YadicapiTimeout { get; set; }
        public string YatransapiKey { get; }
        public TimeSpan YatransapiTimeout { get; }
        public string TelegramToken { get; set; }
        public string BotHelperToken { get; set; }
        public string ControlPanelChatId { get; set; }
        public string MongoConnectionString { get; set; }
        public string MongoDbName { get; set; }

    }
}