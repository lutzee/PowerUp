using System.Text;
using Migrator.Framework;

namespace Id.DatabaseMigration.Logging
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
