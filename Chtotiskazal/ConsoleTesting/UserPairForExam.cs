using Chotiskazal.Dal;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTesting
{
    class UserPairForExam
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public User User { get; set; }
        public WordDictionary Pair { get; set; }
        public bool IsPhrase { get; set; }
        public QuestionMetric Metric { get; set; }

        public string TranslationForExam { get; set; }


        public UserPairForExam CreateFromBaseUserPair(UserPair userPair, User user, QuestionMetric metric)
        {
            var userPairForExam = new UserPairForExam();
            return userPairForExam;

        }
        //Set translations for Exams
        //(the number of translations is _maxTranslationSize in UserWordServices

        public void SetTranslations(string[] translations)
        {
            TranslationForExam = string.Join(", ", translations);
        }
    }
}
