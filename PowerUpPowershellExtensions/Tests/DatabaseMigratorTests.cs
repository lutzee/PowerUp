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
    public class DatabaseMigratorTests
    {
        private Assembly MigrationsAssembly;
        private StringBuilder Log;
        private StringLogger Logger;
        private DatabaseMigrator DatabaseMigrator;

        [SetUp]
        public void SetUp()
        {
            string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseMigratorTests)).CodeBase);
            MigrationsAssembly = Assembly.LoadFrom(new Uri(Path.Combine(folder,
                                         "ExampleMigrationAssemblies\\Id.VisaDebitMicrositeAU.DatabaseMigrations.dll")).LocalPath);
            Log = new StringBuilder();
            Logger = new StringLogger(Log);
            DatabaseMigrator = new DatabaseMigrator(Logger, true, "SqlServer", -1, true) {TestMode = true};
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseMigrator.CleanUp();
            MigrationsAssembly = null;
            Logger = null;
            DatabaseMigrator = null;
        }

        [Test]
        public void DatabaseMigrator_Execute_DryRunLogsActions()
        {
            DatabaseMigrator.Execute(MigrationsAssembly);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
        
        [Test]        
        public void DatabaseMigrator_Execute_ExecutesActions()
        {
            DatabaseMigrator.DryRun = false;
            DatabaseMigrator.Execute(MigrationsAssembly);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
    }
}
