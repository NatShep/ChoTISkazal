namespace Chotiskazal.Logic.DAL.Migrations
{
    interface IMigration
    {
        string Name { get; }
        string Query { get; }
    }
}
