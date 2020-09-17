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
        public UsersPair[] GetWorstForUser(User user, int count)
        {
            if (!File.Exists(DbFile))
                return new UsersPair[0];
            using var cnn = SimpleDbConnection();
            cnn.Open();

            var lookup = new Dictionary<string, UsersPair>();

            cnn.Query<UsersPair, WordDictionary, UsersPair>(
            @"Select DISTINCT dw.EnWord, w.* from 
                (SELECT * FROM UserPairs WHERE UserId= @userId limit @count) w 
                LEFT JOIN QuestionMetric q on q.Id=w.MetricId
                LEFT JOIN PairDictionary dw on dw.Id=w.PairId
                ORDER BY q.AggregateScore desc",
            (w, dw) =>
                {
                    var pair = new UsersPair();
                    if (!lookup.TryGetValue(dw.EnWord, out pair))
                        lookup.Add(dw.EnWord, pair = w);
                    return pair;
                }, new { userId=user.UserId, count=count },
            splitOn: "MetricId,PairId");
            return lookup.Values.ToArray();
        }
        
        public string[] GetAllTranslateForUser(User user, int pairId)
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


        public int SaveToUserDictionary(WordDictionary pair, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
