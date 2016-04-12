using System.Diagnostics;
using NUnit.Framework;

namespace Id.DatabaseMigration.Testing
{
    [TestFixture]
    public abstract class LatestDbSchemaVersionFixture : DatabaseSchemaMigrationsFixtureBase
    {
        protected override long Version
        {
            get { return -1; }
        }

        [Test]
        public virtual void WhenMigrationUpped_ThenSucceeds()
        {
            Trace.WriteLine("Upped to latest version OK");
        }
    }
}