using Id.DatabaseMigration.SqlServer;

namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class DisableFullTextServiceCommand : CommandBase
  {
    public DisableFullTextServiceCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings)
      : base(queryExecuter, settings)
    {
    }

    public override void Execute()
    {
      this.QueryExecuter.ExecuteScalar<object>(this.Settings.DefaultConnectionString, "\r\n                /****** This property adds overhead – enable only if necessary ******/\r\n                IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))\r\n                EXEC sp_fulltext_database @action = 'disable'");
    }
  }
}
