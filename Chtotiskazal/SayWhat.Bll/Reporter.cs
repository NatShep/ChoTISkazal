﻿using System;
 using SayWhat.Bll.Statistics;
 using SayWhat.MongoDAL.QuestionMetrics;
 using Serilog;
 using Serilog.Events;
 using Serilog.Formatting.Json;
 using Serilog.Sinks.RollingFile;

 namespace SayWhat.Bll;

 public static class Reporter {
     public static QuestionMetricRepo QuestionMetricRepo { get; set; }
     public static BotStatisticCollector Collector { get; } = new();

     private static ILogger _telegramLog = null;
     public static void SetTelegramLogger(ILogger logger) {
         _telegramLog = logger;
     }

     private static ILogger _log = Log.Logger = new LoggerConfiguration()
         .WriteTo.Sink(new RollingFileSink(
             @"log-ChoTiSkazal.json",
             new JsonFormatter(), 2147483648, 5))
         .WriteTo.File(@"log-ChoTiSkazal-.txt", rollingInterval: RollingInterval.Day)
         .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
         .CreateLogger();


     public static void WriteInfo(string msg) => _log.Information(msg);
     public static void WriteInfo(string msg, string chatId) => _log.Information(msg + " {@ChatInfo}", new { ChatInfo = chatId });

     public static void ReportError(long? chatId, string msg) {
         _log.Error("msg {@ChatInfo} ", new { ChatInfo = chatId, msg });
         _telegramLog?.Error("❗ " + " msg {@ChatInfo} ", new { ChatInfo = chatId, msg });
         Collector.OnError();
     }

     public static void ReportError(long? chatId, string msg, Exception e) 
         => ReportError(chatId, msg + $".\r\n Exception:\r\n{e}");

     public static void ReportUserIssue(string msg) {
         _log.Error($"Report {msg} ");
         _telegramLog?.Error($"❗{msg}");
     }
    
     public static void ReportError(long? chatId, string msg, string[] history, Exception e) {
         if (history == null)
             ReportError(chatId, msg, e);
         else
             ReportError(chatId,
                 $"❗'{msg}'\r\n" +
                 "-----\r\n" +
                 $"{string.Join("\r\n", history)}\r\n" +
                 "-----\r\n" +
                 "Exception:\r\n" +
                 e?.ToString()??"none");
     }

     public static void ReportQuestionDone(QuestionMetric questionMetric, string chatId, string questionName) {
         _log.Information("Save question metric {@ChatInfo} {@questionMetric} {@questionName}", new { ChatInfo = chatId }, questionMetric, questionName);
         QuestionMetricRepo?.Add(questionMetric);
     }
     public static void ReportTranslationRequsted(long? userTelegramId, bool isRussian)
         => Collector.OnTranslationRequest(userTelegramId, isRussian);
     public static void ReportTranslationPairSelected(long? userTelegramId)
         => Collector.OnTranslationSelected(userTelegramId);
     public static void ReportTranslationPairRemoved(long? userTelegramId)
         => Collector.OnTranslationRemoved(userTelegramId);
     public static void ReportNewWordFromLearningSet(long? userTelegramId)
         => Collector.OnNewWordFromLearningSet(userTelegramId);
     public static void ReportTranslationNotFound(long? userTelegramId)
         => Collector.OnTranslationNotFound(userTelegramId);

     public static void ReportBotWokeup(int meId, string meUsername) {
         var message = $"Waking up. I am {meId}:{meUsername} ";
         _log.Information(message);
         _telegramLog?.Error("✅ " + message);
     }
     public static void ReportExamPassed(long? userTelegramId, DateTime started, int questionsCount, int questionsPassed) {
         _log.Information("Register Exam {@ChatInfo} {@Exam}", new { ChatInfo = userTelegramId },
             new { Started = started, QuestionsCount = questionsCount, QuestionPassed = questionsPassed });
         Collector.OnExam(userTelegramId, questionsCount, questionsPassed);
     }

     public static void ReportNewUser(string userTelegramNick, string telegramId) {
         var message = $"New user added:  {userTelegramNick}:{telegramId} ";
         _log.Information(message);
         _telegramLog?.Error("✅ " + message);
         Collector.OnNewUser();
     }
     public static void OnUserInput(long? userTelegramId) => Collector.OnUserInput(userTelegramId);
     public static void ReportCommand(string eCommand, long chatIdIdentifier) => Collector.OnCommand(chatIdIdentifier, eCommand);
 }