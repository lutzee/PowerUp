namespace Id.DatabaseMigration.SqlServer
{
  public interface ISqlServerSettings
  {
    string Server { get; }

    string DatabaseName { get; }

    string MasterDatabaseName { get; }

    string DefaultUserName { get; }

    string DefaultUserPassword { get; }

    string MigrationUserName { get; }

    string MigrationUserPassword { get; }

    string MasterConnectionString { get; }

    string DefaultConnectionString { get; }
  }
}
