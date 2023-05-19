using System;
/*
public class Zen
{

    public Zen(int[] changings, int outdatedWordsCount)
    {
        int[] changings1 = changings??new int[8];

        var needToBeDone = 0;
        var notLearnedWords = 0;
        for (int i = 0; i < 4; i++)
        {
            needToBeDone+= changings1[i] * ( 4-i);
            notLearnedWords += changings1[i];
        }
        //we got 4 scores per learning (approx)
        // so we need at least {needToBeDone}/4 exams to be done

        var learningCount = needToBeDone / 4;

        var outdateLearnCount = outdatedWordsCount / 3;

        if (learningCount > outdateLearnCount) {
            CountOfLearningNeedToBeDone = learningCount;
        }
        else {
            CountOfLearningNeedToBeDone = learningCount + (outdateLearnCount - learningCount) / 4;
        }
        //we have to have at lesat 30 words for learning
        CountOfWordsNeedToBeAdded += Math.Max(30 - notLearnedWords, 0);
        
        //Zen Equal 0 if CountOfLearningNeedToBeDone== CountOfWordsNeedToBeAdded = 0;
        //Zen is negative if CountOfWordsNeedToBeAdded = 20 && CountOfLearningNeedToBeDone = 0;
        //Zen is positive if CountOfLearningNeedToBeDone = 20 && CountOfWordsNeedToBeAdded = 0;
        Rate = CountOfLearningNeedToBeDone - CountOfWordsNeedToBeAdded;
        if (Rate >= 20) Rate = 20;
        //if zen = 20  rate = 0.5
        //if zen = 0   rate ~ 1
        //if zen = -20 rate = 1.5
        AddWordsBonusRate = -0.025 * Rate + 1;
        LearnWordsBonusRate = 2 - AddWordsBonusRate;
    }
    /// <summary>
    /// if rate is negative - user should add words
    /// if rate is positive - user should learn words
    /// </summary>
    public double Rate { get; }
    public bool NeedToAddNewWords => Rate < -5;
    
    public int CountOfLearningNeedToBeDone { get; }
    public int CountOfWordsNeedToBeAdded { get; }
    public double AddWordsBonusRate { get; } = 1;
    public double LearnWordsBonusRate { get; } = 1;
    
}*/