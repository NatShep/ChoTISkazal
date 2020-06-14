using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using Chotiskazal.Dal.Logic;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;
using Dapper;

namespace Chotiskazal.Dal.Repo
{
    public class DictionaryRepository: BaseRepo
    {
        public DictionaryRepository(string fileName): base(fileName) { }
       
        public WordPairDictionary[] GetAllWordPairs()
        {
            if(!File.Exists(DbFile))
                return new PairModel[0];

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<PairModel>(@"Select * From Words order by PassedScore").ToArray();
            }
        }
        public Phrase[] GetAllPhrasesOrNull(WordPairDictionary wordPair)
        {
            if (!File.Exists(DbFile))
                return new Phrase[0];

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<Phrase>(@"Select * From ContextPhrases").ToArray();
            }
        }     
        
        public void AddWordPair(WordPairDictionary word)
        {
            if (!File.Exists(DbFile))
            {
                ApplyMigrations();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                word.Id = cnn.Query<long>(
                    @"INSERT INTO Words (  OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed, AllMeanings,Revision )
                                      VALUES( @OriginWord,  @Translation,  @Transcription, @Created, @LastExam, @PassedScore, @AggregateScore, @Examed, @AllMeanings, @Revision  ); 
                          select last_insert_rowid()", word).First();

                if (word.Phrases != null)
                {
                    foreach (var phrase in word.Phrases)
                    {
                        phrase.Created = DateTime.Now;
                        cnn.Execute(
                            @"INSERT INTO ContextPhrases ( Origin,  Translation,  Created, OriginWord, TranslationWord)   
                                      VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord)", phrase);
                    }
                }
            }
        }
        public void AddPhrases(WordPairDictionary word, Phrase[] phrases)
        {

        }
        public int GetContextPhrasesCount()
        {
            if (!File.Exists(DbFile))
                return 0;

            using var cnn = SimpleDbConnection();

            cnn.Open();
            return cnn.Query<int>(@"Select count(*) From ContextPhrases").FirstOrDefault();
        }

        public WordPairDictionary GetWordPairOrNullByWord(string word)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordPairDictionary>(
                    @"SELECT * FROM Words WHERE OriginWord = @word", new { word }).FirstOrDefault();
                return result;
            }
        }
        public WordPairDictionary GetWordPairOrNullbyTranslate(string translate)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordPairDictionary>(
                    @"SELECT * FROM Words WHERE Translate = @translate", new { translate }).FirstOrDefault();
                return result;
            }
        }
        public WordPairDictionary GetWordPairOrNull(int id)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordPairDictionary>(
                    @"SELECT Id, OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed
            FROM Words
            WHERE Id = @id", new {id}).FirstOrDefault();
                return result;
            }
        }

      
    }
}
