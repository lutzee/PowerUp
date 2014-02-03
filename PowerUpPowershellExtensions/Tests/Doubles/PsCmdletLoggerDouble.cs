using Id.PowershellExtensions;

namespace Tests
{
    public class PsCmdletLoggerDouble : IPsCmdletLogger
    {
        public int ExceptionsLogged { get; private set; }

        public void Log(string format, params object[] args)
        {
        }

        public void Log(string message)
        {
        }

        public void Log(System.Exception ex)
        {
            ExceptionsLogged++;
        }
    }
}