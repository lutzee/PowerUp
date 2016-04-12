using Id.DatabaseMigration.SqlServer;
using System.Data.SqlTypes;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class CreateDatabaseCommand : CommandBase
  {
    private const string CreateSqlString = "CREATE DATABASE [{0}]";

    public CreateDatabaseCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings)
      : base(queryExecuter, settings)
    {
    }

    public override void Execute()
    {
      if (this.DatabaseExists)
        return;
      this.QueryExecuter.ExecuteNonQuery(this.Settings.MasterConnectionString, string.Format("CREATE DATABASE [{0}]", (object) this.Settings.DatabaseName));
      if (!this.DatabaseExists)
        throw new SqlNullValueException(this.Settings.DatabaseName);
    }
  }
}
