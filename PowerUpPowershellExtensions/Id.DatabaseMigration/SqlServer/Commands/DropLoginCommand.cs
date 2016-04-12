using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class DropLoginCommand : CommandBase
  {
    private const string CheckLoginExistsString = "SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'";
    private const string DropLoginsqlString = "DROP LOGIN {0}";
    private readonly string loginName;

    public DropLoginCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string loginName)
      : base(queryExecuter, settings)
    {
      this.loginName = loginName;
    }

    public override void Execute()
    {
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.MasterConnectionString, string.Format("SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'", (object) this.loginName)) != 1)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("DROP LOGIN {0}", (object) this.loginName));
    }
  }
}
