using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Id.PowershellExtensions.ParsedSettings
{
    public class SettingsFileReader : ISettingsReader
    {
        private string fullPathAndFilename;
        private Stream file;

        public SettingsFileReader(string fileName, string currentDirectory)
        {
            fullPathAndFilename = fileName;
            if (!Path.IsPathRooted(fullPathAndFilename))
                fullPathAndFilename = Path.Combine(currentDirectory, fullPathAndFilename);

            file = GetStream();
        }

        public SettingsFileReader(Stream file)
        {
            this.file = file;
        }

        private Stream GetStream()
        {
            return new FileStream(fullPathAndFilename, FileMode.Open);
        }

        private bool IsXml(string contents) {
            var extension = Path.GetExtension(fullPathAndFilename);
            if (extension != null && extension.ToLower() == "xml")
                return true;

            if (!string.IsNullOrEmpty(contents))
            {
                var trimmedData = contents.TrimStart();
                if (string.IsNullOrEmpty(trimmedData))
                {
                    return false;
                }

                if (trimmedData[0] == '<')
                {
                    try
                    {
                        var parsed = XElement.Parse(contents).Value;
                        return true;
                    }
                    catch (System.Xml.XmlException)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        public IEnumerable<string> ReadSettings()
        {
            using (var sr = new StreamReader(file))
            {
                var fileString= sr.ReadToEnd();

                if(IsXml(fileString)) {
                    fileString = ConvertFromXml(fileString);
                }

                return fileString
                    .Replace("\n", "\r\n")
                    .Replace("\r\r\n", "\r\n")
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.TrimEnd())
                    .Where(x => !string.IsNullOrEmpty(x));
            }
        }

        private string ConvertFromXml(string fileString) {
            var builder = new StringBuilder();

            var root = XElement.Parse(fileString);
            
            foreach(var section in root.Descendants("section")) {
                var sectionName = section.Attribute("name");

                var sectionString = String.Empty;

                var sectionInheritance = section.Attribute("inherits");
                
                if (sectionName != null) {
                    sectionString += sectionName.Value;

                    if (sectionInheritance != null) {
                        sectionString += ":" + sectionInheritance.Value;
                    }

                    builder.AppendLine(sectionString);
                }

                foreach(var setting in section.Descendants("setting")) {
                    var name = setting.Attribute("name");
                    var value = setting.Attribute("value");
                    if (name != null && value != null)
                    {
                        builder.AppendLine("\t" + name.Value + "\t" + value.Value);
                    }
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
