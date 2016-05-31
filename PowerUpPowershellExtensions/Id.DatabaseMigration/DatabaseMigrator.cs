using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Id.DatabaseMigration.SqlServer;
using Migrator.Framework;
using Migrator.Framework.Loggers;

namespace Id.DatabaseMigration
{
    public class DatabaseMigrator
    {
        public ILogger Logger { get; set; }
        public bool DryRun { get; set; }
        public string Provider { get; set; }
        public long To { get; set; }
        public bool Trace { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public DatabaseMigrator(ILogger logger, bool dryRun, string provider, long to, bool trace)
        {
            Logger = logger;
            DryRun = dryRun;
            Provider = provider;
            To = to;
            Trace = trace;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettings(asm);
            ExecuteCore(asm);
        }

        public void Execute(ISqlServerSettings settings, Assembly asm)
        {
            Settings = settings;
            ExecuteCore(asm);
        }

        public void Execute(IDictionary<string, string> settings, Assembly asm)
        {
            Settings = new DictionarySettings(settings);
            ExecuteCore(asm);
        }

        protected void ExecuteCore(Assembly asm)
        {
            SqlServerAdministrator = new SqlServerAdministrator(Settings);
            AmbientSettings.Settings = Settings;

            EnsureDatabaseExists();

            Logger.Log("Running migrations with connection string {0}", new object[] { Settings.DefaultConnectionString });
            var migrator = new Migrator.Migrator(this.Provider, Settings.DefaultConnectionString, asm,
                                                 Trace, Logger) { DryRun = this.DryRun };

            using (var logStream = new MemoryStream())
            using (var writer = new StreamWriter(logStream))
            {
                migrator.Logger = new SqlScriptFileLogger(migrator.Logger, writer);
                this.RunMigration(migrator);

                var logOutputStream = new MemoryStream(logStream.ToArray());
                Logger.Log(GetStringFromStream(logOutputStream));
            }
        }

        public void RunMigration(Migrator.Migrator mig)
        {
            if (mig.DryRun)
            {
                mig.Logger.Log("********** Dry run! Not actually applying changes. **********", new object[0]);
            }
            if (this.To == -1L)
            {
                mig.MigrateToLastVersion();
            }
            else
            {
                mig.MigrateTo(this.To);
            }
        }

        public void CleanUp()
        {
            if (!string.IsNullOrEmpty(Settings.DefaultUserName))
            {
                Logger.Log("Dropping login and user '{0}'", new object[] { Settings.DefaultUserName });
                SqlServerAdministrator.DropDefaultUser();
                SqlServerAdministrator.DropDefaultLogin();
            }

            Logger.Log("Dropping database '{0}'", new object[] { Settings.DatabaseName });
            SqlServerAdministrator.DropDatabase();
        }

        private void EnsureDatabaseExists()
        {
            Logger.Log("Ensuring database '{0}'", new object[] { Settings.DatabaseName });
            SqlServerAdministrator.CreateDatabase();

            if (!string.IsNullOrEmpty(Settings.DefaultUserName))
            {
                Logger.Log("Ensuring login and user '{0}'", new object[] { Settings.DefaultUserName });
                SqlServerAdministrator.CreateDefaultLogin();
                SqlServerAdministrator.CreateDefaultUser();
            }
        }

        private static string GetStringFromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (stream.Length > 0)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(buffer, 0, (int)stream.Length);

                return Encoding.UTF8.GetString(buffer);
            }

            return string.Empty;
        }
    }
}
