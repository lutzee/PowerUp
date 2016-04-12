using System.Collections.Generic;
using System.Reflection;
using Migrator.Framework;
using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration;

namespace Id.PowershellExtensions.DatabaseMigrations
{
    public class NewUserAndLogin
    {
        public ILogger Logger { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public NewUserAndLogin(ILogger logger, string userName, string password)
        {
            Logger = logger;
            UserName = userName;
            Password = password;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettings(asm);

            ExecuteCore();
        }

        public void Execute(ISqlServerSettings serverSettings)
        {
            Settings = serverSettings ?? AmbientSettings.Settings;

            ExecuteCore();
        }

        public void Execute(IDictionary<string, string> settings)
        {
            Settings = settings != null
                ? new DictionarySettings(settings)
                : AmbientSettings.Settings;

            ExecuteCore();
        }

        protected void ExecuteCore()
        {
            SqlServerAdministrator = new SqlServerAdministrator(Settings);

            if (!string.IsNullOrEmpty(UserName))
            {
                Logger.Log("Creating login and user '{0}' in database {1}", new object[] { UserName, Settings.DatabaseName });
                SqlServerAdministrator.CreateLogin(UserName, Password);
                SqlServerAdministrator.CreateUser(UserName, false);
            }
        }
    }
}
