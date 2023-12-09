using System;
using Microsoft.Extensions.Configuration;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot;

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
            MinWordsQuestionsInOneExam = ReadInt(secton, nameof(ExamSettings.MinWordsQuestionsInOneExam)),
            MaxWordsQuestionsInOneExam = ReadInt(secton, nameof(ExamSettings.MaxWordsQuestionsInOneExam)),
            MaxExamSize = ReadInt(secton,nameof(ExamSettings.MaxExamSize)), 
                
            ExamsCountGoalForDay = ReadInt(secton, nameof(ExamSettings.ExamsCountGoalForDay)), 
            MaxTranslationsInOneExam = ReadInt(secton, nameof(ExamSettings.MaxTranslationsInOneExam)), 
                
            NewWordInOneExam = ReadInt(secton, nameof(ExamSettings.NewWordInOneExam)), 
            LearningWordsInOneExam = ReadInt(secton, nameof(ExamSettings.LearningWordsInOneExam)), 
            WellDoneWordsInOneExam = ReadInt(secton, nameof(ExamSettings.WellDoneWordsInOneExam)), 
            LearnedWordsInOneExam = ReadInt(secton, nameof(ExamSettings.LearnedWordsInOneExam)), 
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
    public string TelegramToken { get; set; }
    public string BotHelperToken { get; set; }
    public string ControlPanelChatId { get; }
    public string MongoConnectionString { get; set; }
    public string MongoDbName { get; set; }

}