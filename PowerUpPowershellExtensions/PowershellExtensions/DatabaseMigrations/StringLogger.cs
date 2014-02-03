using Migrator.Framework;
using System.Text;

namespace Id.PowershellExtensions.DatabaseMigrations
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
