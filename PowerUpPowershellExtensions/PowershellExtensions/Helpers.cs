using System;
using System.Text;

namespace Id.PowershellExtensions
{
    public class Helpers
    {
        public static string GetFullExceptionMessage(Exception ex)
        {
            var fullMessage = new StringBuilder();

            fullMessage.AppendLine(ex.Message);
            fullMessage.AppendLine("\n");

            int i = 0;
            while (ex.InnerException != null)
            {
                string indent = string.Join("\t", new string[i + 1]);
                fullMessage.Append(indent);
                fullMessage.AppendLine("Inner Exception:\n");
                fullMessage.Append(indent);
                fullMessage.AppendLine(ex.InnerException.Message);
                fullMessage.AppendLine("\n");
                ex = ex.InnerException;
                i++;
            }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                fullMessage.AppendLine("Stack Trace:\n");
                fullMessage.AppendLine(ex.StackTrace);
            }

            return fullMessage.ToString();
        }
    }
}
