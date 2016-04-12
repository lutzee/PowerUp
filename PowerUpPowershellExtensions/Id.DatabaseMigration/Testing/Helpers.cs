using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace Id.DatabaseMigration.Testing
{
    public static class Helpers
    {
        private const string FKCHECK = "select \r\ncase (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c\r\njoin INFORMATION_SCHEMA.TABLE_CONSTRAINTS fk ON c.CONSTRAINT_NAME = fk.CONSTRAINT_NAME\r\nwhere c.Constraint_Name = '{0}'\r\nand fk.Constraint_Type = 'FOREIGN KEY')\r\n\twhen 1 then 1\r\n\telse 0\r\nend";

        public static bool GetConnectionTestResult(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlConnection.ClearPool(connection);
                using (SqlCommand sqlCommand = new SqlCommand("select 'passed' as passed", connection))
                {
                    try
                    {
                        connection.Open();
                        return (string)sqlCommand.ExecuteScalar() == "passed";
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public static bool CheckForeignKeyExists(ISqlServerSettings settings, string keyName)
        {
            return Helpers.GetScalar<int>(string.Format("select \r\ncase (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c\r\njoin INFORMATION_SCHEMA.TABLE_CONSTRAINTS fk ON c.CONSTRAINT_NAME = fk.CONSTRAINT_NAME\r\nwhere c.Constraint_Name = '{0}'\r\nand fk.Constraint_Type = 'FOREIGN KEY')\r\n\twhen 1 then 1\r\n\telse 0\r\nend", (object)keyName), settings) == 1;
        }

        public static void ExecuteNonQuery(string sql, ISqlServerSettings settings)
        {
            using (var connection = new SqlConnection(settings.DefaultConnectionString))
            {
                SqlConnection.ClearPool(connection);
                using (var sqlCommand = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static T GetScalar<T>(string sql, ISqlServerSettings settings)
        {
            using (SqlConnection connection = new SqlConnection(settings.DefaultConnectionString))
            {
                SqlConnection.ClearPool(connection);
                using (SqlCommand sqlCommand = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    return (T)sqlCommand.ExecuteScalar();
                }
            }
        }

        public static IEnumerable<Column> GetColumns(ITransformationProvider assertionDatabase, string tableName)
        {
            return (IEnumerable<Column>)Enumerable.ToList<Column>((IEnumerable<Column>)assertionDatabase.GetColumns(tableName));
        }
    }
}
