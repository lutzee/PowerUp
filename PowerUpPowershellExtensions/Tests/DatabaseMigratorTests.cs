using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Id.DatabaseMigration;
using Id.DatabaseMigration.Logging;
using NUnit.Framework;
using System.IO;
using System.Reflection;

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
        private Dictionary<string, string> Settings;

        [SetUp]
        public void SetUp()
        {
            string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseMigratorTests)).CodeBase);
            MigrationsAssembly = Assembly.LoadFrom(new Uri(Path.Combine(folder, "SampleDbMigrations.dll")).LocalPath);
            Settings = new Dictionary<string, string>
            {
                { "Server", "." },
                { "Database", "TestDb" }
            };
            Log = new StringBuilder();
            Logger = new StringLogger(Log);
            DatabaseMigrator = new DatabaseMigrator(Logger, true, "SqlServer", -1, true);
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
            DatabaseMigrator.Execute(Settings, MigrationsAssembly);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
        
        [Test]        
        public void DatabaseMigrator_Execute_ExecutesActions()
        {
            DatabaseMigrator.DryRun = false;
            DatabaseMigrator.Execute(Settings, MigrationsAssembly);

            Assert.That(Log.ToString(), Is.Not.Empty);
            Trace.WriteLine(Log.ToString());
            Console.WriteLine(Log.ToString());
        }
    }
}
