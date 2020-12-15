﻿using System;
 using SayWhat.MongoDAL.QuestionMetrics;
 using Serilog;
 using Serilog.Core;
 using Serilog.Events;
 using Serilog.Formatting.Json;
 using Serilog.Sinks.RollingFile;
 using TelegramSink;

 namespace SayWhat.Bll
{
    public static class Botlog{
        
        public static QuestionMetricRepo QuestionMetricRepo { get; set; }
        
    
        
        private static ILogger _log = Log.Logger = new LoggerConfiguration()
            .WriteTo.Sink(new RollingFileSink(
                @"log-ChoTiSkazal.json", 
                new JsonFormatter(), 2147483648, 5))
            .WriteTo.File(@"log-ChoTiSkazal-.txt", rollingInterval:RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
        
        
        private static ILogger _alarmLog = Log.Logger = new LoggerConfiguration()
            .WriteTo.TeleSink(
                telegramApiKey:"1481805832:AAEHcFj3toa6D4_D0UQDX9FjHJ0P2QeGN80",
                telegramChatId:"-331839279",
                minimumLevel:LogEventLevel.Information)
            .CreateLogger();
        
        public static void WriteError(long? chatId, string msg, bool writeToTelegram)
        {
            _log.Error("msg {@ChatInfo} ", new {ChatInfo = chatId, msg});
            if (writeToTelegram)
                _alarmLog.Error("❗ " + " msg {@ChatInfo} ", new {ChatInfo = chatId, msg});
        }
        public static void WriteInfo(string msg,bool writeToTelegram)
        {
            _log.Information(msg);
            if (writeToTelegram)
                _alarmLog.Error("✅ " + msg);
        }
        
        public static void WriteInfo(string msg, string chatId,bool writeToTelegram)
        {
            _log.Information(msg+" {@ChatInfo}", new {ChatInfo = chatId});
            if (writeToTelegram)
                _alarmLog.Error("✅ " + " msg {@ChatInfo} ", new {ChatInfo = chatId, msg});
        }

        public static void UpdateMetricInfo(long? userTelegramId, string metricId, string param,
            TimeSpan swElapsed)
        {
            _log.Information("Update metric info: {@metricInfo} ", new {UserTelegramId = userTelegramId, MetricId=metricId,Param=param,SwElapsed=swElapsed});
        }

        public static void SaveQuestionMetricInfo(QuestionMetric questionMetric, string chatId)
        {
            _log.Information("Save question metric {@ChatInfo} {@questionMetric}", new {ChatInfo=chatId}, questionMetric);
            QuestionMetricRepo?.Add(questionMetric);
        }

        public static void RegisterExamInfo(long? userTelegramId, DateTime started, int questionsCount, int questionsPassed)
        {
            _log.Information("Register Exam {@ChatInfo} {@Exam}", new {ChatInfo=userTelegramId},
                new {Started=started,QuestionsCount=questionsCount,QuestionPassed=questionsPassed});
        }
        
    
    
        
    }
}