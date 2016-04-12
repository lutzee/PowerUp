using System.Reflection;
using System.Text;
using Id.DatabaseMigration.Logging;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using Migrator.Providers;
using Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace Id.DatabaseMigration.Testing
{
    public abstract class MigrationFixtureBase
    {
        protected abstract long Version { get; }

        protected abstract ISqlServerSettings Settings { get; }

        protected abstract Assembly MigrationsAssembly { get; }

        protected virtual Dialect Dialect
        {
            get
            {
                return (Dialect)new SqlServer2005Dialect();
            }
        }

        protected virtual ITransformationProvider GetDatabase()
        {
            return new SqlServerTransformationProvider(this.Dialect, this.Settings.DefaultConnectionString)
            {
                Logger = new Logger(false, new TraceWriter())
            };
        }

        protected virtual IServerAdministrator GetServer()
        {
            return new SqlServerAdministrator(this.Settings);
        }

        [SetUp]
        public virtual void Setup()
        {
            this.MigrateUp();
        }

        [TearDown]
        public virtual void TearDown()
        {
            this.MigrateDown();
        }

        protected void MigrateUp()
        {
            MigrateToVersion(this.Version, this.MigrationsAssembly);
        }

        protected void MigrateDown()
        {
            MigrateToVersion(0, this.MigrationsAssembly);
        }

        protected void MigrateTo(long version)
        {
            MigrateToVersion(version, this.MigrationsAssembly);
        }

        private void MigrateToVersion(long version, Assembly assembly)
        {
            var log = new StringBuilder();
            var logger = new StringLogger(log);
            var db = new DatabaseMigrator(logger, false, "SqlServer", version, true);

            db.Execute(Settings, assembly);
        }
    }
}
