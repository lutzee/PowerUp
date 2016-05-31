using Id.DatabaseMigration.SqlServer;
using System.Data.SqlTypes;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class CreateUserCommand : CommandBase
  {
    private const string DbOwner = "db_owner";
    private const string DbDataReader = "db_datareader";
    private const string CheckUserExistsString = "SELECT Count(*) FROM {0}..sysusers WHERE name = '{1}'";
    private const string AddRoleMemberSpString = "EXEC sp_addrolemember N'{0}', N'{1}'";
    private const string CreateUserSqlString = "CREATE USER [{0}] FOR LOGIN [{0}] WITH DEFAULT_SCHEMA = [{1}]";
    private readonly string userName;
    private readonly bool readOnly;

    public CreateUserCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string userName, bool readOnly)
      : base(queryExecuter, settings)
    {
      this.userName = userName;
      this.readOnly = readOnly;
    }

    public override void Execute()
    {
      string commandString = string.Format("SELECT Count(*) FROM {0}..sysusers WHERE name = '{1}'", (object) this.Settings.DatabaseName, (object) this.userName);
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.DefaultConnectionString, commandString) == 1)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.DefaultConnectionString, string.Format("CREATE USER [{0}] FOR LOGIN [{0}] WITH DEFAULT_SCHEMA = [{1}]", (object) this.userName, (object) this.GetUserSchema()));
      this.QueryExecuter.ExecuteNonQuery(this.Settings.DefaultConnectionString, string.Format("EXEC sp_addrolemember N'{0}', N'{1}'", (object) this.GetUserRole(), (object) this.userName));
      if (this.QueryExecuter.ExecuteScalar<int>(this.Settings.DefaultConnectionString, commandString) != 1)
        throw new SqlNullValueException(this.userName);
    }

    private string GetUserSchema()
    {
      return "dbo";
    }

    private string GetUserRole()
    {
      return !this.readOnly ? "db_owner" : "db_datareader";
    }
  }
}
