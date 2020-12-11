using System;

namespace SayWhat.MongoDAL.Words
{
    public class WordStatsChanging
    {
        public static readonly WordStatsChanging Zero = new WordStatsChanging(); 

        public const int A1LearnScore = 7;
        public const int A3LearnScore = 22;

        public static WordStatsChanging CreateForNewWord(double absoluteScore)
        {
            var result = new WordStatsChanging();
            if (absoluteScore >= A3LearnScore)      result.A3WordsCountChanging++;
            else if (absoluteScore >= WordLeaningGlobalSettings.LearnedWordMinScore) result.A2WordsCountChanging++;
            else if (absoluteScore >= A1LearnScore) result.A1WordsCountChanging++;
            else result.A0WordsCountChanging++;
            
            result.LeftToA2Changing  = Math.Min(0, absoluteScore-WordLeaningGlobalSettings.LearnedWordMinScore);
            return result;
        }

        private WordStatsChanging()
        {
        }

        public WordStatsChanging(
            int a0, int a1, int a2, int a3, 
            double absoluteScoreChanging, 
            double leftLeftToA2, 
            int outdatedChanging)
        {
            A0WordsCountChanging = a0;
            A1WordsCountChanging = a1;
            A2WordsCountChanging = a2;
            A3WordsCountChanging = a3;
            LeftToA2Changing = leftLeftToA2;
            AbsoluteScoreChanging = absoluteScoreChanging;
            OutdatedChanging = outdatedChanging;
        }
        /// <summary>
        /// How many words appears in A0 (new word) zone
        /// </summary>
        public int A0WordsCountChanging { get; private set; }
        /// <summary>
        /// How many words appears in A1 (familiar word) zone
        /// </summary>
        public int A1WordsCountChanging { get; private set; }
        /// <summary>
        /// How many words appears in A2 (learned word) zone
        /// </summary>
        public int A2WordsCountChanging { get; private set; }
        /// <summary>
        /// How many words appears in A3 (well-done) zone
        /// </summary>
        public int A3WordsCountChanging { get; private set; }
        /// <summary>
        /// Changing of absolute score
        /// </summary>
        public double AbsoluteScoreChanging { get; private set; }
        /// <summary>
        /// Absolute score changings for words, that below A2 zone.
        ///
        /// It equals to -{A2LearnScore} for new words
        /// </summary>
        public double LeftToA2Changing { get; private set; }
        
        /// <summary>
        /// Number of outdated word
        ///
        /// Positive: Means that {OutdatedChanging}  become outdated (Score > A2 scores, but Score-time become less than A2)
        /// Negative: Means that {-OutdatedChanging} become fresh(Score > A2 scores, and Score-time become more than A2)
        /// </summary>
        public int OutdatedChanging { get; private set; }
    }
}