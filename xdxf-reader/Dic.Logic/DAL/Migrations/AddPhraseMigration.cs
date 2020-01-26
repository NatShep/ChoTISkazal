namespace Dic.Logic.DAL.Migrations
{
    public class AddPhraseMigration : IMigration
    {
        public string Name => "AddPhrase";

        public string Query =>
            @"create table if not exists ContextPhrases
              (
                 Id                                   integer primary key AUTOINCREMENT,
                 OriginWord                           nvarchar(100) not null,
                 Origin                               nvarchar(200) not null,
                 TranslationWord                      nvarchar(100) not null,
                 Translation                          nvarchar(200) not null,
                 Created                              datetime not null,
            )";
    }
}