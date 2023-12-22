using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable 414

namespace SayWhat.MongoDAL.Words;

[BsonIgnoreExtraElements]
public class UserWordModel {
    public UserWordModel(
        ObjectId userUserId, string word, string translation,
        UserWordType wordType, double rate = 0) {
        _userUserId = userUserId;
        _word = word;
        _currentOrderScore = Math.Pow(
            1.5, rate);
        _absoluteScore = rate;
        _scoreUpdatedTimestamp = DateTime.Now;
        _wordType = wordType;
        RuTranslations = new[] { new UserWordTranslation(translation) };
        
    }

    public UserWordModel(
        ObjectId userId, string word, TranslationDirection direction, double absScore,
        UserWordTranslation translation, UserWordType wordType) {
        _userUserId = userId;
        _word = word;
        _translationDirection = direction;
        _currentOrderScore = Math.Pow(
            1.5, absScore);
        _absoluteScore = absScore;
        _scoreUpdatedTimestamp = DateTime.Now;
        _wordType = wordType;
        RuTranslations = new[] { translation };
    }

    public UserWordModel() { }

    #region mongo fields

    /// <summary>
    /// Last time the question was asked
    /// </summary>
    [BsonElement(UserWordsRepo.LastQuestionAskedTimestampFieldName)]
    private DateTime? _lastQuestionAskedTimestamp;

    public ObjectId Id { get; set; }

    [BsonElement(UserWordsRepo.UserIdFieldName)]
    private ObjectId _userUserId;

    [BsonElement("l")] private TranslationDirection _translationDirection;

    [BsonElement(UserWordsRepo.OriginWordFieldName)]
    private string _word;

    /// <summary>
    /// Current words rate. Include [AbsoluteScore] [AgingFactor] [Randomization]
    /// Index for exam selecting
    /// </summary>
    [BsonElement(UserWordsRepo.CurrentScoreFieldName)]
    private double _currentOrderScore;

    /// <summary>
    /// Absolute words score.
    /// </summary>
    [BsonElement(UserWordsRepo.AbsoluteScoreFieldName)]
    private double _absoluteScore;

    /// <summary>
    /// Number of correctly answered questions 
    /// </summary>
    [BsonElement(UserWordsRepo.QuestionPassedFieldName)]
    private int _questionPassed;

    /// <summary>
    /// Number of asked question 
    /// </summary>
    [BsonElement(UserWordsRepo.QuestionAskedFieldName)]
    private int _questionAsked;

    [BsonElement("tr")] public UserWordTranslation[] RuTranslations { get; set; }

    [BsonElement("t")] [BsonDefaultValue(UserWordType.UsualWord)] [BsonIgnoreIfDefault]
    private UserWordType _wordType = UserWordType.UsualWord;

    /// <summary>
    /// Last updated
    /// </summary>
    [BsonElement(UserWordsRepo.LastUpdateScoreTime)]
    private DateTime _scoreUpdatedTimestamp = DateTime.Now;

    #endregion

    public bool IsWord => _wordType == UserWordType.UsualWord;

    public bool IsPhrase => !IsWord;

    public string Word => _word;

    // current score is increased AgedScore
    // for increase the distance between two values
    public double CurrentOrderScore => Math.Pow(1.5, Score.AgedScore);

    public double AbsoluteScore => _absoluteScore;
    public int QuestionPassed => _questionPassed;
    public int QuestionAsked => _questionAsked;
    public DateTime? LastQuestionAskedTimestamp => _lastQuestionAskedTimestamp;
    public DateTime ScoreUpdatedTimestamp => _scoreUpdatedTimestamp;

    public UserWordScore Score => new UserWordScore(_absoluteScore, LastQuestionAskedTimestamp ?? DateTime.Now);
    public bool HasAnyExamples => RuTranslations.Any(t => t.Examples?.Any() == true);
    public DateTime? LastExam => LastQuestionAskedTimestamp;
    public string AllTranslationsAsSingleString => string.Join(", ", TextTranslations);
    public IEnumerable<string> TextTranslations => RuTranslations.Select(t => t.Word);

    public bool ContainsTranscription(string transcription) =>
        RuTranslations.Any(r => r.Transcription == transcription);

    public IEnumerable<Example> Examples =>
        RuTranslations
            .SelectMany(t => t.Examples)
            .Select(t => t.ExampleOrNull)
            .Where(e => e != null);

    public Example GetRandomExample() =>
        Examples
            .ToList()
            .GetRandomItemOrNull();

    public void OnQuestionPassed(double questionPassScore) {
        _absoluteScore = LearnMechanics.CalculateAbsoluteScoreOnSuccessAnswer(questionPassScore, this);
        _lastQuestionAskedTimestamp = DateTime.Now;
        _questionAsked++;
        _questionPassed++;
        _scoreUpdatedTimestamp = DateTime.Now;
    }

    public void OnQuestionFailed(double questionFailScore) {
        _absoluteScore = LearnMechanics.CalculateAbsoluteScoreOnFailedAnswer(questionFailScore, this);
        _questionAsked++;
        _lastQuestionAskedTimestamp = _scoreUpdatedTimestamp = DateTime.Now;
        _scoreUpdatedTimestamp = DateTime.Now;
    }

    public void RefreshScoreUpdate() {
        //  _currentOrderScore = AbsoluteScore;
        _currentOrderScore = CurrentOrderScore;
        _scoreUpdatedTimestamp = DateTime.Now;
    }

    public void AddTranslations(List<UserWordTranslation> newTranslates) {
        newTranslates.AddRange(RuTranslations);
        RuTranslations = newTranslates.ToArray();
    }

    public UserWordTranslation RemoveTranslation(string translationText) {
        var tr = RuTranslations.FirstOrDefault(i => i.Word.Equals(translationText));
        if (tr != null)
            RuTranslations = RuTranslations.Where(t => t != tr).ToArray();
        return tr;
    }

    public override string ToString() =>
        $"{Word} absolute_score: {AbsoluteScore} current_order_score: {CurrentOrderScore} ages_score:{Score.AgedScore} updated {ScoreUpdatedTimestamp} LastAnswer: {LastQuestionAskedTimestamp}";


    public bool HasTranslation(string translatedText) => RuTranslations.Any(t => t.Word.Equals(translatedText, StringComparison.InvariantCultureIgnoreCase));
}