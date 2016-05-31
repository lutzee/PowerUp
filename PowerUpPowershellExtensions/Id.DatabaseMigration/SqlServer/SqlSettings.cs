namespace Id.DatabaseMigration.SqlServer
{
    public abstract class SqlSettings : ISqlServerSettings
    {
        private const string IntegratedSecurityConnectionString = "Server={0};Integrated security=SSPI;database={1}";
        private const string SqlServerSecurityConnectionString = "Server={0};database={1};user id={2};password={3}";

        public string DatabaseName { get; set; }

        public string DefaultConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(this.MigrationUserName))
                    return string.Format(IntegratedSecurityConnectionString, (object)this.Server, (object)this.DatabaseName);
                return string.Format(SqlServerSecurityConnectionString, (object)this.Server, (object)this.DatabaseName, (object)this.MigrationUserName, (object)this.MigrationUserPassword);
            }
        }

        public string DefaultUserName { get; set; }

        public string DefaultUserPassword { get; set; }

        public string MigrationUserName { get; set; }

        public string MigrationUserPassword { get; set; }

        public string MasterConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(this.MigrationUserName))
                    return string.Format(IntegratedSecurityConnectionString, (object)this.Server, (object)this.MasterDatabaseName);
                return string.Format(SqlServerSecurityConnectionString, (object)this.Server, (object)this.MasterDatabaseName, (object)this.MigrationUserName, (object)this.MigrationUserPassword);
            }
        }

        public string Server { get; set; }

        public virtual string MasterDatabaseName
        {
            get
            {
                return "master";
            }
        }
    }
}