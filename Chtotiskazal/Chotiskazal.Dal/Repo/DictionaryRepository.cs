using System;

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
                word.Id = cnn.ExecuteScalar<int>(
                    @"INSERT INTO PairDictionary (  EnWord,  Transcription,  RuWord, Sourse)
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Sourse); 
                          select last_insert_rowid()", word);

                //TODO 
                if (word.Phrases != null)
                {
                    foreach (var phrase in word.Phrases)
                    {
                        //     cnn.Execute(
                        //       @"INSERT INTO ContextPhrases ( Origin,  Translation,  Created, OriginWord, TranslationWord)   
                        //               VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord)", phrase);
                    }
                }
                //TODO
                
                return word.Id;
            }
        }

        public int AddPhrase(int pairId, string enPhrase, string ruTranslate)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                var phrase = new Phrase(pairId, enPhrase, ruTranslate);
                cnn.Open();
                return cnn.ExecuteScalar<int>(
                    @"INSERT INTO Phrases ( PairId ,  EnPhrase, RuTranslate)
                                      VALUES( @PairId,  @EnPhrase,  @RuTranslate); 
                          select last_insert_rowid()", phrase);
            }
        }

        public WordDictionary[] GetAllWordPairsByWord(string word)
        {
            //TODO
            //We can find by RuWord or by EnWord.
            //Check the symbol of word before seeking
            //now just for EnWord!!!
            //TODO

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
                return cnn.Query<Phrase>(@"Select * From Phrases WHERE PairId = @wordPairId  ").ToArray();
            }
        }

        public WordDictionary GetPairByIdOrNull(int? id)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT * FROM PairDictionary 
                    WHERE Id = @id", new {id}).FirstOrDefault();
                return result;
            }
        }

        //TODO
        //need to test
        //TODO
        public WordDictionary GetPairWithPhrasesById(int? id)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary, Phrase, WordDictionary>(
                    @"SELECT * FROM PairDictionary PD WHERE Id = @id
                    JOIN Phrases Ph ON Ph.PairId = PD.Id ", 
                    (wd, ph) =>
                    {
                        wd.Phrases.Add(ph);
                        return wd;
                    });
            }
            return null;
        }
        //TODO
        public string[] GetAllTranslate(string word)
        {
            //if word == RuWord(английские символы), ищем в таблице WordPairDictionary по русскому слову все переводы
            //if word == EnWord(русские символы), ищем в таблице WordPairDictionary по русскому слову все переводы
            return new string[0];
        }
    }
}
