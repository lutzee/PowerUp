using System;
using System.Data;
using System.Data.SqlClient;

namespace Id.DatabaseMigration.SqlServer
{
  public class SqlServerQueryExecuter : IQueryExecuter
  {
    public void ExecuteNonQuery(string connectionString, string commandString)
    {
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlConnection.ClearPool(connection);
        connection.Open();
        string[] strArray = commandString.Split(new string[4]
        {
          "\r\nGO",
          "GO\r\n",
          "GO ",
          "GO\t"
        }, StringSplitOptions.RemoveEmptyEntries);
        int num = 1;
        try
        {
          foreach (string cmdText in strArray)
          {
            using (SqlCommand sqlCommand = new SqlCommand(cmdText, connection))
            {
              sqlCommand.CommandTimeout = 360;
              sqlCommand.CommandType = CommandType.Text;
              sqlCommand.ExecuteNonQuery();
            }
            ++num;
          }
        }
        catch (Exception ex)
        {
          string str = strArray[num - 1];
          throw new Exception(string.Format("An exception occured while execuing command number {0}, starting: {1}", (object) num, (object) str.Substring(0, Math.Min(str.Length, 2000))), ex);
        }
      }
    }

    public T ExecuteScalar<T>(string connectionString, string commandString)
    {
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        SqlConnection.ClearPool(connection);
        connection.Open();
        using (SqlCommand sqlCommand = new SqlCommand(commandString, connection))
          return (T) Convert.ChangeType(sqlCommand.ExecuteScalar(), typeof (T));
      }
    }
  }
}
