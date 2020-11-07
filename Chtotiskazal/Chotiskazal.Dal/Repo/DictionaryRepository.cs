using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Dapper;

namespace Chotiskazal.Dal.Repo
{
    public class DictionaryRepository : BaseRepo
    {
        public DictionaryRepository(string fileName) : base(fileName){}
        
        public int AddWordPair(WordDictionary word)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                word.PairId = cnn.ExecuteScalar<int>(
                    @"INSERT INTO PairDictionary (  EnWord,  Transcription,  RuWord, Sourse)
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Sourse); 
                          select last_insert_rowid()", word);
                //TODO Add Phrase if word has them

                return word.PairId;
            }
        }

        public int AddPhrase(int pairId, string enWord, string ruWord, string enPhrase, string ruTranslate)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                var phrase = new Phrase(pairId, enWord, ruWord , enPhrase, ruTranslate);
                cnn.Open();
                return cnn.ExecuteScalar<int>(
                    @"INSERT INTO Phrases ( PairId , EnWord, WordTranslate, EnPhrase, PhraseRuTranslate)
                                      VALUES( @PairId,  @EnWord, @WordTranslate, @EnPhrase,  @PhraseRuTranslate); 
                          select last_insert_rowid()", phrase);
            }
        }

        public WordDictionary[] GetAllWordPairsByWord(string word)
        {
            //TODO find by RuWord or by EnWord.
            //Check the symbol of word before seeking

            if (!File.Exists(DbFile))
                throw new Exception("No Db File");
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT * FROM PairDictionary WHERE EnWord = @word", new {word});
                return result.ToArray();
            }
        }
        
        public Phrase[] GetAllPhrasesByPairId(int wordPairId)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<Phrase>(@"Select * From Phrases WHERE PairId = @wordPairId",new{wordPairId}).ToArray();
            }
        }

        public WordDictionary GetPairWithPhrasesByIdOrNull(int id)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var phrases = new List<Phrase>();
                var result = cnn.Query<WordDictionary, Phrase, WordDictionary>(
                    @"SELECT * FROM 
                    (SELECT * FROM PairDictionary WHERE PairId=@id) pd
                     JOIN Phrases ph ON ph.PairId = pd.PairId",
                    (pd, ph) =>
                    {
                        //TODO почему он не мапит Id для фразы???
                       phrases.Add(ph);
                       return pd;
                    }, new {id},
                    splitOn:"PairId").FirstOrDefault();
                result.Phrases = phrases;
                return result;
            }
            return null;
        }
        
        public string[] GetAllTranslate(string word)
        {
            //TODO find by RuWord or by EnWord.
            //if word == RuWord(английские символы), ищем в таблице WordPairDictionary по русскому слову все переводы
            //if word == EnWord(русские символы), ищем в таблице WordPairDictionary по английскому слову все переводы
          
            CheckDbFile.Check(DbFile);
            //TODO Is it need distinct?
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<string>(
                    @"SELECT RuWord FROM PairDictionary  WHERE  EnWord=@word", new {word}).ToArray();
            }
        }

        public void RemovePhrase(int phraseId)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var sqlQuery = "DELETE FROM Phrases WHERE Id = @phrasesId";
                cnn.Execute(sqlQuery, new { phraseId });
            }
        }
        
        public Phrase[] FindSeveralPhraseById(int[] allPhrasesIdForUser)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<Phrase>(
                    @"SELECT * FROM Phrases 
                    WHERE PairId in @allPhrasesIdForUser", allPhrasesIdForUser);
                return result.ToArray();
            }
        }
        
        //TODO Additional methods 
        public WordDictionary GetPairByIdOrNull(int? id)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT * FROM PairDictionary 
                    WHERE PairId = @id", new {id}).FirstOrDefault();
                return result;
            }
        }

      
    }
}
