using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class SetSimpleRecoveryCommand : CommandBase
  {
    public SetSimpleRecoveryCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings)
      : base(queryExecuter, settings)
    {
    }

    public override void Execute()
    {
      this.QueryExecuter.ExecuteScalar<object>(this.Settings.DefaultConnectionString, string.Format("\r\n                /****** Recovery always set to SIMPLE only if Transaction log backups are required ******/\r\n                ALTER DATABASE {0} SET RECOVERY SIMPLE", (object) this.Settings.DatabaseName));
    }
  }
}
