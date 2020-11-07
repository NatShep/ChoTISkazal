using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public int SaveToUserDictionary(UserWordForLearning userWordForLearning)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.ExecuteScalar<int>(
                    @"INSERT INTO UserWords (UserId, EnWord, UserTranslations, Transcription, Created, PhrasesIds, IsPhrase,
                                                 PassedScore, AggregateScore,LastExam,Examed,Revision)

                      VALUES(@UserId,  @EnWord, @UserTranslations, @Transcription, @Created, @PhrasesIds, @IsPhrase,
                                                 @PassedScore, @AggregateScore,@LastExam,@Examed,@Revision)",
                    userWordForLearning);


                return result;
            }
        }

        public UserWordForLearning[] GetWorstForUser(int userId, int count)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            var result = cnn.Query<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId               
				  order by AggregateScore desc limit @count          		
                ", new {userId, count});

            return result.ToArray();
        }

        public UserWordForLearning[] GetWorstTestWordsForUser(int count, int learnRate, int userId)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            return cnn.Query<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId AND PassedScore > @learnRate 
                  order by AggregateScore desc limit @count       		
                ", new {userId, learnRate, count}).ToArray();
        }

        public UserWordForLearning[] GetAllUserWordsForLearning(int userId)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return cnn.Query<UserWordForLearning>(
                @"Select * from UserWords WHERE UserId=userId
                ", new {userId}).ToArray();
        }

        public string[] GetAllWordsForUser(in int userId)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return cnn.Query<string>(
                @"Select EnWord FROM UsersPairs where up.UserId==@userId
                  ", new {userId}).ToArray();
        }

        public void UpdateScores(UserWordForLearning userWordForLearning)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update UserWords set AggregateScore = @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed " +
                    $"WHERE Id = @Id";
                cnn.Execute(op, userWordForLearning);
            }
        }

        public void UpdateAgingAndRandomization(int count)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserWordForLearning>
                    (@"Select * From UserWords order by RANDOM() limit @count", new {count}).ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op = $"Update UserWords set AggregateScore = " +
                             $"{word.AggregateScore.ToString(CultureInfo.InvariantCulture)} " +
                             $"where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }

        public UserWordForLearning GetWordByEnWordOrNull(int userId, string enWord)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            return cnn.Query<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId AND enWord = @enWord       		
                ", new {userId,enWord}).FirstOrDefault();
        }



        //TODO additional methods


        public void UpdateAgingAndRandomization()
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserWordForLearning>(@"Select * From UserPairs").ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op =
                        $"Update UserWords set AggregateScore = {word.AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }


        public void UpdateWordTranslations(UserWordForLearning userWord)
        {

            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();

                var op =
                        $"Update UserWords Set " +
                        $" UserTranslations = @UserTranslations " +
                        $"Where Id = @Id";
                    cnn.Execute(op,userWord);

            }
        }
    }
}