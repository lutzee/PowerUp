using System;
using System.IO;
using System.Text;

namespace SampleDbMigrations
{
  internal class ResourceHelpers
  {
    public static Stream GetStreamFromResource(string name)
    {
      return typeof (ResourceHelpers).Assembly.GetManifestResourceStream(name);
    }

    public static string GetStringFromResource(string name)
    {
      Stream manifestResourceStream = typeof (ResourceHelpers).Assembly.GetManifestResourceStream(name);
      if (manifestResourceStream == null)
        throw new ArgumentNullException(name);
      if (manifestResourceStream.Length <= 0L)
        return string.Empty;
      byte[] numArray = new byte[manifestResourceStream.Length];
      manifestResourceStream.Position = 0L;
      manifestResourceStream.Read(numArray, 0, (int) manifestResourceStream.Length);
      return Encoding.UTF8.GetString(numArray);
    }
  }
}
