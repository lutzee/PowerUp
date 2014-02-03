using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace Id.PowershellExtensions.ParsedSettings
{
    public class SettingsParser
    {
        private const string KeyRegexPattern = @"\${(?<KEY>[^>]*?)}";
        private const string CommentPattern = @"^\s{0,}#";
        private readonly Regex _keyRegex = new Regex(KeyRegexPattern, RegexOptions.IgnoreCase);
        private readonly Regex _commentRegex = new Regex(CommentPattern);
        private const string DefaultSection = "default";

        public Dictionary<string, string[]> Parse(IEnumerable<string> settingsLines, string section, char settingDelimiter)
        {
            var output = ReadSettingsForSection(settingsLines, section);

            if (ContainsDependentSettings(output))
                output = ResolveDependentSettings(output, null, null);

            return ParseSettings(output, settingDelimiter);
        }

        public Dictionary<string, string> ParseWithoutSeparatingValues(IEnumerable<string> settingsLines, string section)
        {
            var output = ReadSettingsForSection(settingsLines, section);

            if (ContainsDependentSettings(output))
                output = ResolveDependentSettings(output, null, null);

            return output;
        }

        public Dictionary<string, string> ReadRawSettingsForSection(IEnumerable<string> settingsLines, string section)
        {
            return ReadSettingsForSectionNoResolution(settingsLines, section);
        }

        private static Dictionary<string, string[]> ParseSettings(Dictionary<string, string> output, char settingDelimiter)
        {
            return output.ToDictionary(setting => setting.Key, setting => ParseSetting(setting.Value, settingDelimiter));
        }

        private static string[] ParseSetting(string setting, char settingDelimiter)
        {
            var textReader = new StringReader(setting);
            var parser = new TextFieldParser(textReader)
                             {
                                 Delimiters = new[] { settingDelimiter.ToString() },
                                 TextFieldType = FieldType.Delimited,
                                 HasFieldsEnclosedInQuotes = true,
                                 TrimWhiteSpace = true
                             };

            var fields = parser.ReadFields();

            return fields ?? new[] { "" };
        }

        private Dictionary<string, string> ReadSettingsForSection(IEnumerable<string> settingsLines, string section)
        {
            var output = new Dictionary<string, string>();

            var sections = GetSectionNames(settingsLines).ToList();
            var resolvedInheritanceChain = ResolveSectionInheritanceChain(sections, GetSectionNameFromKey(section)).ToList();

            ReadSettingsForSectionInternal(settingsLines, DefaultSection, ref output);

            foreach (var mode in resolvedInheritanceChain)
            {
                ReadSettingsForSectionInternal(settingsLines, mode, ref output);
            }

            return output;
        }

        private Dictionary<string, string> ReadSettingsForSectionNoResolution(IEnumerable<string> settingsLines, string section)
        {
            var output = new Dictionary<string, string>();
          
            ReadSettingsForSectionInternal(settingsLines, section, ref output);

            return output;
        }

        public IEnumerable<string> GetSectionNames(IEnumerable<string> settingsLines)
        {
            foreach (var line in settingsLines.Where(x => !_commentRegex.IsMatch(x)))
            {
                if (!char.IsWhiteSpace(line[0]))
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<string> ResolveSectionInheritanceChain(IEnumerable<string> sections, string section)
        {
            var locatedSection = sections.FirstOrDefault(x => GetSectionNameFromKey(x).Equals(section, StringComparison.InvariantCultureIgnoreCase));

            if (locatedSection.Contains(":"))
            {
                var inheritsFrom = locatedSection.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ElementAt(1);
                var chain = ResolveSectionInheritanceChain(sections, inheritsFrom);
                foreach (var mode in chain)
                {
                    yield return mode;
                }
                yield return GetSectionNameFromKey(locatedSection);
            }
            else
            {
                yield return locatedSection;
            }
        }

        public static string GetSectionNameFromKey(string key)
        {
            return key.Contains(":")
                       ? key.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(m => m.Trim()).First()
                       : key;
        }

        private void ReadSettingsForSectionInternal(IEnumerable<string> settingsLines, string section, ref Dictionary<string, string> existingSettings)
        {
            bool isSetting = false;

            foreach (var line in settingsLines.Where(x => !_commentRegex.IsMatch(x)))
            {
                if (!char.IsWhiteSpace(line[0]))
                {
                    var locatedSection = GetSectionNameFromKey(line);
                    isSetting = locatedSection.Equals(section, StringComparison.InvariantCultureIgnoreCase);
                }
                else if (isSetting)
                {
                    string[] setting =
                        line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).Where(
                            x => !string.IsNullOrEmpty(x.Trim())).ToArray();
                    if (setting.Length > 0)
                    {
                        string key = setting[0].Trim();
                        string value = setting.Length == 1 ? string.Empty : setting[1].Trim();

                        if (existingSettings.Keys.Any(x => x.ToLowerInvariant() == key.ToLowerInvariant()))
                        {
                            string oldKey = existingSettings.Keys.First(x => x.ToLowerInvariant() == key.ToLowerInvariant());
                            existingSettings[oldKey] = value;
                        }
                        else
                        {
                            existingSettings.Add(key, value);
                        }
                    }
                }
            }
        }

        private bool ContainsDependentSettings(Dictionary<string, string> settings)
        {
            if (settings.Values.Any(x => _keyRegex.IsMatch(x)))
            {
                //Validate all keys that need to be resolved, can be resolved
                foreach (string key in settings.Keys)
                {
                    MatchCollection matches = _keyRegex.Matches(settings[key]);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (!settings.ContainsKey(match.Groups["KEY"].Value))
                                throw new KeyNotFoundException("The setting " + key + " has a dependency on the unknown setting " + match.Groups["KEY"].Value);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private Dictionary<string, string> ResolveDependentSettings(Dictionary<string, string> settings, string keyToResolve, string rootKeyToResolve)
        {
            while (settings.Values.Any(x => _keyRegex.IsMatch(x)))
            {
                if (keyToResolve == null)
                {
                    string setting = settings.Values.First(x => _keyRegex.IsMatch(x));
                    keyToResolve = _keyRegex.Match(setting).Groups["KEY"].Value;

                    if (rootKeyToResolve == null)
                        rootKeyToResolve = keyToResolve;
                }
                else if (rootKeyToResolve.Equals(keyToResolve, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Circular dependency detected");

                string value = settings[keyToResolve];
                var matches = _keyRegex.Matches(value);

                if (matches.Count > 0)
                {
                    return ResolveDependentSettings(settings, matches[0].Groups["KEY"].Value, rootKeyToResolve);
                }

                string[] keys = settings.Keys.ToArray();
                foreach (string key in keys.Where(key => settings[key].Contains("${" + keyToResolve + "}")))
                {
                    settings[key] = settings[key].Replace("${" + keyToResolve + "}", settings[keyToResolve]);
                }

                rootKeyToResolve = null;
                keyToResolve = null;
            }

            return settings;
        }
    }
}
