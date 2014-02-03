using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Reflection;
using System.IO;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration;

namespace Id.PowershellExtensions.PushDatabaseMigration
{
    public class DatabaseMigrator
    {
        public ILogger Logger { get; set; }
        public bool DryRun { get; set; }
        public string Provider { get; set; }
        public long To { get; set; }
        public bool TestMode { get; set; }
        public bool Trace { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public DatabaseMigrator(ILogger logger, bool dryRun, string provider, long to, bool trace)
            : this(logger, dryRun, provider, to, trace, false)
        { }

        public DatabaseMigrator(ILogger logger, bool dryRun, string provider, long to, bool trace, bool testMode)
        {
            Logger = logger;
            DryRun = dryRun;
            Provider = provider;
            To = to;
            Trace = trace;
            TestMode = testMode;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettingsParser(asm);
            SqlServerAdministrator = new SqlServerAdministrator(Settings);

            EnsureDatabaseExists();

            Logger.Log("Running migrations with connection string {0}", new object[] { Settings.DefaultConnectionString });
            var migrator = new Migrator.Migrator(this.Provider, Settings.DefaultConnectionString, asm,
                                                 Trace, Logger) { DryRun = this.DryRun };

            MemoryStream logOutputStream = new MemoryStream();

            using (MemoryStream logStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(logStream))
            {
                migrator.Logger = new SqlScriptFileLogger(migrator.Logger, writer);
                this.RunMigration(migrator);

                logOutputStream = new MemoryStream(logStream.ToArray());
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
            if (!TestMode)
                return;

            AmbientSettings.Settings = null;

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
            if (TestMode)
            {
                ((XmlSettingsParser)Settings).DatabaseName = string.Format("{0}_{1}", Settings.DatabaseName,
                                                                            Guid.NewGuid().ToString("N").Substring(0, 5));
                AmbientSettings.Settings = Settings;
            }

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
