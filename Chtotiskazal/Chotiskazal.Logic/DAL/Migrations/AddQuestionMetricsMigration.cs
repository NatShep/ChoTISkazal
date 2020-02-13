namespace Chotiskazal.Logic.DAL.Migrations
{
    public class AddQuestionMetricsMigration : IMigration
    {
        public string Name => "AddQuestionMetrics";

        public string  Query => @"create table if not exists QuestionMetrics
              (
                 Id                                   integer primary key AUTOINCREMENT,
                 WordId                               integer not null,
                 Created                              datetime not null,
                 PreviousExam                         datetime not null,
                 WordAdded                            datetime not null,
                 
                 ElaspedMs                            integer not null,
                 AggregateScoreBefore                 real not null,
                 PhrasesCount                         integer not null,
                 PassedScoreBefore                    real not null,
                 ExamsPassed integer not null,
                 
                 Result integer not null,
                 Type nvarchar(30) null
              )";
    }
}
