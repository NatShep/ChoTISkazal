namespace Chotiskazal.Logic.DAL.Migrations
{
    public class AddPhraseMigration : SimpleMigration
    {
        public override string Name => "AddPhrase";

        public override string Query =>
            @"create table if not exists ContextPhrases
              (
                 Id                                   integer primary key AUTOINCREMENT,
                 OriginWord                           nvarchar(100) not null,
                 Origin                               nvarchar(200) not null,
                 TranslationWord                      nvarchar(100) not null,
                 Translation                          nvarchar(200) not null,
                 Created                              datetime not null
            )";
    }
}