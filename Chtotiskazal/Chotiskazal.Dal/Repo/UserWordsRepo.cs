using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    public class UserWordsRepo: BaseRepo
    {
        public UserWordsRepo(string fileName) : base(fileName) { }       
        public UsersPair[] GetAllWordsForUser(User user)
        {
            //...
            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Query<UsersPair>(
                @"Select w.*, c.* from 
                (SELECT * FROM UsersWords where (User==@user) uw 
                LEFT JOIN Words w on w.Id=uw.WordId
                LEFT JOIN ContextPhrases c on c.WordId = w.Id");
            return new UsersPair[0];
        }
        public UsersPair[] GetWordsForUserTests(int count, int learnRate, User user)
        {
            if (!File.Exists(DbFile))
                return new UsersPair[0];
            return new UsersPair[0];
        }
        public UsersPair[] GetWorstForUser(int count)
        {
            if (!File.Exists(DbFile))
                return new UsersPair[0];
            return new UsersPair[0];
        }
        
        public string[] GetAllTranslateForUser(User user, string word)
        {
            //...
            return new string[0];
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
     
        public void UpdateScores(WordPairDictionary word)
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
        public void UpdateScoresAndTranslation(WordPairDictionary word)
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
