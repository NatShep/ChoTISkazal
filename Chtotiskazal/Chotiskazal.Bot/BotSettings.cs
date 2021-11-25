using System;
using Microsoft.Extensions.Configuration;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot
{
    public class BotSettings
    {
        public BotSettings() {}
        
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
            WelcomeMessage = configuration["welcome-msg"];
        }

        private ExamSettings ReadExamSettings(IConfigurationSection secton) =>
            new ExamSettings
            {
                MinAdvancedExamMinQuestionAskedCount = ReadInt(secton, "MinAdvancedExamMinQuestionAskedCount"),
                MaxAdvancedExamMinQuestionAskedCount = ReadInt(secton,"MaxAdvancedExamMinQuestionAskedCount"),
                MaxAdvancedQuestionsCount = ReadInt(secton,"MaxAdvancedQuestionsCount"),
                MinLearningWordsCountInOneExam = ReadInt(secton,"MinLearningWordsCountInOneExam"),
                MaxLearningWordsCountInOneExam = ReadInt(secton,"MaxLearningWordsCountInOneExam"),
                MinTimesThatLearningWordAppearsInExam = ReadInt(secton,"MinTimesThatLearningWordAppearsInExam"),
                MaxTimesThatLearningWordAppearsInExam = ReadInt(secton,"MaxTimesThatLearningWordAppearsInExam"),
                MaxExamSize = ReadInt(secton,"MaxExamSize"),
            };

        private int ReadInt(IConfigurationSection section, string key) {

            try
            {
                var value = section[key];
                return int.Parse(value);
            }
            catch (Exception)
            {
                throw new InvalidCastException($"Cannot parse or found {key}");
            }
            
        }
        public ExamSettings ExamSettings { get; }
        public string WelcomeMessage { get; }
        public string YadicapiKey { get; set; }
        public TimeSpan YadicapiTimeout { get; set; }
        public string YatransapiKey { get; }
        public TimeSpan YatransapiTimeout { get; }
        public string TelegramToken { get; }
        public string BotHelperToken { get; }
        public string ControlPanelChatId { get; }
        public string MongoConnectionString { get; set; }
        public string MongoDbName { get; set; }

    }
}