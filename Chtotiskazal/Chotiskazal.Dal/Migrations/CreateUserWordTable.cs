namespace Chotiskazal.Dal.Migrations
{
    public class CreateUserWordTable : SimpleMigration
    {
        public override string Name => "CreateUserWordTable";

        public override string  Query => @"create table if not exists UserWords
              (
                 Id                            integer primary key AUTOINCREMENT,
                 UserId                              integer not null,
                 EnWord                              nvarchar(100) not null,
                 UserTranslations                    nvarchar(100) not null,
                 Transcription                       nvarchar(100) not null,
                 Created                             datetime not null,
                 Phrases                             nvarchar(100) not null,
                 PassedScore                         real not null,             
                 AggregateScore                      double ,
                 LastExam                            integer not null,               
                 Examed                              integer,
                 Revision                            int not null,               
              )";
        
    }
}