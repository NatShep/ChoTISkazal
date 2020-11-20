using System.Collections.Generic;
using System.Linq;

namespace SayWhat.Bll.Services
{
    public static class ExamHelper
    {
        public static List<UserWordModel> PrepareExamList(UserWordModel[] learningWords)
        {
            var examsList = new List<UserWordModel>(learningWords.Length * 4);

            //Every learning word appears in test from 2 to 4 times
            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));

            while (examsList.Count > 32)
            {
                examsList.RemoveAt(examsList.Count - 1);
            }

            return examsList;
        }
    }
}