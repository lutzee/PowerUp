using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public abstract class CommandBase
  {
    private const string CheckDatabaseExistsString = "SELECT Count(*) FROM sys.databases WHERE [name]='{0}'";
    private readonly IQueryExecuter _queryExecuter;
    private readonly ISqlServerSettings _settings;

    public ISqlServerSettings Settings
    {
      get
      {
        return this._settings;
      }
    }

    public IQueryExecuter QueryExecuter
    {
      get
      {
        return this._queryExecuter;
      }
    }

    public bool DatabaseExists
    {
      get
      {
        return this._queryExecuter.ExecuteScalar<int>(this._settings.MasterConnectionString, string.Format("SELECT Count(*) FROM sys.databases WHERE [name]='{0}'", (object) this._settings.DatabaseName)) == 1;
      }
    }

    protected CommandBase(IQueryExecuter queryExecuter, ISqlServerSettings settings)
    {
      this._queryExecuter = queryExecuter;
      this._settings = settings;
    }

    public abstract void Execute();
  }
}
