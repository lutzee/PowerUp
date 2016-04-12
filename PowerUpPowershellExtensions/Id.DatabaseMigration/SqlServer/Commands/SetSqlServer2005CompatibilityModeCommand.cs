using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class SetSqlServer2005CompatibilityModeCommand : CommandBase
  {
    public SetSqlServer2005CompatibilityModeCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings)
      : base(queryExecuter, settings)
    {
    }

    public override void Execute()
    {
      this.QueryExecuter.ExecuteScalar<object>(this.Settings.DefaultConnectionString, string.Format("\r\n                    EXEC dbo.sp_dbcmptlevel @dbname=N'{0}', @new_cmptlevel=90", (object) this.Settings.DatabaseName));
    }
  }
}
