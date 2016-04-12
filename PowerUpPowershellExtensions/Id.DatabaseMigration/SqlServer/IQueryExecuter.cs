namespace Id.DatabaseMigration.SqlServer
{
  public interface IQueryExecuter
  {
    void ExecuteNonQuery(string connectionString, string commandString);

    T ExecuteScalar<T>(string connectionString, string commandString);
  }
}
