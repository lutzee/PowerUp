using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace SampleDbMigrations._00_ServerSetup
{
  [Migration(1L)]
  public class CreateDatabase : SetupMigration
  {
    public CreateDatabase(ISqlServerSettings settings)
      : base(settings)
    { }

    public CreateDatabase()
    { }

    public override void Up()
    {
      this.Server.CreateDatabase();
      this.Server.CreateDefaultLogin();
      this.Server.CreateDefaultUser();
      this.Server.DisableFullTextServices();
      this.Server.SetSimpleRecoveryMode();
    }

    public override void Down()
    {
      this.Server.DropDefaultUser();
      this.Server.DropDefaultLogin();
      this.Server.DropDatabase();
    }
  }
}
