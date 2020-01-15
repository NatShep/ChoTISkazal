namespace Dic.Logic.DAL.Migrations
{
    public class AddHistoryMigration : IMigration
    {
        public string Name => "AddHistory";

        public string Query => 
            @"create table if not exists ExamHistory
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 Count                               integer not null,
                 Passed                              integer not null,
                 Failed                              integer not null,
                 Started                             datetime not null,
                 Finished                            datetime null
            )";
    }
}