using System;
using System.Collections.Generic;
using Migrator.Framework;
using System.Management.Automation;

namespace Id.PowershellExtensions.PushDatabaseMigration
{
    public abstract class BaseLogger : ILogger
    {
        public void ApplyingDBChange(string sql)
        {
            this.Log(sql, new object[0]);
        }

        public void Exception(string message, Exception ex)
        {
            this.Log("============ Error Detail ============", new object[0]);
            this.Log("Error: {0}", new object[] { message });
            this.LogExceptionDetails(ex);
            this.Log("======================================", new object[0]);
        }

        public void Exception(long version, string migrationName, Exception ex)
        {
            this.Log("============ Error Detail ============", new object[0]);
            this.Log("Error in migration: {0}", new object[] { version });
            this.LogExceptionDetails(ex);
            this.Log("======================================", new object[0]);
        }

        public void Finished(List<long> currentVersion, long finalVersion)
        {
            foreach (var l in currentVersion)
            {
                this.Log("Migrated to version {0}", new object[] { l });
            }
            this.Log("Migrated to final version {0}", new object[] { finalVersion });
        }

        public abstract void Log(string format, params object[] args);        

        public void MigrateDown(long version, string migrationName)
        {
            this.Log("Removing {0}: {1}", new object[] { version.ToString(), migrationName });
        }

        public void MigrateUp(long version, string migrationName)
        {
            this.Log("Applying {0}: {1}", new object[] { version.ToString(), migrationName });
        }

        public void RollingBack(long originalVersion)
        {
            this.Log("Rolling back to migration {0}", new object[] { originalVersion });
        }

        public void Skipping(long version)
        {
            this.MigrateUp(version, "<Migration not found>");
        }

        public void Started(List<long> currentVersions, long finalVersion)
        {
            this.Log("Latest version applied : {0}.  Target version : {1}", new object[] { this.LatestVersion(currentVersions), finalVersion });
        }

        public void Trace(string format, params object[] args)
        {
            this.Log("[Trace] {0}", new object[] { string.Format(format, args) });
        }

        public void Warn(string format, params object[] args)
        {
            this.Log("[Warning] {0}", new object[] { string.Format(format, args) });
        }

        private void LogExceptionDetails(Exception ex)
        {
            this.Log("{0}", new object[] { ex.Message });
            this.Log("{0}", new object[] { ex.StackTrace });
            for (Exception iex = ex.InnerException; iex != null; iex = iex.InnerException)
            {
                this.Log("Caused by: {0}", new object[] { iex });
                this.Log("{0}", new object[] { ex.StackTrace });
            }
        }

        private string LatestVersion(List<long> versions)
        {
            if (versions.Count > 0)
            {
                long latest = versions[versions.Count - 1];
                return latest.ToString();
            }
            return "No migrations applied yet!";
        }
    }
}
