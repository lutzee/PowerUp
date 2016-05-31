using System;
using System.Data.SqlClient;
using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace SampleDbMigrations
{
  [Migration(102L)]
  public class CreateELMAHSchema : SetupMigration
  {
    public CreateELMAHSchema(ISqlServerSettings settings)
      : base(settings)
    {
    }

    public CreateELMAHSchema()
        : base((ISqlServerSettings)null)
    {
    }

    public override void Down()
    {
      this.ExecuteSqlInTransaction(ResourceHelpers.GetStringFromResource("SampleDbMigrations.Resources.ELMAH_SqlServer_TearDown.sql"));
    }

    public override void Up()
    {
      this.ExecuteSqlInTransaction(ResourceHelpers.GetStringFromResource("SampleDbMigrations.Resources.ELMAH_SqlServer_SetUp.sql"));
    }

    private void ExecuteSqlInTransaction(string sql)
    {
      this.Database.BeginTransaction();
      try
      {
        this.Server.ExecuteAdHocCommand(sql);
        this.Database.Commit();
      }
      catch (SqlException ex)
      {
        this.Database.Rollback();
        throw new MigrationException("An exception occured during the CreateELMAHSchema migration", (Exception) ex);
      }
    }
  }
}
