using System;

public class Zen
{
    private readonly int _outdatedWordsCount;
    private readonly int[] _changings;

    public Zen(int[] changings, int outdatedWordsCount)
    {
        _outdatedWordsCount = outdatedWordsCount;
        _changings = changings??new int[8];

        var needToBeDone = 0;
        var notLearnedWords = 0;
        for (int i = 0; i < 4; i++)
        {
            needToBeDone+= changings[i] * ( 4-i);
            notLearnedWords += changings[i];
        }
        //we got 4 scores per learning (approx)
        // so we need at least {needToBeDone}/4 exams to be done

        var learningCount = needToBeDone / 4;

        var outdateLearnCount = outdatedWordsCount / 2;

        if (learningCount > outdateLearnCount) {
            CountOfLearningNeedToBeDone = learningCount;
        }
        else {
            CountOfLearningNeedToBeDone = learningCount + (outdateLearnCount - learningCount) / 4;
        }
        //we have to have at leat 20 words for learning
        CountOfWordsNeedToBeAdded += Math.Max(20 - notLearnedWords, 0);
        
        //Zen Equal 0 if CountOfLearningNeedToBeDone== CountOfWordsNeedToBeAdded = 0;
        //Zen is negative if CountOfWordsNeedToBeAdded = 20 && CountOfLearningNeedToBeDone = 0;
        //Zen is positive if CountOfLearningNeedToBeDone = 20 && CountOfWordsNeedToBeAdded = 0;
        Rate = CountOfLearningNeedToBeDone - CountOfWordsNeedToBeAdded;
        if (Rate >= 20) Rate = 20;
        //if zen = 20  rate = 1
        //if zen = 0   rate ~ 2
        //if zen = -20 rate = 3
        AddWordsBonusRate = (int)Math.Round(-0.05 * Rate + 2);
        LearnWordsBonusRate = (3 - AddWordsBonusRate) + 1;
    }
    /// <summary>
    /// if rate is negative - user should add words
    /// if rate is positive - user should learn words
    /// </summary>
    public double Rate { get; }
    public int CountOfLearningNeedToBeDone { get; }
    public int CountOfWordsNeedToBeAdded { get; }
    public int AddWordsBonusRate { get; } = 1;
    public int LearnWordsBonusRate { get; } = 1;
    
}