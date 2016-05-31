using NUnit.Framework;

namespace Id.DatabaseMigration.Testing
{
    [TestFixture]
    public abstract class DatabaseSchemaMigrationsFixtureBase : MigrationFixtureBase
    {
        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            CleanUp();
        }

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
            CleanUp();
        }

        protected void CleanUp()
        {
            var server = GetServer();

            server.DropDatabase();
            server.DropDefaultLogin();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}