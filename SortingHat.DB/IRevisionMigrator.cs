namespace SortingHat.DB
{
    internal interface IRevisionMigrator
    {
        void Migrate(SQLiteDatabase db);
    }
}