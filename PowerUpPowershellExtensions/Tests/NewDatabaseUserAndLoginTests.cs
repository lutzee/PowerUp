using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Id.DatabaseMigration;
using Id.DatabaseMigration.Logging;
using Id.PowershellExtensions.DatabaseMigrations;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Tests
{
    [TestFixture]
    [Category("DB")]
    public class NewDatabaseUserAndLoginTests
    {
        private Assembly MigrationsAssembly;
        private StringBuilder Log;
        private StringLogger Logger;
        private DatabaseMigrator DatabaseMigrator;
        private NewUserAndLogin NewUserAndLogin;
        private Dictionary<string, string> Settings;

        [SetUp]
        public void SetUp()
        {
            string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseMigratorTests)).CodeBase);
            MigrationsAssembly = Assembly.LoadFrom(new Uri(Path.Combine(folder, "SampleDbMigrations.dll")).LocalPath);
            Log = new StringBuilder();
            Logger = new StringLogger(Log);
            Settings = new Dictionary<string, string>()
            {
                { "Server", "." },
                { "Database", "TestDb" }
            };
            DatabaseMigrator = new DatabaseMigrator(Logger, false, "SqlServer", -1, true/*trace*/);
            DatabaseMigrator.Execute(Settings, MigrationsAssembly);
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseMigrator.CleanUp();
            MigrationsAssembly = null;
            Logger = null;
            DatabaseMigrator = null;
            NewUserAndLogin = null;
        }

        [Test]
        public void NewDatabaseUserAndLogin_Execute_CreatesUserAndLogin()
        {
            NewUserAndLogin = new NewUserAndLogin(Logger, @"Id-TestUser", "Password");
            NewUserAndLogin.Execute(Settings);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
    }
}
