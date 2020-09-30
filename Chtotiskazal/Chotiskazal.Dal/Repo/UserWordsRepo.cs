using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    public class UserWordsRepo : BaseRepo
    {
        public UserWordsRepo(string fileName) : base(fileName)
        {
        }

        public int SaveToUserDictionary(int pairId, int userId)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                using (var transaction = cnn.BeginTransaction())
                {
                    var metricId = cnn.ExecuteScalar<int>(
                        @"INSERT INTO QuestionMetric (ElaspedMs, Result, Type, Revision, PreviousExam, 
                          LastExam, ExamsPassed, Examed, PassedScore, AggregateScore, AggregateScoreBefore, PassedScoreBefore)
                          VALUES(@ElaspedMs, @Result, @Type, @Revision, @PreviousExam, @LastExam, @ExamsPassed, 
                          @Examed, @PassedScore, @AggregateScore, @AggregateScoreBefore, @PassedScoreBefore); 
                          select last_insert_rowid()", new QuestionMetric());

                    var result= cnn.ExecuteScalar<int>(
                        @"INSERT INTO UserPairs ( UserId ,  PairId, MetricId, Created, IsPhrase)
                      VALUES( @UserId,  @PairId, @MetricId, @Created, @IsPhrase); 
                          select last_insert_rowid()", new UserPair(userId, pairId, metricId, false));
                    
                    transaction.Commit();
                    return result;
                }
            }
        }

        public UserPair GetPairByDicIdOrNull(User user, int id)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<UserPair>(
                    @"SELECT * FROM UserPairs 
                    WHERE UserId = @UserId AND PairId=@id", new {user.UserId, id}).FirstOrDefault();
            }
        }
        
        //TODO
        public UserPair[] GetWorstForUser(User user, int count)
        {
            CheckDbFile.Check(DbFile);
            
            using var cnn = SimpleDbConnection();
            cnn.Open();

            var lookup = new Dictionary<string, UserPair>();

            cnn.Query<UserPair, WordDictionary, UserPair>(
                @"Select DISTINCT dw.EnWord, w.* from 
                (SELECT * FROM UserPairs WHERE UserId= @userId limit @count) w 
                LEFT JOIN QuestionMetric q on q.Id=w.MetricId
                LEFT JOIN PairDictionary dw on dw.Id=w.PairId
                ORDER BY q.AggregateScore desc",
                (w, dw) =>
                {
                    var pair = new UserPair();
                    if (!lookup.TryGetValue(dw.EnWord, out pair))
                        lookup.Add(dw.EnWord, pair = w);
                    return pair;
                }, new {userId = user.UserId, count = count},
                splitOn: "MetricId,PairId");
            return lookup.Values.ToArray();
        }
        //TODO
        public string[] GetAllTranslateForWord(User user, string word)
        {
            //...
            return new string[0];
        }
        //TODO
        public UserPair[] GetWordsForUserTests(int count, int learnRate, User user)
        {
            if (!File.Exists(DbFile))
                return new UserPair[0];
            return new UserPair[0];
        }
        //TODO
        public UserPair[] GetAllWordsForUser(User user)
        {
            //...
            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Query<UserPair>(
                @"Select w.*, c.* from 
                (SELECT * FROM UsersWords where (User==@user) uw 
                LEFT JOIN Words w on w.Id=uw.WordId
                LEFT JOIN ContextPhrases c on c.WordId = w.Id");
            return new UserPair[0];
        }
    }
}
