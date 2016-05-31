using System;
using System.Data;
using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace SampleDbMigrations
{
  [Migration(103L)]
  public class CreateEntryTables : SetupMigration
  {
    public CreateEntryTables(ISqlServerSettings settings)
      : base(settings)
    { }

    public CreateEntryTables()
        : base((ISqlServerSettings)null)
    { }

    public override void Up()
    {
      this.Database.BeginTransaction();
      try
      {
        this.Database.AddTable("Entry", new Column("EntryId", DbType.Guid, ColumnProperty.PrimaryKey), new Column("Name", DbType.String, (int) byte.MaxValue), new Column("EmailAddress", DbType.AnsiString, (int) byte.MaxValue), new Column("EntryDate", DbType.DateTime), new Column("IsDefaultEntry", DbType.Boolean));
        this.Database.AddTable("EntryItem", new Column("EntryItemId", DbType.Guid, ColumnProperty.PrimaryKey), new Column("EntryId", DbType.Guid), new Column("ItemName", DbType.String, (int) byte.MaxValue), new Column("CategoryId", DbType.Int32), new Column("LocationId", DbType.Int32), new Column("Store", DbType.String, (int) byte.MaxValue), new Column("URL", DbType.String, 1000, ColumnProperty.Null), new Column("IsWinner", DbType.Boolean), new Column("WinnerPhoto", DbType.Binary, int.MaxValue, ColumnProperty.Null), new Column("Quote", DbType.String, 1000, ColumnProperty.Null), new Column("StatusId", DbType.Int32));
        this.Database.Commit();
      }
      catch (Exception ex)
      {
        this.Database.Rollback();
        throw new MigrationException("", ex);
      }
    }

    public override void Down()
    {
      this.Database.BeginTransaction();
      try
      {
        this.Database.RemoveTable("EntryItem");
        this.Database.RemoveTable("Entry");
        this.Database.Commit();
      }
      catch (Exception ex)
      {
        this.Database.Rollback();
        throw new MigrationException("", ex);
      }
    }
  }
}
