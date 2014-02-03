using System;
using System.Collections.Generic;
using Migrator.Framework;
using System.Management.Automation;
using System.Text;

namespace Id.PowershellExtensions.PushDatabaseMigration
{
    public class StringLogger : BaseLogger, ILogger
    {
        private StringBuilder LogString;

        public StringLogger(StringBuilder logString)
        {
            LogString = logString;
        }

        public override void Log(string format, params object[] args)
        {
            LogString.AppendFormat(format, args).AppendLine();
        }        
    }
}
