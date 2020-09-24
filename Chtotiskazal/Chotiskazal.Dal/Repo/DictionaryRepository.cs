using System;

using System.IO;
using System.Linq;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Dapper;

namespace Chotiskazal.Dal.Repo
{
    public class DictionaryRepository: BaseRepo
    {
        public DictionaryRepository(string fileName): base(fileName) { }
       
        public WordDictionary[] GetAllWordPairs()
        {
            if(!File.Exists(DbFile))
                return new WordDictionary[0];

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<WordDictionary>(@"Select * From Words order by PassedScore").ToArray();
            }
        }

        //Get all Phreases for wordPair(or for Word?)
        public Phrase[] GetAllPhrasesForWordOrNull(string word)
        {
            if (!File.Exists(DbFile))
                return new Phrase[0];

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<Phrase>(@"Select * From ContextPhrases").ToArray();
            }
        }     

        //Get All translate for RuWord or EngWord
        public string[] GetAllTranslate(string word)
        {
            //if word == RuWord(английские символы), ищем в таблице WordPairDictionary по русскому слову все переводы
            //if word == EnWord(русские символы), ищем в таблице WordPairDictionary по русскому слову все переводы

            return new string[0];
        }
        
        public int AddWordPair(WordDictionary word)
        {
            if (!File.Exists(DbFile))
            {
                ApplyMigrations();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                word.Id = cnn.ExecuteScalar<int>(
                    @"INSERT INTO PairDictionary (  EnWord,  Transcription,  RuWord, Sourse)
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Sourse); 
                          select last_insert_rowid()", word);
                
                if (word.Phrases != null)
                {
                    foreach (var phrase in word.Phrases)
                    {
                   //     cnn.Execute(
                     //       @"INSERT INTO ContextPhrases ( Origin,  Translation,  Created, OriginWord, TranslationWord)   
                       //               VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord)", phrase);
                    }
                }
                return word.Id;
            }
        }

        public int AddPhrase(int pairId, string enPhrase, string ruTranslate)
        {
            using (var cnn = SimpleDbConnection())
            {
                var phrase = new Phrase(pairId,enPhrase,ruTranslate);
                cnn.Open();
                return cnn.ExecuteScalar<int>(
                    @"INSERT INTO Phrases ( PairId ,  EnPhrase, RuTranslate)
                                      VALUES( @PairId,  @EnPhrase,  @RuTranslate); 
                          select last_insert_rowid()", phrase);
            }
        }

        public WordDictionary[] GetWordPairOrNullByWord(string word)
        {
            //We can finf by RuWord or by EnWord.
            //Check the symbol of word before seeking
            //now just for EnWord!!!
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT * FROM PairDictionary WHERE EnWord = @word", new { word });
                return result.ToArray();
            }
        }
        public WordDictionary GetWordPairOrNull(int id)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT Id, OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed
            FROM Words
            WHERE Id = @id", new {id}).FirstOrDefault();
                return result;
            }
        }

      
    }
}
