using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users;

public class UserFreqWord
{
    public UserFreqWord()
    {
    }

    public UserFreqWord(int number, FreqWordResult result)
    {
        Number = number;
        Result = result;
    }

    /// <summary>
    /// Частотный номер слова
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Статус выбора
    /// </summary>
    public FreqWordResult Result { get; set; }
}

[BsonIgnoreExtraElements]
public class UserFrequencyState
{
    [BsonElement("w")] public List<UserFreqStoredItem> OrderedWords { get; set; }
}

[BsonIgnoreExtraElements]
public class UserFreqStoredItem
{
    public UserFreqStoredItem()
    {
    }

    public UserFreqStoredItem(int number, int result)
    {
        Number = number;
        Result = result;
    }

    [BsonElement("n")] public int Number { get; set; }
    [BsonElement("r")] public int Result { get; set; }
}

public enum FreqWordResult
{
    /// <summary>
    /// User choose to learn word
    /// </summary>
    UserSelectToLearn = 0,

    /// <summary>
    /// User choose that word is known
    /// </summary>
    UserSelectThatItIsKnown = 10,

    /// <summary>
    /// User choose to skip the word. Such a word does not saves to user model, and resstarts after bot resstart
    /// </summary>
    UserSelectToSkip = 20,

    /// <summary>
    /// User already learning the word 
    /// </summary>
    AlreadyLearning = 30,

    /// <summary>
    /// User already learned the word with bot 
    /// </summary>
    AlreadyLearned = 40,
}