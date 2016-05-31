namespace Id.DatabaseMigration
{
  public interface IServerAdministrator
  {
    void CreateDatabase();

    void CreateDefaultLogin();

    void CreateDefaultUser();

    void DropDefaultUser();

    void DropDefaultLogin();

    void DropDatabase();

    void DisableFullTextServices();

    void SetSimpleRecoveryMode();

    void SetSqlServer2005CompatibilityMode();

    void CreateLogin(string name, string password);

    void CreateWindowsLogin(string name);

    void CreateUser(string name, bool readOnly);

    void DropLogin(string name);

    void DropUser(string name);

    void ExecuteAdHocCommand(string commandString, bool useMaster);

    void ExecuteAdHocCommand(string commandString);
  }
}
