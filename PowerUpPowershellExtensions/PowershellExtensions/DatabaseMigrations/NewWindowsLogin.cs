using System.Collections.Generic;
using System.Reflection;
using Migrator.Framework;
using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration;

namespace Id.PowershellExtensions.DatabaseMigrations
{
    public class NewWindowsLogin
    {
        public ILogger Logger { get; set; }
        public string UserName { get; set; }

        private ISqlServerSettings Settings;
        private SqlServerAdministrator SqlServerAdministrator;

        public NewWindowsLogin(ILogger logger, string userName)
        {
            Logger = logger;
            UserName = userName;
        }

        public void Execute(Assembly asm)
        {
            Settings = new XmlSettings(asm);

            ExecuteCore();
        }

        public void Execute(IDictionary<string, string> settings)
        {
            Settings = settings != null
                ? new DictionarySettings(settings)
                : AmbientSettings.Settings;

            ExecuteCore();
        }

        public void Execute(ISqlServerSettings serverSettings)
        {
            Settings = serverSettings ?? AmbientSettings.Settings;

            ExecuteCore();
        }

        public void ExecuteCore()
        {
            SqlServerAdministrator = new SqlServerAdministrator(AmbientSettings.Settings ?? Settings);

            if (!string.IsNullOrEmpty(UserName))
            {
                Logger.Log("Creating windows login '{0}' in database {1}", new object[] { UserName, Settings.DatabaseName });
                SqlServerAdministrator.CreateWindowsLogin(UserName);
                SqlServerAdministrator.CreateUser(UserName, false);
            }
        }
    }
}
