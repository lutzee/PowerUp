using System.Transactions;
using Id.DatabaseMigration.SqlServer;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Id.DatabaseMigration
{
    public abstract class SmoMigration : ScriptedMigration
    {
        protected SmoMigration()
            : this(AmbientSettings.Settings)
        { }

        protected SmoMigration(ISqlServerSettings settings) : base(settings) { }

        public override void Up()
        {
            var connection = this.CreateConnection();
            var server = new Server(connection);

            try
            {
                using (var scope = new TransactionScope())
                {
                    var database = server.Databases[this.Settings.DatabaseName];

                    UpCore(database);

                    scope.Complete();
                }
            }
            finally
            {
                server.ConnectionContext.ForceDisconnected();
            }
        }

        protected abstract void UpCore(Database database);

        private ServerConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(this.Settings.MigrationUserName))
                return new ServerConnection(this.Settings.Server) { NonPooledConnection = true };

            return new ServerConnection(
                this.Settings.Server,
                this.Settings.MigrationUserName,
                this.Settings.MigrationUserPassword
                ) { NonPooledConnection = true };
        }
    }
}