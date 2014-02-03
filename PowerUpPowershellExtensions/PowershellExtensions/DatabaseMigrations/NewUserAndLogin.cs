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
    public class NewUserAndLogin
    {
        public ILogger Logger { get; set; }
        public string Provider { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public NewUserAndLogin(ILogger logger, string provider, string userName, string password)
        {
            Logger = logger;
            Provider = provider;
            UserName = userName;
            Password = password;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettingsParser(asm);
            SqlServerAdministrator = new SqlServerAdministrator(AmbientSettings.Settings ?? Settings);

            if (!string.IsNullOrEmpty(UserName))
            {
                Logger.Log("Creating login and user '{0}' in database {1}", new object[] { UserName, Settings.DatabaseName });
                SqlServerAdministrator.CreateLogin(UserName, Password);
                SqlServerAdministrator.CreateUser(UserName, false);
            }
        }
    }
}
