using System.Configuration;

namespace Id.DatabaseMigration.SqlServer
{
    public class SqlServerSettings : ISqlServerSettings
    {
        private const string ConnectionString = "Server={0};Integrated security=SSPI;database={1}";

        public string Server
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationServer"];
            }
        }

        public string DatabaseName
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationDatabase"];
            }
        }

        public string MasterDatabaseName
        {
            get
            {
                return "master";
            }
        }

        public string DefaultUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationDefaultUser"];
            }
        }

        public string DefaultUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationDefaultUserPassword"];
            }
        }

        public string MasterConnectionString
        {
            get
            {
                return string.Format(ConnectionString, (object)this.Server, (object)this.MasterDatabaseName);
            }
        }

        public string DefaultConnectionString
        {
            get
            {
                return string.Format(ConnectionString, (object)this.Server, (object)this.DatabaseName);
            }
        }

        public string DefaultIntegratedSecurityConnectionString
        {
            get
            {
                return this.DefaultConnectionString;
            }
        }

        public string MigrationUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationUserName"];
            }
        }

        public string MigrationUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["MigrationUserPassword"];
            }
        }
    }
}
