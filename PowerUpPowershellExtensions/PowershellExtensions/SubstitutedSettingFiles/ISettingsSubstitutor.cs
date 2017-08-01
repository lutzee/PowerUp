using System.Collections.Generic;

namespace Id.PowershellExtensions.SubstitutedSettingFiles
{
    public interface ISettingsSubstitutor
    {
        void CreateSubstitutedDirectory(string templateDirectory, string targetDirectory, IDictionary<string, string[]> settings);
    }
}