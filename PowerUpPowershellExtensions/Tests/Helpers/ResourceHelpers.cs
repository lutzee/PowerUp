using System.IO;

namespace Tests.Helpers
{
    internal class ResourceHelpers
    {
        public static Stream GetStreamFromResource(string name)
        {
            return typeof(ResourceHelpers).Assembly.GetManifestResourceStream(name);
        }

        public static byte[] GetFileBytesFromResource(string name)
        {
            var memoryStream = new MemoryStream();
            GetStreamFromResource(name).CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static void SaveResourceToDisk(string folder, string name)
        {
            using (var stream = GetStreamFromResource(name))
            using (var output = File.Open(Path.Combine(folder, name), FileMode.CreateNew))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(output);
            }
        }

        public static void SaveResourceToDiskAsFilename(string filename, string resourceName)
        {
            using (var stream = GetStreamFromResource(resourceName))
            using (var output = File.Open(filename, FileMode.OpenOrCreate))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(output);
            }
        }
    }
}
