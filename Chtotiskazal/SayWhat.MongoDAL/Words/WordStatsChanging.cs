using System;

namespace SayWhat.MongoDAL.Words
{
    public class WordStatsChanging
    {
        public static readonly WordStatsChanging Zero = new WordStatsChanging(); 

        public const int A1LearnScore = 7;
        public const int A2LearnScore = 12;
        public const int A3LearnScore = 22;

        public static WordStatsChanging CreateForNewWord(double absoluteScore)
        {
            var result = new WordStatsChanging();
            if (absoluteScore >= A3LearnScore)      result.A3WordsCountChanging++;
            else if (absoluteScore >= A2LearnScore) result.A2WordsCountChanging++;
            else if (absoluteScore >= A1LearnScore) result.A1WordsCountChanging++;
            else result.A0WordsCountChanging++;
            
            result.LeftToA2Changing  = Math.Min(0, absoluteScore-A2LearnScore);
            return result;
        }

        public static WordStatsChanging Create(double originAbsoluteScore, double resultAbsoluteScore)
        {
            int a0 = 0;
            int a1 = 0;
            int a2 = 0;
            int a3 = 0;
            
            if (originAbsoluteScore >= A3LearnScore)      a3--;
            else if (originAbsoluteScore >= A2LearnScore) a2--;
            else if (originAbsoluteScore >= A1LearnScore) a1--;
            else a0--;
            
            if (resultAbsoluteScore >= A3LearnScore)      a3++;
            else if (resultAbsoluteScore >= A2LearnScore) a2++;
            else if (resultAbsoluteScore >= A1LearnScore) a1++;
            else a0++;


            var originA2Score = Math.Min(A2LearnScore, originAbsoluteScore);
            var resultA2Score = Math.Min(A2LearnScore, resultAbsoluteScore);
            
            return new WordStatsChanging
            {
                A0WordsCountChanging = a0,
                A1WordsCountChanging = a1,
                A2WordsCountChanging = a2,
                A3WordsCountChanging = a3,
                AbsoluteScoreChanging = resultAbsoluteScore - originAbsoluteScore,
                LeftToA2Changing = resultA2Score-originA2Score
            };
            
        }

        private WordStatsChanging()
        {
            
        }

        public WordStatsChanging(int a0, int a1, int a2, int a3, double leftLeftToA2)
        {
            A0WordsCountChanging = a0;
            A1WordsCountChanging = a1;
            A2WordsCountChanging = a2;
            A3WordsCountChanging = a3;
            LeftToA2Changing = leftLeftToA2;
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
    }
}