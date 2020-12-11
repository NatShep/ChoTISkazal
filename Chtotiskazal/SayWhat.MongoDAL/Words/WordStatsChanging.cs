using System;

namespace SayWhat.MongoDAL.Words
{
    public class WordStatsChanging
    {
        public static readonly WordStatsChanging Zero = new WordStatsChanging(); 
        private const int MaxObservingScore = 7;

        public static int CategorizedScore(double absoluteScore)
        {
            int normalizedAbsScore = (int) absoluteScore;
            return normalizedAbsScore >= MaxObservingScore 
                ? MaxObservingScore 
                : normalizedAbsScore;
        }

        public static WordStatsChanging CreateForNewWord(double absoluteScore)
        {
            var changings = new int[MaxObservingScore+1];
            changings[CategorizedScore(absoluteScore)]++;
            return new WordStatsChanging(changings,absoluteScore,WordLeaningGlobalSettings.LearnedWordMinScore,0);
        }

        public static WordStatsChanging operator +(WordStatsChanging lstats, WordStatsChanging rstats)
        {
            if (lstats == null)
                return rstats;
            if (rstats == null)
                return lstats;
            return new WordStatsChanging(
                lstats.WordScoreChangings.Sum(rstats.WordScoreChangings),
                lstats.AbsoluteScoreChanging + rstats.AbsoluteScoreChanging,
                lstats.LeftToA2Changing + rstats.LeftToA2Changing,
                lstats.OutdatedChanging + rstats.OutdatedChanging);
        }

        private WordStatsChanging()
        {
        }

        public WordStatsChanging(
            int[] wordScoreChangings,
            double absoluteScoreChanging, 
            double leftLeftToA2, 
            int outdatedChanging)
        {
            WordScoreChangings = wordScoreChangings;
            AbsoluteScoreChanging = absoluteScoreChanging;
            OutdatedChanging = outdatedChanging;
            LeftToA2Changing = leftLeftToA2;
        }
        public int[] WordScoreChangings { get; private set; }
        /*/// <summary>
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
        /// Absolute score changings for words, that below A2 zone.
        ///
      */
        /// <summary>
        /// Changing of absolute score
        /// </summary>
        /// It equals to -{A2LearnScore} for new words
        /// </summary>
        public double LeftToA2Changing { get; }
        public double AbsoluteScoreChanging { get; }

        public int CountOf(int minimumScoreCategory, int maximumScoreCategory)
        {
            var acc = 0;
            for (int i = minimumScoreCategory; i < WordScoreChangings.Length && i< maximumScoreCategory; i++)
            {
                acc+=WordScoreChangings[i];
            }
            return acc;
        }
        /// <summary>
        /// Number of outdated word
        ///
        /// Positive: Means that {OutdatedChanging}  become outdated (Score > A2 scores, but Score-time become less than A2)
        /// Negative: Means that {-OutdatedChanging} become fresh(Score > A2 scores, and Score-time become more than A2)
        /// </summary>
        public int OutdatedChanging { get; }
    }
}