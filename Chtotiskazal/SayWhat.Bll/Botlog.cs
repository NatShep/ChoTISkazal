﻿using System;
 using Serilog;
 using Serilog.Events;
 using Serilog.Formatting.Json;
 using Serilog.Sinks.RollingFile;

 namespace SayWhat.Bll
{
    public static class Botlog{
        
        
        private static ILogger _log = Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("Version", "6.0.0")
            .WriteTo.Sink(new RollingFileSink(
                @"log-ChoTiSkazal.json", 
                new JsonFormatter(), 2147483648, 5))
            .WriteTo.File(@"log-ChoTiSkazal-.txt", rollingInterval:RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
        
        public static void WriteError(long? chatId, string msg)
        {
            _log.Error("msg {@ChatInfo} ", new {ChatInfo = chatId});
        }
        public static void WriteInfo(string msg)
        {
            _log.Information(msg);
        }
        
        public static void WriteInfo(string msg, string chatId)
        {
            _log.Information(msg+" {@ChatInfo}", new {ChatInfo = chatId});
        }

        public static void UpdateMetricInfo(long? userTelegramId, string metricId, string param,
            TimeSpan swElapsed)
        {
            _log.Information("Update metric info: {@metricInfo} ", new {UserTelegramId = userTelegramId, MetricId=metricId,Param=param,SwElapsed=swElapsed});
        }

        public static void SaveQuestionMetricInfo(QuestionMetric questionMetric, string chatId)
        {
            _log.Information("Save question metric {@ChatInfo} {@questionMetric}", new {ChatInfo=chatId}, questionMetric);
        }

        public static void RegisterExamInfo(long? userTelegramId, DateTime started, int questionsCount, int questionsPassed)
        {
            _log.Information("Register Exam {@ChatInfo} {@Exam}", new {ChatInfo=userTelegramId},
                new {Started=started,QuestionsCount=questionsCount,QuestionPassed=questionsPassed});
        }
    }
}