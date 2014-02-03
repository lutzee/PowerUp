using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tests.Helpers
{
    internal class ResourceHelpers
    {
        public static Stream GetStreamFromResource(string name)
        {
            return typeof(ResourceHelpers).Assembly.GetManifestResourceStream(name);
        }

        public static byte[] GetFileBytesFromResource(Type typeInAssembly, string name)
        {
            var memoryStream = new MemoryStream();
            GetStreamFromResource(name).CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
