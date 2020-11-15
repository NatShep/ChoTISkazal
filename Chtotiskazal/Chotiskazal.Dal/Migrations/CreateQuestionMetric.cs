namespace Chotiskazal.Dal.Migrations
{ 
public class CreateQuestionMetricTable : SimpleMigration
    {
        public override string Name => "CreateQuestionMetricTable";

        public override string  Query => @"create table if not exists QuestionMetric
              (
                 MetricId                            integer primary key AUTOINCREMENT,
                 WordId                              integer not null,
                 Created                             datetime not null,
                 PreviousExam                        datetime,                 
                 ElaspedMs                           integer,
                 AggregateScoreBefore                real not null,
                 PassedScoreBefore                   real not null,             
                 ExamsPassed                         integer not null,               
                 Result                              integer,
                 Type                                nvarchar(100) not null           
              )";
        
    }
}