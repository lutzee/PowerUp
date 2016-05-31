using System;
using System.Data;
using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace SampleDbMigrations
{
  [Migration(101L)]
  public class CreateReferenceDataTables : SetupMigration
  {
    public CreateReferenceDataTables(ISqlServerSettings settings)
      : base(settings)
    {
    }

    public CreateReferenceDataTables()
        : base((ISqlServerSettings)null)
    {
    }

    public override void Up()
    {
      this.Database.BeginTransaction();
      try
      {
        this.Database.AddTable("Category", new Column("CategoryId", DbType.Int32, ColumnProperty.PrimaryKey), new Column("Name", DbType.AnsiString, 100), new Column("ShortName", DbType.AnsiString, 100), new Column("IsDeleted", DbType.Boolean));
        this.Database.AddTable("Location", new Column("LocationId", DbType.Int32, ColumnProperty.PrimaryKey), new Column("ParentLocationId", DbType.Int32, ColumnProperty.Null), new Column("Name", DbType.AnsiString, 100), new Column("IsDeleted", DbType.Boolean));
        this.Database.AddTable("Status", new Column("StatusId", DbType.Int32, ColumnProperty.PrimaryKey), new Column("Name", DbType.AnsiString, 100), new Column("IsDeleted", DbType.Boolean));
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
        this.Database.RemoveTable("Category");
        this.Database.RemoveTable("Location");
        this.Database.RemoveTable("Status");
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
