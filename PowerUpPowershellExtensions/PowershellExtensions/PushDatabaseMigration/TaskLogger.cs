using System;
using System.Collections.Generic;
using Migrator.Framework;
using System.Management.Automation;

namespace Id.PowershellExtensions.PushDatabaseMigration
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
