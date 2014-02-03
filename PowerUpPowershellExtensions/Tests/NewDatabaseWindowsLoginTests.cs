using System;
using System.Diagnostics;
using System.Text;
using Id.PowershellExtensions.DatabaseMigrations;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using NUnit.Framework.SyntaxHelpers;

namespace Tests
{
    [TestFixture]
    [Category("DB")]
    public class NewDatabaseWindowsLoginTests
    {
        private Assembly MigrationsAssembly;
        private StringBuilder Log;
        private StringLogger Logger;
        private DatabaseMigrator DatabaseMigrator;
        private NewWindowsLogin NewWindowsLogin;

        [SetUp]
        public void SetUp()
        {            
            string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseMigratorTests)).CodeBase);
            MigrationsAssembly = Assembly.LoadFrom(new Uri(Path.Combine(folder,
                                         "ExampleMigrationAssemblies\\Id.VisaDebitMicrositeAU.DatabaseMigrations.dll")).LocalPath);
            Log = new StringBuilder();
            Logger = new StringLogger(Log);
            DatabaseMigrator = new DatabaseMigrator(Logger, false, "SqlServer", -1, true/*trace*/) { TestMode = true};            
            DatabaseMigrator.Execute(MigrationsAssembly);
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseMigrator.CleanUp();
            MigrationsAssembly = null;
            Logger = null;
            DatabaseMigrator = null;
            NewWindowsLogin = null;
        }

        [Test]
        public void NewDatabaseWindowsLogin_Execute_CreatesWindowsLogin()
        {
            NewWindowsLogin = new NewWindowsLogin(Logger, "SqlServer", @"cnw\cnwar5");
            NewWindowsLogin.Execute(MigrationsAssembly);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
    }
}
