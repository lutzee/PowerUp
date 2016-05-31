namespace Id.DatabaseMigration.SqlServer.Commands
{
  public class AdHocSqlCommand : CommandBase
  {
    private string CommandString;
    private bool UseMaster;

    public AdHocSqlCommand(IQueryExecuter queryExecuter, ISqlServerSettings settings, string commandString, bool useMaster)
      : base(queryExecuter, settings)
    {
      this.CommandString = commandString;
      this.UseMaster = useMaster;
    }

    public override void Execute()
    {
      this.QueryExecuter.ExecuteNonQuery(this.UseMaster ? this.Settings.MasterConnectionString : this.Settings.DefaultConnectionString, this.CommandString);
    }
  }
}
