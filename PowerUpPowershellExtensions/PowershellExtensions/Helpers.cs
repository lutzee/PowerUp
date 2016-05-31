using System;
using System.IO;
using System.Text;
using href.Utils;

namespace Id.PowershellExtensions
{
    internal class EncodedFile
    {
        public string Contents { get; set; }
        public Encoding Encoding { get; set; }
        public DirectoryInfo Directory { get; set; }
        public FileInfo File { get; set; }
    }

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

        internal static EncodedFile GetFileWithEncoding(string file)
        {
            return GetFileWithEncodingCore(file);
        }

        internal static EncodedFile GetFileWithEncodingNoBom(string file)
        {
            return GetFileWithEncodingCore(file, false);
        }

        private static EncodedFile GetFileWithEncodingCore(string file, bool includeBom = true)
        {
            using (Stream fs = File.Open(file, FileMode.Open))
            {
                var rawData = new byte[fs.Length];
                fs.Read(rawData, 0, (int)fs.Length);
                var encoding = EncodingTools.DetectInputCodepage(rawData);
                var preambleLength = encoding.GetPreamble().Length;
                var contents = !includeBom && preambleLength > 0
                    ? encoding.GetString(rawData, preambleLength, rawData.Length - preambleLength)
                    : encoding.GetString(rawData);

                return new EncodedFile
                {
                    Contents = contents,
                    Encoding = encoding,
                    Directory = new DirectoryInfo(new DirectoryInfo(file).Parent.FullName),
                    File = new FileInfo(file)
                };
            }
        }
    }
}
