namespace Chotiskazal.Dal.Migrations
{ 
public class CreateQuestionMetricTable : SimpleMigration
    {
        public override string Name => "CreateQuestionMetricTable";

        public override string  Query => @"create table if not exists QuestionMetric
              (
                 MetricId                                  integer primary key AUTOINCREMENT,
                 ElaspedMs                           integer,
                 Result                              integer,
                 Type                                nvarchar(100) not null,
                 Revision                            integer,
                 PreviousExam                        datetime not null,
                 LastExam                            datetime not null,
                 ExamsPassed                         integer not null,
                 Examed                              integer not null,
                 PassedScore                         integer not null,
                 AggregateScore                      real not null,
                 AggregateScoreBefore                real not null,
                 PassedScoreBefore                   real not null 
              )";
        
    }
}