using Id.DatabaseMigration.SqlServer;
using Id.DatabaseMigration.Testing;
using Migrator.Framework;
using System;
using System.Data.SqlClient;

namespace Id.DatabaseMigration
{
    public abstract class ScriptedMigration : Migration
    {
        public ISqlServerSettings Settings { get; private set; }

        public IServerAdministrator Server { get; private set; }

        protected ScriptedMigration()
            : this(AmbientSettings.Settings)
        { }

        protected ScriptedMigration(ISqlServerSettings settings)
            : this(new SqlServerAdministrator(settings ?? AmbientSettings.Settings))
        {
            this.Settings = settings ?? AmbientSettings.Settings;
        }

        protected ScriptedMigration(IServerAdministrator serverAdministrator)
        {
            this.Server = serverAdministrator;
        }

        protected void ExecuteSqlInTransaction(string sql, bool useMaster = false)
        {
            this.Database.BeginTransaction();
            try
            {
                this.Server.ExecuteAdHocCommand(sql, useMaster);
                this.Database.Commit();
            }
            catch (SqlException ex)
            {
                this.Database.Rollback();
                throw new MigrationException("An exception occured during the migration", (Exception)ex);
            }
        }

        protected void ExecuteSqlWithoutTransaction(string sql, bool useMaster = false)
        {
            try
            {
                this.Server.ExecuteAdHocCommand(sql, useMaster);
            }
            catch (SqlException ex)
            {
                throw new MigrationException("An exception occured during the migration. Any changes will need to be manually rolled-back", (Exception)ex);
            }
        }

        protected void ExecuteSqlFromResourceInTransaction(Type typeInResourceAssembly, string resourceName, bool useMaster = false)
        {
            this.ExecuteSqlInTransaction(ResourceHelpers.GetStringFromResource(typeInResourceAssembly, resourceName), useMaster);
        }

        protected void ExecuteSqlFromResourceWithoutTransaction(Type typeInResourceAssembly, string resourceName, bool useMaster = false)
        {
            this.ExecuteSqlWithoutTransaction(ResourceHelpers.GetStringFromResource(typeInResourceAssembly, resourceName), useMaster);
        }
    }
}
