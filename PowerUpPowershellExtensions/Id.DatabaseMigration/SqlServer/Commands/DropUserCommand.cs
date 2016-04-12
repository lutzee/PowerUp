using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class DropUserCommand : CommandBase
  {
    private const string CheckUserExistsString = "SELECT Count(*) FROM {0}..sysusers WHERE name = '{1}'";
    private const string DropUserSqlString = "DROP USER {0}";
    private readonly string userName;

    public DropUserCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string userName)
      : base(queryExecuter, settings)
    {
      this.userName = userName;
    }

    public override void Execute()
    {
      if (!this.DatabaseExists || this.QueryExecuter.ExecuteScalar<int>(this.Settings.DefaultConnectionString, string.Format("SELECT Count(*) FROM {0}..sysusers WHERE name = '{1}'", (object) this.Settings.DatabaseName, (object) this.userName)) != 1)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.DefaultConnectionString, string.Format("DROP USER {0}", (object) this.userName));
    }
  }
}
