using System;
using System.Collections;
using System.Collections.Generic;

namespace Id.PowershellExtensions.ParsedSettings
{
    public class OverrideSettingsReader : ISettingsReader
    {
        private readonly Hashtable _overrideSettings;
        private readonly string _section;

        public OverrideSettingsReader(Hashtable overrideSettings, string section)
        {
            _overrideSettings = overrideSettings;
            _section = section;
        }

        public IEnumerable<string> ReadSettings()
        {
            var lines = new List<string>();

            if ((_overrideSettings == null) || (_overrideSettings.Keys.Count <= 0))
            {
                return lines;
            }

            lines.Add(_section);

            foreach (var key in _overrideSettings.Keys)
            {
                lines.Add(String.Format("\t{0}\t{1}", key, _overrideSettings[key]));
            }

            return lines;
        }
    }
}