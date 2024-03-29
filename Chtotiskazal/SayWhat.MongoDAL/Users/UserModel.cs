﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

#pragma warning disable 169

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
#pragma warning disable 649

namespace SayWhat.MongoDAL.Users;

[BsonIgnoreExtraElements]
public class UserModel
{
    public UserModel()
    {
    }

    public UserModel(long? telegramId, string firstName, string lastName, string telegramNick, UserSource source)
    {
        Id = ObjectId.GenerateNewId();

        _source = source;
        _telegramId = telegramId;
        _telegramNick = telegramNick;
        _telegramFirstName = firstName;
        _telegramLastName = lastName;
        _totalScoreBaskets = Baskets.CreateBaskets();

        InitializeTodayStats(DateTime.Today);
        InitializeLastMonthStats();
        OnAnyActivity();
    }


    #region mongo fields

    public ObjectId Id { get; set; }

    [BsonElement(UsersRepo.UserTelegramIdFieldName)]
    private long? _telegramId = null;

    [BsonElement("sc")] private int[] _totalScoreBaskets;
    [BsonElement("a")] private DateTime _lastActivity;
    [BsonElement("last_ex")] private DateTime _lastExam;
    [BsonElement("tfn")] private string _telegramFirstName;
    [BsonElement("tln")] private string _telegramLastName;
    [BsonElement("tn")] private string _telegramNick;
    [BsonElement("src")] private UserSource _source;
    [BsonElement("wc")] private int _wordsCount;
    [BsonElement("ec")] private int _examplesCount;
    [BsonElement("pc")] private int _pairsCount;
    [BsonElement("en_wtc")] private int _englishWordTranslationRequestsCount;
    [BsonElement("ru_wtc")] private int _russianWordTranslationRequestsCount;
    [BsonElement("mgs")] private int _maxGoalStreak;
    [BsonElement("exr")] private int _examsInARow = -1;


    [BsonDefaultValue(false)] [BsonIgnoreIfDefault] [BsonElement("engInterface")]
    private bool _isEnglishInterface;

    [BsonDefaultValue(false)] [BsonIgnoreIfDefault] [BsonElement("interfaceLangChanged")]
    private bool _wasInterfaceLangChanged;

    [BsonDefaultValue(null)]
    [BsonIgnoreIfDefault]
    [BsonElement("lds")]
    public List<DailyStats> LastDaysStats { get; set; }

    [BsonDefaultValue(null)]
    [BsonIgnoreIfDefault]
    [BsonElement("lms")]
    public List<MonthsStats> LastMonthStats { get; set; }

    [BsonDefaultValue(null)]
    [BsonIgnoreIfDefault]
    [BsonElement("kts")]
    public List<UserTrainSet> TrainingSets { get; set; }

    [BsonElement("scr")] private int _totalScore;
    [BsonElement("ldc")] private int _learningDone;
    [BsonElement("qpc")] private int _questionPassed;
    [BsonElement("qfc")] private int _questionFailed;

    //   [BsonElement("oc")] private int _outdatedWordsCount;
    [BsonElement("gs")] private double _gamingScore;

    [BsonElement("fr")] private UserFrequencyState _frequencyState;
    [BsonElement("ntf")] private UserNotificationState _notificationState = new();

    #endregion

    private static readonly HashSet<int> AllowedFreqWordsResults =
        Enum.GetValues<FreqWordResult>().Select(e => (int)e).ToHashSet();

    /// <summary>
    /// Сколько экзаменов (а не изучений) было у пользователей подряд.
    /// 0 - если последним было добавление слов а не экзамен 
    /// </summary>
    public int ExamsInARow
    {
        get => _examsInARow;
        set => _examsInARow = value;
    }

    public UserNotificationState NotificationState => _notificationState;

    /// <summary>
    /// Набор сортированных по порядку выборов частотных слов для пользователя
    /// </summary>
    public IEnumerable<UserFreqWord> OrderedFrequentItems
    {
        get
        {
            if (_frequencyState?.OrderedWords == null)
                return Array.Empty<UserFreqWord>();

            var answer = new List<UserFreqWord>();
            foreach (var word in _frequencyState.OrderedWords)
            {
                if (AllowedFreqWordsResults.Contains(word.Result))
                    answer.Add(new UserFreqWord(word.Number, (FreqWordResult)word.Result));
            }

            return answer;
        }
    }

    public bool IsEnglishInterface
    {
        get => _isEnglishInterface;
        set
        {
            _isEnglishInterface = value;
            _wasInterfaceLangChanged = true;
        }
    }

    public bool WasInterfaceLanguageChanged => _wasInterfaceLangChanged;

    public DateTime Created => Id.CreationTime;
    public DateTime LastActivity => _lastActivity;
    public DateTime LastExam => _lastExam;
    
    public long? TelegramId => _telegramId;
    public string TelegramFirstName => _telegramFirstName;
    public string TelegramLastName => _telegramLastName;
    public string TelegramNick => _telegramNick;
    public UserSource Source => _source;
    public int WordsCount => _wordsCount;
    public int PairsCount => _pairsCount;
    public int ExamplesCount => _examplesCount;
    public int EnglishWordTranslationRequestsCount => _englishWordTranslationRequestsCount;
    public int RussianWordTranslationRequestsCount => _russianWordTranslationRequestsCount;

    public int MaxGoalStreak
    {
        get => _maxGoalStreak;
        set => _maxGoalStreak = value;
    }

    /// <summary>
    /// Количество выученных слов у пользователя
    /// </summary>
    public int WordsLearned =>
        _totalScoreBaskets.BasketCountOfScores((int)WordLeaningGlobalSettings.WellDoneWordMinScore);

    /// <summary>
    /// Количество новых слов у пользователя
    /// </summary>
    public int WordsNewby =>
        _totalScoreBaskets.BasketCountOfScores(0, (int)WordLeaningGlobalSettings.LearningWordMinScore);

    public int LearningDone => _learningDone;
    public int QuestionAsked => _questionPassed + _questionFailed;

    public void OnAnyActivity() => _lastActivity = DateTime.Now;

    private void OnQuestionActivity()
    {
        OnAnyActivity();
        _lastExam = DateTime.Now;
    }

    public void AddFrequentWord(int frequentWordOrderNumber, FreqWordResult status)
    {
        _frequencyState ??= new UserFrequencyState();
        _frequencyState.OrderedWords ??= new List<UserFreqStoredItem>();
        _frequencyState.OrderedWords.Add(new UserFreqStoredItem(frequentWordOrderNumber, (int)status));
        _frequencyState.OrderedWords = _frequencyState.OrderedWords.OrderBy(o => o.Number).ToList();
    }

    public double OnNewWordAdded(WordStatsChange statsChange, int pairsCount, int examplesCount)
    {
        _wordsCount++;
        var (today, month) = FixStatsAndGetCurrent();

        today.WordsAdded++;
        month.WordsAdded++;

        var gamingScore = WordLeaningGlobalSettings.NewWordGamingScore; // * Zen.AddWordsBonusRate;
        today.OnGameScoreIncreased(gamingScore);
        month.OnGameScoreIncreased(gamingScore);
        _gamingScore += gamingScore;

        return gamingScore +
               OnPairsAdded(
                   statsChange: statsChange,
                   pairsCount: pairsCount,
                   examplesCount: examplesCount);
    }

    private (DailyStats, MonthsStats) FixStatsAndGetCurrent()
    {
        var daily = GetToday();
        var monthly = GetLastMonth();
        return (daily, monthly);
    }

    public MonthsStats GetLastMonth()
    {
        if (LastMonthStats == null || LastMonthStats.Count == 0)
            return InitializeLastMonthStats();

        MonthsStats monthly;
        var today = DateTime.Today;

        var thisMonth = new DateTime(today.Year, today.Month, 1);

        monthly = LastMonthStats.Last();
        if (monthly.Date != thisMonth)
        {
            monthly = new MonthsStats { Date = thisMonth };
            LastMonthStats.Add(monthly);
        }

        return monthly;
    }

    private MonthsStats InitializeLastMonthStats()
    {
        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var monthly = new MonthsStats { Date = thisMonth };
        if (LastMonthStats == null)
            LastMonthStats = new List<MonthsStats>();

        LastMonthStats.Add(monthly);
        return monthly;
    }

    public DailyStats GetToday()
    {
        var today = DateTime.Today;
        DailyStats daily;

        if (LastDaysStats == null || LastDaysStats.Count == 0)
        {
            daily = new DailyStats { Date = today };
            LastDaysStats = new List<DailyStats>();
            LastDaysStats.Add(daily);
        }
        else
        {
            while (LastDaysStats.Any() && (today - LastDaysStats[0].Date).TotalDays > 49)
            {
                LastDaysStats.RemoveAt(0);
            }

            if (LastDaysStats.Count == 0)
            {
                daily = new DailyStats { Date = today };
                LastDaysStats = new List<DailyStats>();
                LastDaysStats.Add(daily);
                return daily;
            }

            var lastDay = LastDaysStats.Last();
            if (lastDay.Date != today)
                daily = InitializeTodayStats(today);
            else
                daily = lastDay;
        }

        return daily;
    }

    private DailyStats InitializeTodayStats(DateTime today)
    {
        if (LastDaysStats == null)
            LastDaysStats = new List<DailyStats>();
        var daily = new DailyStats { Date = today };
        LastDaysStats.Add(daily);
        return daily;
    }

    public double OnPairsAdded(WordStatsChange statsChange, int pairsCount, int examplesCount)
    {
        _pairsCount += pairsCount;
        _examplesCount += examplesCount;

        var (today, month) = FixStatsAndGetCurrent();
        today.PairsAdded += pairsCount;
        today.ExamplesAdded += examplesCount;
        month.PairsAdded += pairsCount;
        month.ExamplesAdded += examplesCount;
        var gamingScore = pairsCount * WordLeaningGlobalSettings.NewPairGamingScore; // * Zen.AddWordsBonusRate;
        ApplyWordStatsChanges(statsChange, today, month, gamingScore);
        OnAnyActivity();
        return gamingScore;
    }

    public void OnPairRemoved(UserWordTranslation tr, WordStatsChange wordScoreDelta)
    {
        var examplesCount = tr.Examples?.Length ?? 0;
        _pairsCount--;
        _examplesCount -= examplesCount;
        var (today, month) = FixStatsAndGetCurrent();
        today.PairsAdded--;
        today.ExamplesAdded -= examplesCount;
        month.PairsAdded--;
        month.ExamplesAdded -= examplesCount;

        var gamingScore = -WordLeaningGlobalSettings.NewPairGamingScore; // * Zen.AddWordsBonusRate;
        ApplyWordStatsChanges(wordScoreDelta, today, month, gamingScore);
        OnAnyActivity();
    }

    public void OnWordRemoved(UserWordModel alreadyExistsWord)
    {
        _wordsCount--;
        if (_wordsCount < 0) _wordsCount = 0;

        var (today, month) = FixStatsAndGetCurrent();
        today.WordsAdded--;
        month.WordsAdded--;

        var scoreDelta = UserWordScore.Zero - alreadyExistsWord.Score;
        var gamingScore = -WordLeaningGlobalSettings.NewWordGamingScore; // * Zen.AddWordsBonusRate;

        ApplyWordStatsChanges(scoreDelta, today, month, gamingScore);
    }

    private void ApplyWordStatsChanges(
        WordStatsChange statsChange, DailyStats dailyStats, MonthsStats monthsStats, double gamingScore)
    {
        dailyStats.AppendStats(statsChange);
        monthsStats.AppendStats(statsChange);
        dailyStats.OnGameScoreIncreased(gamingScore);
        monthsStats.OnGameScoreIncreased(gamingScore);
        _gamingScore += gamingScore;
        if (_gamingScore <= 0) _gamingScore = 0;

        if (_totalScoreBaskets == null)
            _totalScoreBaskets = statsChange.Baskets?.ToArray()
                                 ?? Baskets.CreateBaskets();
        else
            _totalScoreBaskets.AddValuesInplace(statsChange.Baskets);
        _totalScoreBaskets.SetLowLimitInplace(0);
    }

    public void OnEnglishWordTranslationRequest()
    {
        _englishWordTranslationRequestsCount++;
        OnAnyActivity();
    }

    public void OnRussianWordTranslationRequest()
    {
        _russianWordTranslationRequestsCount++;
        OnAnyActivity();
    }

    public void OnQuestionPassed(WordStatsChange statsChange)
    {
        _questionPassed++;
        var (today, month) = FixStatsAndGetCurrent();

        today.QuestionsPassed++;
        month.QuestionsPassed++;

        OnQuestionActivity();

        var gamingScore = WordLeaningGlobalSettings.QuestionPassedGamingScore;
        ApplyWordStatsChanges(statsChange, today, month, gamingScore);
    }

    public void OnQuestionFailed(WordStatsChange statsChange)
    {
        _questionFailed++;
        var (today, month) = FixStatsAndGetCurrent();

        today.QuestionsFailed++;
        month.QuestionsFailed++;

        OnQuestionActivity();

        ApplyWordStatsChanges(statsChange, today, month, WordLeaningGlobalSettings.QuestionFailedGamingScore);
    }

    public void OnLearningDone()
    {
        _learningDone++;
        var (today, month) = FixStatsAndGetCurrent();

        today.LearningDone++;
        month.LearningDone++;

        var gamingScore = WordLeaningGlobalSettings.LearningDoneGamingScore; // * Zen.LearnWordsBonusRate;
        ApplyWordStatsChanges(WordStatsChange.Zero, today, month, gamingScore);
        OnAnyActivity();
    }

    public void RecreateTotalStatisticScores(IEnumerable<UserWordModel> allUserWords)
    {
        _totalScoreBaskets = Baskets.CreateBaskets();
        var totalChange = WordStatsChange.Zero;
        foreach (var word in allUserWords)
        {
            totalChange += WordStatsChange.CreateForScore(word.AbsoluteScore);
        }

        _wordsCount = allUserWords.Count();
        _totalScoreBaskets = totalChange.Baskets.ToArray();
    }

    public void RecreateStatistic(IEnumerable<UserWordModel> allUserWords)
    {
        if (_wordsCount == _totalScoreBaskets?.Sum())
            return;

        var days = new Dictionary<DateTime, DailyStats>();
        var months = new Dictionary<DateTime, MonthsStats>();
        _totalScoreBaskets = Baskets.CreateBaskets();
        var totalChange = WordStatsChange.Zero;
        _gamingScore = 0;
        foreach (var word in allUserWords)
        {
            totalChange += WordStatsChange.CreateForScore(word.AbsoluteScore);
            var creationTime = word.Id.CreationTime;
            var day = creationTime.Date;
            if (!days.TryGetValue(day, out var dailyStats))
            {
                dailyStats = new DailyStats()
                {
                    Date = day
                };
                days.Add(day, dailyStats);
            }

            var month = new DateTime(creationTime.Year, creationTime.Month, 1);
            if (!months.TryGetValue(month, out var monthsStats))
            {
                monthsStats = new MonthsStats()
                {
                    Date = month
                };
                months.Add(month, monthsStats);
            }

            var eCount = word.RuTranslations.Select(t => t.Examples.Length).Sum();
            monthsStats.ExamplesAdded += eCount;
            dailyStats.ExamplesAdded += eCount;
            monthsStats.PairsAdded += word.RuTranslations.Length;
            dailyStats.PairsAdded += word.RuTranslations.Length;
            monthsStats.WordsAdded++;
            dailyStats.WordsAdded++;

            dailyStats.QuestionsPassed += word.QuestionPassed;
            monthsStats.QuestionsPassed += word.QuestionPassed;
            dailyStats.QuestionsFailed += word.QuestionAsked - word.QuestionPassed;
            monthsStats.QuestionsFailed += word.QuestionAsked - word.QuestionPassed;

            var wc = WordStatsChange.CreateForScore(word.AbsoluteScore);

            var score = WordLeaningGlobalSettings.NewWordGamingScore +
                        WordLeaningGlobalSettings.NewPairGamingScore * word.RuTranslations.Length +
                        WordLeaningGlobalSettings.AverageScoresForPassedQuestion * word.QuestionPassed +
                        WordLeaningGlobalSettings.QuestionFailedGamingScore *
                        (word.QuestionAsked - word.QuestionPassed);
            ApplyWordStatsChanges(wc, dailyStats, monthsStats, (int)score);
        }

        if (LastDaysStats == null)
            LastDaysStats = new List<DailyStats>();
        else
            LastDaysStats.Clear();

        if (LastMonthStats == null)
            LastMonthStats = new List<MonthsStats>();
        else
            LastMonthStats.Clear();

        foreach (var day in days)
        {
            LastDaysStats.Add(day.Value);
        }

        LastDaysStats.Sort((d1, d2) => d1.Day.CompareTo(d2.Day));

        foreach (var month in months)
        {
            LastMonthStats.Add(month.Value);
        }

        LastMonthStats.Sort((d1, d2) => d1.Months.CompareTo(d2.Months));
        _wordsCount = allUserWords.Count();
        _totalScoreBaskets = totalChange.Baskets.ToArray();
    }
}