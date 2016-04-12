using Id.DatabaseMigration;
using Id.DatabaseMigration.SqlServer.Commands;

namespace Id.DatabaseMigration.SqlServer
{
  public class SqlServerAdministrator : IServerAdministrator
  {
    private readonly IQueryExecuter queryExecuter;
    private readonly ISqlServerSettings settings;

    internal SqlServerAdministrator(IQueryExecuter queryExecuter, ISqlServerSettings settings)
    {
      this.queryExecuter = queryExecuter;
      this.settings = settings;
    }

    public SqlServerAdministrator(ISqlServerSettings settings)
      : this((IQueryExecuter) new SqlServerQueryExecuter(), settings)
    {
    }

    public void CreateDatabase()
    {
      new CreateDatabaseCommand(this.queryExecuter, this.settings).Execute();
    }

    public void CreateDefaultLogin()
    {
      if (string.IsNullOrEmpty(this.settings.DefaultUserName))
        return;
      new CreateLoginCommand(this.queryExecuter, this.settings, this.settings.DefaultUserName, this.settings.DefaultUserPassword).Execute();
    }

    public void CreateDefaultUser()
    {
      if (string.IsNullOrEmpty(this.settings.DefaultUserName))
        return;
      new CreateUserCommand(this.queryExecuter, this.settings, this.settings.DefaultUserName, false).Execute();
    }

    public void DropDefaultUser()
    {
      if (string.IsNullOrEmpty(this.settings.DefaultUserName))
        return;
      new DropUserCommand(this.queryExecuter, this.settings, this.settings.DefaultUserName).Execute();
    }

    public void DropDefaultLogin()
    {
      if (string.IsNullOrEmpty(this.settings.DefaultUserName))
        return;
      new DropLoginCommand(this.queryExecuter, this.settings, this.settings.DefaultUserName).Execute();
    }

    public void DropDatabase()
    {
      new DropDatabaseCommand(this.queryExecuter, this.settings).Execute();
    }

    public void DisableFullTextServices()
    {
      new DisableFullTextServiceCommand(this.queryExecuter, this.settings).Execute();
    }

    public void SetSimpleRecoveryMode()
    {
      new SetSimpleRecoveryCommand(this.queryExecuter, this.settings).Execute();
    }

    public void SetSqlServer2005CompatibilityMode()
    {
      new SetSqlServer2005CompatibilityModeCommand(this.queryExecuter, this.settings).Execute();
    }

    public void CreateLogin(string loginName, string loginPassword)
    {
      if (string.IsNullOrEmpty(loginName))
        return;
      new CreateLoginCommand(this.queryExecuter, this.settings, loginName, loginPassword).Execute();
    }

    public void CreateWindowsLogin(string loginName)
    {
      if (string.IsNullOrEmpty(loginName))
        return;
      new CreateWindowsLoginCommand(this.queryExecuter, this.settings, loginName).Execute();
    }

    public void CreateUser(string name, bool readOnly)
    {
      if (string.IsNullOrEmpty(name))
        return;
      new CreateUserCommand(this.queryExecuter, this.settings, name, readOnly).Execute();
    }

    public void DropLogin(string loginName)
    {
      if (string.IsNullOrEmpty(loginName))
        return;
      new DropLoginCommand(this.queryExecuter, this.settings, loginName).Execute();
    }

    public void DropUser(string userName)
    {
      if (string.IsNullOrEmpty(userName))
        return;
      new DropUserCommand(this.queryExecuter, this.settings, userName).Execute();
    }

    public void ExecuteAdHocCommand(string commandString)
    {
      this.ExecuteAdHocCommand(commandString, false);
    }

    public void ExecuteAdHocCommand(string commandString, bool useMaster)
    {
      new AdHocSqlCommand(this.queryExecuter, this.settings, commandString, useMaster).Execute();
    }
  }
}
