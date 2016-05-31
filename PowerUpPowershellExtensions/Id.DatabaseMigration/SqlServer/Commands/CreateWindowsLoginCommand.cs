using Id.DatabaseMigration.SqlServer;
using System.Data.SqlTypes;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class CreateWindowsLoginCommand : CommandBase
  {
    private const string CheckLoginExistsString = "SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'";
    private const string CreateLoginSqlString = "CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[{1}], DEFAULT_LANGUAGE=[us_english]";
    private readonly string loginName;

    public CreateWindowsLoginCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string loginName)
      : base(queryExecuter, settings)
    {
      this.loginName = loginName;
    }

    public override void Execute()
    {
      string commandString = string.Format("SELECT Count(*) FROM sys.syslogins WHERE [name]='{0}'", (object) this.loginName);
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.MasterConnectionString, commandString) == 1)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[{1}], DEFAULT_LANGUAGE=[us_english]", (object) this.loginName, (object) this.Settings.DatabaseName));
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.MasterConnectionString, commandString) != 1)
        throw new SqlNullValueException(this.loginName);
    }
  }
}
