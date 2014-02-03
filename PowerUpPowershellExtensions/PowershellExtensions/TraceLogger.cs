using System.Diagnostics;
using System.Management.Automation;

namespace Id.PowershellExtensions
{
    public class TraceLogger : IPsCmdletLogger
    {        
        public void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public void Log(string message)
        {
            Trace.WriteLine(message);
        }

        public void Log(System.Exception ex)
        {
            Trace.WriteLine(Helpers.GetFullExceptionMessage(ex));
        }
    }
}
