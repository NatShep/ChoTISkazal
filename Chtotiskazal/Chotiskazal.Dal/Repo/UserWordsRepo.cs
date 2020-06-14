using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    class UserWordsRepo: BaseRepo
    {
        public UserWordsRepo(string fileName) : base(fileName) { }       
        public UsersPair[] GetAllWordsForUser(User user)
        {
            //...
            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Query<WordDictionary>(
                @"Select w.*, c.* from 
                (SELECT * FROM UsersWords where (User==@user) uw 
                LEFT JOIN Words w on w.Id=uw.WordId
                LEFT JOIN ContextPhrases c on c.WordId = w.Id");
            return new WordDictionary[0];
        }
        public UsersPair[] GetWordsForUserTests(int count, int learnRate, User user)
        {
            if (!File.Exists(DbFile))
                return new WordDictionary[0];
            return new WordDictionary[0];
        }
        public UsersPair[] GetWorstForUser(int count)
        {
            if (!File.Exists(DbFile))
                return new WordDictionary[0];
            return new WordDictionary[0];
        }
        
        public UsersPair[] GetAllTranslate(User user, string word)
        {
            //...
            return new UsersPair[0];
        }
       
        public void UpdateAgingAndRandomization(int count)
        {
            if (!File.Exists(DbFile))
                return;          
        }
        public void UpdateAgingAndRandomization()
        {
            if (!File.Exists(DbFile))
                return;

        }
        public void UpdateScores(WordDictionary word)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }
        public void UpdateScoresAndTranslation(WordDictionary word)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"Translation = @Translation," +
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"AllMeanings = @AllMeanings," +
                    $"Revision = @Revision," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }
        }
}
