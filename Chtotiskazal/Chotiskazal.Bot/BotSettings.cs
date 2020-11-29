using System;
using Microsoft.Extensions.Configuration;
using SayWhat.Bll.Services;

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


            ExamSettings = ReadExamSettings(configuration.GetSection("examSettings")); 
            TelegramToken = configuration["telegramToken"];
            MongoConnectionString = configuration["dbString"];
            HelpMessage = configuration["help-msg"];
            WelcomeMessage = configuration["welcome-msg"];
        }

        private ExamSettings ReadExamSettings(IConfigurationSection configuration) =>
            new ExamSettings
            {
                MinAdvancedExamMinQuestionAskedCount = int.Parse(configuration["MinAdvancedExamMinQuestionAskedCount"]),
                MaxAdvancedExamMinQuestionAskedCount = int.Parse(configuration["MaxAdvancedExamMinQuestionAskedCount"]),
                MaxAdvancedQuestionsCount = int.Parse(configuration["MaxAdvancedQuestionsCount"]),
                LearningWordsCountInOneExam = int.Parse(configuration["LearningWordsCountInOneExam"]),
                MinTimesThatLearningWordAppearsInExam = int.Parse(configuration["MinTimesThatLearningWordAppearsInExam"]),
                MaxTimesThatLearningWordAppearsInExam = int.Parse(configuration["MaxTimesThatLearningWordAppearsInExam"]),
                MaxExamSize = int.Parse(configuration["MaxExamSize"]),
            };


        public ExamSettings ExamSettings { get; }
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