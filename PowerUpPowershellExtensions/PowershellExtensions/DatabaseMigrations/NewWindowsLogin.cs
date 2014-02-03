using System;
using System.Text;
using System.Reflection;
using System.IO;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration;

namespace Id.PowershellExtensions.DatabaseMigrations
{
    public class NewWindowsLogin
    {
        public ILogger Logger { get; set; }
        public string Provider { get; set; }
        public string UserName { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public NewWindowsLogin(ILogger logger, string provider, string userName)
        {
            Logger = logger;
            Provider = provider;
            UserName = userName;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettingsParser(asm);
            SqlServerAdministrator = new SqlServerAdministrator(AmbientSettings.Settings  ??  Settings);

            if (!string.IsNullOrEmpty(UserName))
            {
                Logger.Log("Creating windows login '{0}' in database {1}", new object[] { UserName, Settings.DatabaseName });
                SqlServerAdministrator.CreateWindowsLogin(UserName);
                SqlServerAdministrator.CreateUser(UserName, false);
            }
        }
    }
}
