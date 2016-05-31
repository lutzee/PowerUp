using System.Collections.Generic;

namespace Id.DatabaseMigration.SqlServer
{
    public class DictionarySettings : SqlSettings, ISqlServerSettings
    {
        public DictionarySettings(IDictionary<string, string> values)
        {
            Server = GetValueOrDefault(values, "Server");
            DatabaseName = GetValueOrDefault(values, "Database");
            DefaultUserName = GetValueOrDefault(values, "DefaultUser");
            DefaultUserPassword = GetValueOrDefault(values, "DefaultUserPassword");
            MigrationUserName = GetValueOrDefault(values, "MigrationUser");
            MigrationUserPassword = GetValueOrDefault(values, "MigrationUserPassword");
        }

        private static string GetValueOrDefault(IDictionary<string, string> values, string key, string @default = "")
        {
            return values.ContainsKey(key)
                ? values[key]
                : @default;
        }
    }
}