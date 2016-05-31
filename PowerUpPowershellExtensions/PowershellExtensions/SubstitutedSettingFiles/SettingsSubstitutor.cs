using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Id.PowershellExtensions.SubstitutedSettingFiles
{
    public class SettingsSubstitutor : ISettingsSubstitutor
    {
        private static readonly Regex SubfileRegex = new Regex(@"\+{(?<KEY>[^>]*?)}", RegexOptions.IgnoreCase);
        private static readonly Regex KeyRegex = new Regex(@"(?<!`)\${([^}]*)}", RegexOptions.IgnoreCase);

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

            ProcessDirectoryHierarchy(SubstituteFile, settings, environmentFolder, unreplacedSettings);
            ProcessDirectoryHierarchy(SubstituteFilesInFiles, settings, environmentFolder, unreplacedSettings);

            if (unreplacedSettings.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("The following settings could not be resolved:");
                foreach (var file in unreplacedSettings)
                {
                    builder.AppendLine(file.Key + ":");
                    builder.Append(file.Value.Aggregate("", (current, setting) => current + (setting + Environment.NewLine)));
                    builder.AppendLine();
                }

                throw new Exception(builder.ToString());
            }
        }

        private static void ProcessDirectoryHierarchy(
            Func<string, IEnumerable<KeyValuePair<string, string[]>>, IList<string>> fileAction,
            IDictionary<string, string[]> settings,
            string environmentFolder,
            Dictionary<string, IList<string>> unreplacedSettings
        )
        {
            var dirStack = new Stack<string>();
            dirStack.Push(environmentFolder);

            while (dirStack.Count > 0)
            {
                var dir = dirStack.Pop();

                foreach (var file in GetVisibleFiles(dir))
                {
                    var unreplaced = fileAction(file.FullName, settings);

                    if (unreplaced.Count > 0)
                        unreplacedSettings.Add(file.FullName, unreplaced);
                }

                foreach (var dn in GetVisibleDirectories(dir))
                {
                    dirStack.Push(dn.FullName);
                }
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
            var encodedFile = Helpers.GetFileWithEncoding(file);
            var content = encodedFile.Contents;

            content = SubstituteString(settings, content);

            var unsubstitutedSettings = GetUnsubstitutedSettings(content);

            content = ResolveEscapedSubstitutions(content);
            File.WriteAllText(file, content, encodedFile.Encoding);

            return unsubstitutedSettings;
        }

        private static IList<string> SubstituteFilesInFiles(string file, IEnumerable<KeyValuePair<string, string[]>> settings)
        {
            var encodedFile = Helpers.GetFileWithEncoding(file);
            var content = SubstituteFiles(encodedFile);

            var unsubstitutedFiles = GetUnsubstitutedSubFiles(content);

            File.WriteAllText(file, content, encodedFile.Encoding);

            return unsubstitutedFiles;
        }

        private static string ResolveEscapedSubstitutions(string content)
        {
            return content.Replace("`${", "${");
        }

        private static IList<string> GetUnsubstitutedSettings(string content)
        {
            var matches = KeyRegex.Matches(content);
            return (matches.Cast<object>().Select(match => match.ToString())).ToList();
        }

        private static IList<string> GetUnsubstitutedSubFiles(string content)
        {
            var matches = SubfileRegex.Matches(content);
            return (matches.Cast<object>().Select(match => match.ToString())).ToList();
        }

        private static string SubstituteString(string content, string find, string replace, char escapeCharacter)
        {
            var nextPosition = content.IndexOf(find, 0, StringComparison.Ordinal);

            while (nextPosition != -1)
            {
                var toReplace = true;
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

                nextPosition = content.IndexOf(find, nextPosition + 1, StringComparison.Ordinal);
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
                    for (var i = 0; i < keyValuePair.Value.Length; i++)
                    {
                        content = SubstituteString(content, "${" + keyValuePair.Key + "}[" + i + "]", keyValuePair.Value[i], '`');
                    }
                }
            }
            return content;
        }

        private static string SubstituteFiles(EncodedFile file)
        {
            var matches = SubfileRegex.Matches(file.Contents).Cast<Match>();

            if (!matches.Any())
                return file.Contents;

            var content = file.Contents;

            foreach (var match in matches)
            {
                var filename = match.Groups["KEY"].Value;
                var fullSubFilePath = Path.Combine(file.Directory.FullName, filename);

                if (!File.Exists(fullSubFilePath))
                {
                    throw new Exception(string.Format("Sub-file \"{0}\" not found when substituting \"{1}\"", fullSubFilePath, file.File.FullName));
                }

                var subFile = Helpers.GetFileWithEncodingNoBom(fullSubFilePath);

                //TODO: Check for differing encodings

                content = SubstituteString(content, "+{" + filename + "}", subFile.Contents, '`');
            }

            return content;
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