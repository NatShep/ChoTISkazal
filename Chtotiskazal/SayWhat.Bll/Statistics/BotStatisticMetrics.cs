using System;
using System.Collections.Generic;

namespace SayWhat.Bll.Statistics {
public class BotStatisticMetrics {
    public BotStatisticMetrics() {
        Since = DateTime.Now;
    }
    private HashSet<long> _usersThatTranslatedSomething = new(100);
    private HashSet<long> _usersThatChoosSomeTranslation = new(100);
    private HashSet<long> _usersThatPassedExams = new(100);
    private int _questionsAsked;
    private int _questionsPassed;
    private int _examsFinished;
    private int _translationRequested;
    private int _translationChoosen;
    private int _errors;
    private int _translationRemoved;
    private int _tranlationNotFound;
    private int _wordsFromLearningSetAdded;
    public DateTime Since { get; }
    public int QuestionsAsked => _questionsAsked;
    public int QuestionsPassed => _questionsPassed;
    public int ExamsFinished => _examsFinished;
    public int TranslationRequested => _translationRequested;
    public int TranslationChoosen => _translationChoosen;
    public int Errors => _errors;
    public int UsersThatPassedExams => _usersThatPassedExams.Count;
    public int UsersThatChoosSomeTranslation => _usersThatChoosSomeTranslation.Count;
    public int UsersThatTranslatedSomething => _usersThatTranslatedSomething.Count;
    public int TranslationRemoved => _translationRemoved;
    public int TranlationNotFound => _tranlationNotFound;
    public int WordsFromLearningSetAdded => _wordsFromLearningSetAdded;

    public void OnError() {
        _errors++;
    }
    
    public void OnTranslationRequest(long? userTelegramId, bool isRussian) {
        if (userTelegramId.HasValue)
            _usersThatTranslatedSomething.Add(userTelegramId.Value);
        _translationRequested++;
    }
    
    public void OnTranslationSelected(long? userTelegramId) {
        if (userTelegramId.HasValue)
            _usersThatChoosSomeTranslation.Add(userTelegramId.Value);
        _translationChoosen++;
    }
    public void OnExam(
        long? userTelegramId,
        int questionsCount,
        int questionsPassed) {
        if (userTelegramId.HasValue)
            _usersThatPassedExams.Add(userTelegramId.Value);
        _examsFinished++;
        this._questionsAsked += questionsCount;
        this._questionsPassed += questionsPassed;
    }

    public void OnTranslationRemoved(long? userTelegramId) {
        _translationRemoved++;
    }
    public void OnTranslationNotFound(long? userTelegramId) {
        _tranlationNotFound++;
    }
    public void OnNewWordFromLearningSet(long? userTelegramId) {
        _wordsFromLearningSetAdded++;
    }
}
}