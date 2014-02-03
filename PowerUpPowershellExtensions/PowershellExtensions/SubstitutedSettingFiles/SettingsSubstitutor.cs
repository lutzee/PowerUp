using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using href.Utils;

namespace Id.PowershellExtensions.SubstitutedSettingFiles
{
    public class SettingsSubstitutor : ISettingsSubstitutor
    {
        private class EncodedFile
        {
            public string Contents { get; set; }
            public Encoding Encoding { get; set; }
        }

        public void CreateSubstitutedDirectory(string templatesDirectory, string targetDirectory, string environment, IDictionary<string, string[]> settings)
        {
            var fullEnvironmentFolder = Path.Combine(targetDirectory, environment);

            CopyDirectoryRecursively(templatesDirectory, fullEnvironmentFolder);
            SubstituteDirectory(fullEnvironmentFolder, settings);
        }
        

        private static void SubstituteDirectory(string environmentFolder, IDictionary<string, string[]> settings)
        {
            var unreplacedSettings = new Dictionary<string, IList<string>>();

            if (settings.Count == 0)
                return;

            var dirStack = new Stack<string>();
            dirStack.Push(environmentFolder);

            while (dirStack.Count > 0)
            {
                var dir = dirStack.Pop();

                foreach (var file in GetVisibleFiles(dir))
                {
                    var unreplaced = SubstituteFile(file.FullName, settings);

                    if (unreplaced.Count > 0)
                        unreplacedSettings.Add(file.FullName, unreplaced);
                }

                foreach (var dn in GetVisibleDirectories(dir))
                {
                    dirStack.Push(dn.FullName);
                }
            }

            if (unreplacedSettings.Count >0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("The following settings could not be resolved:");
                foreach(var file in unreplacedSettings)
                {
                    builder.AppendLine(file.Key + ":" );
                    builder.Append(file.Value.Aggregate("", (current, setting) => current + (setting + Environment.NewLine)));
                    builder.AppendLine();
                }

                throw new Exception(builder.ToString());
            }
        }
        
        private static IEnumerable<FileInfo> GetVisibleFiles(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetFiles().Where(x => !IsResourceHidden(x));
        }

        private static IEnumerable<FileInfo> GetVisibleFiles(string directory)
        {
            return GetVisibleFiles(new DirectoryInfo(directory));
        }

        private static IEnumerable<DirectoryInfo> GetVisibleDirectories(string directory)
        {
            return GetVisibleDirectories(new DirectoryInfo(directory));
        }

        private static IEnumerable<DirectoryInfo> GetVisibleDirectories(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetDirectories().Where(x => !IsResourceHidden(x));
        }

        private static bool IsResourceHidden(FileSystemInfo info)
        {
            return (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        private static IList<string> SubstituteFile(string file, IEnumerable<KeyValuePair<string, string[]>> settings)
        {
            var encodedFile = GetFileWithEncoding(file);
            var content = encodedFile.Contents;

            content = SubstituteString(settings, content);


            var unsubstitutedSettings = GetUnsubstitutedSettings(content);

            content = ResolveEscapedSubstitutions(content);
            File.WriteAllText(file, content, encodedFile.Encoding);

            return unsubstitutedSettings;
        }

        private static string ResolveEscapedSubstitutions(string content)
        {
            return content.Replace("`${", "${");
        }

        private static IList<string> GetUnsubstitutedSettings(string content)
        {
            var matches = new Regex(@"(?<!`)\${([^}]*)}").Matches(content);
            return (matches.Cast<object>().Select(match => match.ToString())).ToList();
        }

        private static string SubstituteString(string content, string find, string replace, char escapeCharacter)
        {
            var nextPosition = content.IndexOf(find, 0, System.StringComparison.Ordinal);

            while (nextPosition != -1)
            {
                bool toReplace = true;
                if (nextPosition > 0)
                {
                    if (content[nextPosition - 1] == escapeCharacter)
                        toReplace = false;
                }

                if (toReplace)
                {
                    content = content.Remove(nextPosition, find.Length);
                    content = content.Insert(nextPosition, replace);
                }

                nextPosition = content.IndexOf(find, nextPosition + 1, System.StringComparison.Ordinal);
            }

            return content;
        }

        private static string SubstituteString(IEnumerable<KeyValuePair<string, string[]>> settings, string content)
        {
            foreach (var keyValuePair in settings)
            {
                if (keyValuePair.Value.Length == 1)
                {
                    content = SubstituteString(content, "${" + keyValuePair.Key + "}", keyValuePair.Value[0], '`');                    
                }
                else
                {
                    for (var i = 0; i < keyValuePair.Value.Length; i++ )
                    {
                        content = SubstituteString(content,"${" + keyValuePair.Key + "}[" + i + "]", keyValuePair.Value[i], '`');
                    }
                }
            }
            return content;
        }

        private static EncodedFile GetFileWithEncoding(string file)
        {
            using (Stream fs = File.Open(file, FileMode.Open))
            {
                var rawData = new byte[fs.Length];
                fs.Read(rawData, 0, (int)fs.Length);
                var encoding = EncodingTools.DetectInputCodepage(rawData);
                return new EncodedFile { Contents = encoding.GetString(rawData), Encoding = encoding };
            }
        }

        private static void CopyDirectoryRecursively(string source, string destination)
        {
            var sourceDirectoryInfo = new DirectoryInfo(source);

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var fileInfo in GetVisibleFiles(sourceDirectoryInfo))
                File.Copy(fileInfo.FullName, Path.Combine(destination, fileInfo.Name), true);

            foreach (var sourceSubDirectory in GetVisibleDirectories(sourceDirectoryInfo))
            {
                var destinationSubDirectory = new DirectoryInfo(destination).CreateSubdirectory(sourceSubDirectory.Name);
                CopyDirectoryRecursively(sourceSubDirectory.FullName, destinationSubDirectory.FullName);
            }
        }


       
    }
}