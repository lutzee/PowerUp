using System;
using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace SampleDbMigrations
{
  [Migration(104L)]
  public class CreateForeignKeys : SetupMigration
  {
    public CreateForeignKeys(ISqlServerSettings settings)
      : base(settings)
    {
    }

    public CreateForeignKeys()
        : base((ISqlServerSettings)null)
    {
    }

    public override void Up()
    {
      this.Database.BeginTransaction();
      try
      {
        this.Database.AddForeignKey("FK_EntryItem_Category", "EntryItem", "CategoryId", "Category", "CategoryId");
        this.Database.AddForeignKey("FK_EntryItem_Location", "EntryItem", "LocationId", "Location", "LocationId");
        this.Database.AddForeignKey("FK_EntryItem_Status", "EntryItem", "StatusId", "Status", "StatusId");
        this.Database.AddForeignKey("FK_EntryItem_Entry", "EntryItem", "EntryId", "Entry", "EntryId");
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
        this.Database.RemoveForeignKey("EntryItem", "FK_EntryItem_Entry");
        this.Database.RemoveForeignKey("EntryItem", "FK_EntryItem_Status");
        this.Database.RemoveForeignKey("EntryItem", "FK_EntryItem_Location");
        this.Database.RemoveForeignKey("EntryItem", "FK_EntryItem_Category");
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
