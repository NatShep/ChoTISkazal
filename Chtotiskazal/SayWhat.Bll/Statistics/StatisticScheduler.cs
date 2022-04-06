using System;
using System.Text;
using System.Timers;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Statistics {
public static class StatisticScheduler {
    private static DateTime _launchTime;
    private static Func<UserModel[]> _currentUsersLocator;
    private static Timer _timer;

    public static void Launch(TimeSpan timeSpan, Func<UserModel[]> currentUsersLocator) {
        _launchTime = DateTime.Now;
        _timer = new Timer(timeSpan.TotalMilliseconds);
        _timer.Elapsed += (sender, args) => {
            var message = GetStatisticMessage(_currentUsersLocator());
            Botlog.WriteInfo(message,true);
        };
        _timer.Enabled = true;
    }

    private static  string GetStatisticMessage(UserModel[] currentUsers) {
        var counters = Botlog.Collectors.Flush();
        var from = counters.Since;
        
        int activeUsers = 0;
        int newUsers = 0;

        foreach (var user in currentUsers) {
            if (user.LastActivity >= from)
                activeUsers++;
            if (user.Created >= from)
                newUsers++;
        }
        
        var sb = new StringBuilder($"Stats for last {DateTime.Now- from}\r\n");
        sb.AppendLine($"Alive: {DateTime.Now- _launchTime}");
        sb.AppendLine($"New    users: {newUsers}");
        sb.AppendLine($"Active users: {activeUsers}");
        sb.AppendLine($"Users in pool: {currentUsers.Length}");
        sb.AppendLine($"Errors: {counters.Errors}");
        if (activeUsers == 0) {
            sb.AppendLine($"Nothing more happens for last {DateTime.Now - from}");
            return sb.ToString();
        }
        sb.AppendLine();
        sb.AppendLine("Exams:");
        sb.AppendLine($"Users {counters.UsersThatPassedExams}");
        sb.AppendLine($"Exams passed: {counters.ExamsFinished}");
        sb.AppendLine($"Question asked: {counters.QuestionsAsked}");
        sb.AppendLine($"Question failed: {counters.QuestionsPassed-counters.QuestionsAsked}");
        sb.AppendLine();
        sb.AppendLine("Translations:");
        sb.AppendLine($"Users {counters.UsersThatTranslatedSomething}");
        sb.AppendLine($"Translated : {counters.TranslationRequested}");
        sb.AppendLine($"Not found : {counters.TranlationNotFound}");
        sb.AppendLine();
        sb.AppendLine("Translations selected:");
        sb.AppendLine($"Users {counters.UsersThatChoosSomeTranslation}");
        sb.AppendLine($"Translation selected: {counters.TranslationChoosen}");
        sb.AppendLine($"Translation removed : {counters.TranslationRemoved}");
        sb.AppendLine();
        sb.AppendLine("LearningSets:");
        sb.AppendLine($"Added words from ls: {counters.WordsFromLearningSetAdded}");

        return sb.ToString();
    }
}

}