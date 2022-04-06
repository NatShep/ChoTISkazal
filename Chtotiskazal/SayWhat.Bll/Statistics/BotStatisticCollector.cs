namespace SayWhat.Bll.Statistics {
public class BotStatisticCollector {
    private BotStatisticMetrics _current = new();
    public BotStatisticMetrics Flush() {
        var oldCounters = _current;
        _current = new BotStatisticMetrics();
        return oldCounters;
    }
    public void OnTranslationRequest(long? userTelegramId, bool isRussian) 
        => _current.OnTranslationRequest(userTelegramId, isRussian);
    public void OnTranslationSelected(long? userTelegramId) 
        => _current.OnTranslationSelected(userTelegramId);
    public void OnExam(
        long? userTelegramId,
        int questionsCount,
        int questionsPassed) 
        => _current.OnExam(userTelegramId, questionsCount, questionsPassed);
    public void OnError() => _current.OnError();
    public void OnTranslationRemoved(long? userTelegramId) => _current.OnTranslationRemoved(userTelegramId);

    public void OnTranslationNotFound(long? userTelegramId) => _current.OnTranslationNotFound(userTelegramId);
    public void OnNewWordFromLearningSet(long? userTelegramId) => _current.OnNewWordFromLearningSet(userTelegramId);
    public void OnNewUser() => _current.OnNewUser();
}
}

