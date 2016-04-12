using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;

namespace Id.DatabaseMigration
{
    public abstract class SetupMigration : Migration
    {
        public ISqlServerSettings Settings { get; private set; }

        public IServerAdministrator Server { get; private set; }

        protected SetupMigration()
            : this(AmbientSettings.Settings)
        { }

        protected SetupMigration(ISqlServerSettings settings)
            : this(new SqlServerAdministrator(settings ?? AmbientSettings.Settings))
        {
            Settings = settings ?? AmbientSettings.Settings;
        }

        protected SetupMigration(IServerAdministrator serverAdministrator)
        {
            Server = serverAdministrator;
        }
    }
}
