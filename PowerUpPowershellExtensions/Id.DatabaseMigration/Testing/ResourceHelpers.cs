using System;
using System.IO;

namespace Id.DatabaseMigration.Testing
{
  internal class ResourceHelpers
  {
    public static Stream GetStreamFromResource(Type typeInResourceAssembly, string name)
    {
      return typeInResourceAssembly.Assembly.GetManifestResourceStream(name);
    }

    public static string GetStringFromResource(Type typeInResourceAssembly, string name)
    {
      using (StreamReader streamReader = new StreamReader(ResourceHelpers.GetStreamFromResource(typeInResourceAssembly, name)))
        return streamReader.ReadToEnd();
    }
  }
}
