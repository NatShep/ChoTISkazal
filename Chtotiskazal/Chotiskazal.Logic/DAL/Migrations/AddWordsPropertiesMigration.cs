namespace Chotiskazal.Logic.DAL.Migrations
{
    public class AddWordsPropertiesMigration : SimpleMigration
    {
        public override string Name => "AddWordsPropertiesMigration";
        public override string Query => @"ALTER TABLE Words ADD COLUMN AllMeanings text null;
ALTER TABLE Words ADD COLUMN revision integer not null default(0);";
    }
}