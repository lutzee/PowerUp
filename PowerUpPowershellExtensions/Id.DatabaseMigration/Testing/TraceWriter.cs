using System.Diagnostics;
using Migrator.Framework.Loggers;

namespace Id.DatabaseMigration.Testing
{
  public class TraceWriter : ILogWriter
  {
    public void Write(string message, params object[] args)
    {
      Trace.Write(string.Format("TRACE: {0}", (object) string.Format(message, args)));
    }

    public void WriteLine(string message, params object[] args)
    {
      Trace.WriteLine(string.Format("TRACE: {0}", (object) string.Format(message, args)));
    }
  }
}
