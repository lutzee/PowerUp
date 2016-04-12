using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class DropDatabaseCommand : CommandBase
  {
    private const string DropCloseConnDatabaseSqlString = "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
    private const string DropDatabaseSqlString = "DROP DATABASE {0}";

    public DropDatabaseCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings)
      : base(queryExecuter, settings)
    {
    }

    public override void Execute()
    {
      if (!this.DatabaseExists)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", (object) this.Settings.DatabaseName));
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("DROP DATABASE {0}", (object) this.Settings.DatabaseName));
    }
  }
}
