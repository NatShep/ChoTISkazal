using System;
using Chotiskazal.Bot;

namespace PureVocabBuilder;

public static class InputConfigurationReader{

    public static BotSettings ReadConfiguration() {
        try
        {
            var set = new BotSettings();
            Console.WriteLine("DEBUG SETTINGS APPLIED");
            set.MongoConnectionString = "mongodb://localhost:27017/";
            set.MongoDbName = "backuped";//;
            set.YadicapiKey = "<key>";
            set.YadicapiTimeout = TimeSpan.FromSeconds(5);
            
            return set;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}