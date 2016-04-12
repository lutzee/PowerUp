using Id.DatabaseMigration.Logging;
using Migrator.Framework;
using System.Management.Automation;

namespace Id.PowershellExtensions.DatabaseMigrations
{
    public class TaskLogger : BaseLogger, ILogger
    {
        private PSCmdlet Parent;

        public TaskLogger(PSCmdlet parent)
        {
            Parent = parent;
        }

        public override void Log(string format, params object[] args)
        {
            Parent.WriteVerbose(string.Format(format, args));
        }        
    }
}
