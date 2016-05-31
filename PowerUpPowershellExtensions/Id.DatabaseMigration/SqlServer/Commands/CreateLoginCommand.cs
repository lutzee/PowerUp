using Id.DatabaseMigration.SqlServer;
using System.Data.SqlTypes;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class CreateLoginCommand : CommandBase
  {
    private const string CheckLoginExistsString = "SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'";
    private const string CreateLoginSqlString = "CREATE LOGIN [{0}] WITH PASSWORD = N'{1}', CHECK_POLICY = OFF";
    private readonly string loginName;
    private readonly string loginPassword;

    public CreateLoginCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string loginName, string loginPassword)
      : base(queryExecuter, settings)
    {
      this.loginName = loginName;
      this.loginPassword = loginPassword;
    }

    public override void Execute()
    {
      string commandString = string.Format("SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'", (object) this.loginName);
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.MasterConnectionString, commandString) == 1)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("CREATE LOGIN [{0}] WITH PASSWORD = N'{1}', CHECK_POLICY = OFF", (object) this.loginName, (object) this.loginPassword));
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.MasterConnectionString, commandString) != 1)
        throw new SqlNullValueException(this.loginName);
    }
  }
}
