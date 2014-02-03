using System.Collections.Generic;

namespace Id.PowershellExtensions.ParsedSettings
{
    public interface ISettingsReader
    {
        IEnumerable<string> ReadSettings();
    }
}