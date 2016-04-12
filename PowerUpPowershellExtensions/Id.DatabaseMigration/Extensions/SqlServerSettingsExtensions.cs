using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration.Testing;

namespace Id.DatabaseMigration.Extensions
{
    public static class SqlServerSettingsExtensions
    {
        public static T GetScalar<T>(this ISqlServerSettings settings, string sql)
        {
            return Helpers.GetScalar<T>(sql, settings);
        }

        public static void ExecuteNonQuery(this ISqlServerSettings settings, string sql)
        {
            Helpers.ExecuteNonQuery(sql, settings);
        }
    }
}