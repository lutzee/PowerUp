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
        public bool AppendReservedSettings
        {
            get { return _appendReservedSettings; }
            set { _appendReservedSettings = value; }
        }

        public Action<string> WriteWarning
        {
            get { return _writeWarning; }
            set { _writeWarning = value; }
        }

        private const string KeyRegexPattern = @"\${(?<KEY>[^>]*?)}";
        private const string CommentPattern = @"^\s{0,}#";
        private readonly Regex _keyRegex = new Regex(KeyRegexPattern, RegexOptions.IgnoreCase);
        private readonly Regex _commentRegex = new Regex(CommentPattern);
        private bool _appendReservedSettings = true;
        private Action<string> _writeWarning;
        private HashSet<string> _warnings;

        private const string DefaultSection = "default";
        private static readonly string[] IgnoredSectionsForInfo = { "", "package.id", "PackageInformation" };
        private static readonly string[] Separators = { "\t", "  " }; //Tabs and TWO spaces 

        public SettingsParser()
        {
            _writeWarning = s => { };
            _warnings = new HashSet<string>();
        }

        public Dictionary<string, string[]> Parse(IEnumerable<string> settingsLines, string section, char settingDelimiter)
        {
            return Parse(settingsLines, new string[0], section, settingDelimiter);
        }

        public Dictionary<string, string[]> Parse(IEnumerable<string> settingsLines, IEnumerable<string> overrideSettingsLines, string section, char settingDelimiter)
        {
            _warnings = new HashSet<string>();
            var combined = settingsLines.Concat(overrideSettingsLines);
            ValidateSectionNames(settingsLines, overrideSettingsLines, section);

            if (!IgnoredSectionsForInfo.Contains(section))
            {
                GatherDuplicateSettingInfo(combined, section);
                WriteWarnings();
            }

            return ParseCore(combined, section, settingDelimiter);
        }

        public void Validate(
            IEnumerable<string> settingsLines
            )
        {
            Validate(settingsLines, new string[0]);
        }

        public void Validate(
            IEnumerable<string> settingsLines,
            IEnumerable<string> overrideSettingsLines
        )
        {
            _warnings = new HashSet<string>();
            var combined = settingsLines.Concat(overrideSettingsLines);
            var sectionNames = GetSectionNames(combined)
                .Select(GetSectionNameFromKey)
                .Except(new [] { DefaultSection }, StringComparer.OrdinalIgnoreCase)
                .Distinct();

            foreach (var sectionName in sectionNames)
            {
                ValidateSectionNames(settingsLines, overrideSettingsLines, sectionName);
                GatherDuplicateSettingInfo(combined, sectionName);
            }

            WriteWarnings();
        }

        public Dictionary<string, string[]> ParseCore(IEnumerable<string> settingsLines, string section, char settingDelimiter)
        {
            var output = ReadSettingsForSection(settingsLines, section);

            if (ContainsDependentSettings(output))
                output = ResolveDependentSettings(output, null, null);

            return ParseSettings(output, settingDelimiter);
        }

        private void GatherDuplicateSettingInfo(IEnumerable<string> settingsLines, string sectionName)
        {
            var sectionNames = GetSectionNames(settingsLines).Distinct();
            var sectionChain = ResolveSectionInheritanceChainAsLinkedList(sectionNames, sectionName);

            try
            {
                var @default = sectionChain.Find(DefaultSection);
                if (@default == null)
                {
                    sectionChain.AddFirst(DefaultSection);
                }

                //2 checks:
                //On *un-parsed* settings, check that a setting declaration isn't repeated from one environment to another
                //On *parsed* settings, check that setting doesn't start out as one value, change to another, and end-up back at starting value
                var allParsedSettings = sectionChain.Select(
                               s => new { key = s, value = ReadSettingsForSection(settingsLines, s) })
                               .ToDictionary(x => x.key, x => x.value);

                var allUnParsedSettings = sectionChain.Select(
                    s => new { key = s, value = ReadSettingsForSectionNoResolution(settingsLines, s) })
                    .ToDictionary(x => x.key, x => x.value);

                var section = sectionChain.Last;
                var sectionParsedSettings = allParsedSettings[section.Value];
                var sectionUnParsedSettings = allUnParsedSettings[section.Value];

                foreach (var key in sectionParsedSettings.Keys)
                {
                    var finalValue = sectionParsedSettings[key];
                    var lastSectionWithFinalValue = section.Value;
                    var hasChanged = false;

                    foreach (var ancestor in GetAncestors(sectionChain))
                    {
                        var ancestorParsedSettings = allParsedSettings[ancestor];

                        var areEqual = SettingsAreEqual(ancestorParsedSettings[key], finalValue);
                        if (areEqual && hasChanged)
                        {
                            _warnings.Add(
                                string.Format(
                                    "'{0}' in {1} reverts to value '{2}' from {3}",
                                    key, lastSectionWithFinalValue, ancestorParsedSettings[key], ancestor));
                        }
                        else if (areEqual)
                        {
                            lastSectionWithFinalValue = ancestor;

                            var ancestorUnParsedSettings = allUnParsedSettings[ancestor];
                            if (sectionUnParsedSettings.ContainsKey(key))
                            {
                                var declaration = sectionUnParsedSettings[key];
                                if (ancestorUnParsedSettings.ContainsKey(key) && ancestorUnParsedSettings[key].Equals(declaration))
                                {
                                    _warnings.Add(
                                        string.Format(
                                            "'{0}' in {1} duplicates value from {2}",
                                            key, section.Value, ancestor));
                                }
                            }
                        }
                        else
                        {
                            hasChanged = true;
                        }
                    }
                }
            }
            catch
            {
                //GULP!
                //We deliberately don't want anything in here to throw - any issues with the 
                //way settings are structured will be picked-up by the main routines.
            }
        }

        private void WriteWarnings()
        {
            if (_warnings != null && _warnings.Any())
            {
                foreach (var warning in _warnings.OrderBy(x => x))
                {
                    WriteWarning(warning);
                }
            }
        }

        public Dictionary<string, string> ParseWithoutSeparatingValues(IEnumerable<string> settingsLines, IEnumerable<string> overrideSettingsLines, string section)
        {
            ValidateSectionNames(settingsLines, overrideSettingsLines, section);

            var combined = settingsLines.Concat(overrideSettingsLines);
            var output = ReadSettingsForSection(combined, section);

            if (ContainsDependentSettings(output))
                output = ResolveDependentSettings(output, null, null);

            return output;
        }

        private void ValidateSectionNames(IEnumerable<string> settingsLines, IEnumerable<string> overrideSettingsLines, string section)
        {
            var settingsSectionNames = GetSectionNames(settingsLines);
            var overridesSectionNames = GetSectionNames(overrideSettingsLines);

            if (!overridesSectionNames.Any())
                return;

            if (!overridesSectionNames.All(name => settingsSectionNames.Any(s => s.StartsWith(name, StringComparison.OrdinalIgnoreCase))))
                throw new Exception(string.Format("Could not find section \"{0}\"", section));
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

            ValidateSettingsAreNotReserved(output);
            if (AppendReservedSettings)
            {
                AppendReservedSettingsInternal(output, section);
            }

            return output;
        }

        private void AppendReservedSettingsInternal(Dictionary<string, string> output, string section)
        {
            output["environment.profile"] = section;
        }

        private void ValidateSettingsAreNotReserved(Dictionary<string, string> output)
        {
            if (output.Keys.Any(x => x.Equals("environment.profile", StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Reserved key \"environment.profile\" found");
            }
        }

        private Dictionary<string, string> ReadSettingsForSectionNoResolution(IEnumerable<string> settingsLines, string section)
        {
            var output = new Dictionary<string, string>();

            ReadSettingsForSectionInternal(settingsLines, section, ref output);

            return output;
        }

        private IEnumerable<string> GetSectionNames(IEnumerable<string> settingsLines)
        {
            foreach (var line in settingsLines.Where(x => !_commentRegex.IsMatch(x)))
            {
                if (!char.IsWhiteSpace(line[0]))
                {
                    yield return line;
                }
            }
        }

        private static LinkedList<string> ResolveSectionInheritanceChainAsLinkedList(IEnumerable<string> sections, string section)
        {
            return new LinkedList<string>(ResolveSectionInheritanceChain(sections, section));
        }

        private static IEnumerable<string> ResolveSectionInheritanceChain(IEnumerable<string> sections, string section)
        {
            var locatedSection = sections.FirstOrDefault(x => GetSectionNameFromKey(x).Equals(section, StringComparison.OrdinalIgnoreCase));

            if (locatedSection == null)
                throw new Exception(string.Format("Could not find section \"{0}\"", section));

            if (locatedSection.Contains(":"))
            {
                var inheritsFrom = locatedSection.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim()).ElementAt(1);

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

        private static string GetSectionNameFromKey(string key)
        {
            return key.Contains(":")
                       ? key.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(m => m.Trim()).First()
                       : key;
        }

        private static bool SettingsAreEqual(object x, object y)
        {
            var s = x as string;
            if (s != null)
            {
                return s.Equals(y as string);
            }

            var array = x as string[];
            return array != null && array.SequenceEqual(y as string[] ?? new string[0]);
        }

        private static IEnumerable<T> GetAncestors<T>(LinkedList<T> chain)
        {
            var current = chain.Last;
            var prev = current.Previous;

            while (prev != null)
            {
                yield return prev.Value;
                prev = prev.Previous;
            }
        }

        private void ReadSettingsForSectionInternal(IEnumerable<string> settingsLines, string section, ref Dictionary<string, string> existingSettings)
        {
            bool isSetting = false;

            foreach (var line in settingsLines.Where(x => !_commentRegex.IsMatch(x)))
            {
                if (!char.IsWhiteSpace(line[0]))
                {
                    var locatedSection = GetSectionNameFromKey(line);
                    isSetting = locatedSection.Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (isSetting)
                {
                    var setting = GetKeyValueFromLine(line.Trim());

                    if (setting != null)
                    {
                        var key = setting.Item1;
                        var value = setting.Item2;

                        if (existingSettings.Keys.Any(x => x.Equals(key, StringComparison.OrdinalIgnoreCase)))
                        {
                            var oldKey = existingSettings.Keys.First(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
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

        private static Tuple<string, string> GetKeyValueFromLine(string line)
        {
            foreach (var separator in Separators)
            {
                if (line.Contains(separator))
                {
                    var key = line.SubstringBefore(separator, StringComparison.OrdinalIgnoreCase);
                    var value = line.SubstringAfter(separator, StringComparison.OrdinalIgnoreCase);

                    return new Tuple<string, string>(key, (value ?? string.Empty).Trim());
                }
            }

            return string.IsNullOrWhiteSpace(line)
                ? null
                : new Tuple<string, string>(line.Trim(), string.Empty);
        }

        private bool ContainsDependentSettings(Dictionary<string, string> settings)
        {
            if (settings.Values.Any(x => _keyRegex.IsMatch(x)))
            {
                //Validate all keys that need to be resolved, can be resolved
                foreach (string key in settings.Keys)
                {
                    var matches = _keyRegex.Matches(settings[key]);
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
                else if (rootKeyToResolve.Equals(keyToResolve, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Circular dependency detected");

                var value = settings[keyToResolve];
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
