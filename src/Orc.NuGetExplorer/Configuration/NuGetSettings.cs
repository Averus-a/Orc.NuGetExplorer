// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettings.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Catel;
    using Catel.Configuration;
    using NuGet;
    using NuGet.Configuration;

#pragma warning disable 618
    internal class NuGetSettings : ISettings
    {
        #region Fields
        private const char Separator = '|';
        private const string SectionListKey = "NuGet_sections";

        private readonly IConfigurationService _configurationService;
        #endregion

        #region Constructors
        public NuGetSettings(IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => configurationService);

            _configurationService = configurationService;
        }
        #endregion

        #region Methods
        public string GetValue(string section, string key, bool isPath)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            var settingValue = GetValues(section, isPath).FirstOrDefault(x => string.Equals(x.Key, key));

            var result = settingValue is null ? string.Empty : settingValue.Value;

            return result;
        }

        public IReadOnlyList<string> GetAllSubsections(string section)
        {
            throw new NotImplementedException();
        }

        public IList<SettingValue> GetSettingValues(string section, bool isPath = false)
        {
            throw new NotImplementedException();
        }

        public IList<KeyValuePair<string, string>> GetNestedValues(string section, string subSection)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => subSection);

            return GetNuGetValues(section, subSection);
        }

        public IReadOnlyList<SettingValue> GetNestedSettingValues(string section, string subSection)
        {
            throw new NotImplementedException();
        }

        public IList<KeyValuePair<string, string>> GetValues(string section, bool isPath)
        {
            Argument.IsNotNullOrWhitespace(() => section);

            return GetNuGetValues(section, isPath);
        }

        public void SetValue(string section, string key, string value)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            SetNuGetValues(section, new[] {new KeyValuePair<string, string>(key, value)});
        }

        public void SetValues(string section, IReadOnlyList<SettingValue> values)
        {
            foreach (var value in values)
            {
                SetValue(section, value.Key, value.Value);
            }
        }

        public void UpdateSections(string section, IReadOnlyList<SettingValue> values)
        {
            DeleteSection(section);

            foreach (var value in values)
            {
                SetValue(section, value.Key, value.Value);
            }
        }

        public void UpdateSubsections(string section, string subsection, IReadOnlyList<SettingValue> values)
        {
            throw new NotImplementedException();
        }


        public void SetNestedValues(string section, string key, IList<KeyValuePair<string, string>> values)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            SetNuGetValues(section, key, values);
        }

        public void SetNestedSettingValues(string section, string subsection, IList<SettingValue> values)
        {
            throw new NotImplementedException();
        }

        public bool DeleteValue(string section, string key)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            try
            {
                var valuesListKey = GetSectionValuesListKey(section);
                var keysString = _configurationService.GetRoamingValue<string>(valuesListKey);
                if (string.IsNullOrEmpty(keysString))
                {
                    return true;
                }

                var newKeys = keysString.Split(Separator).Where(x => !string.Equals(x, key));
                keysString = string.Join(Separator.ToString(), newKeys);
                _configurationService.SetRoamingValue(valuesListKey, keysString);

                var valueKey = GetSectionValueKey(section, key);
                _configurationService.SetRoamingValue(valueKey, string.Empty);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool DeleteSection(string section)
        {
            Argument.IsNotNullOrWhitespace(() => section);

            var result = true;

            try
            {
                var sectionsString = _configurationService.GetRoamingValue<string>(SectionListKey);
                if (string.IsNullOrEmpty(sectionsString))
                {
                    return true;
                }

                var newSections = sectionsString.Split(Separator).Where(x => !string.Equals(x, section));
                sectionsString = string.Join(Separator.ToString(), newSections);
                _configurationService.SetRoamingValue(SectionListKey, sectionsString);

                var values = GetValues(section, false);
                if (values == null)
                {
                    return false;
                }

                foreach (var settingValue in values)
                {
                    result = result && DeleteValue(section, settingValue.Key);
                }
            }
            catch
            {
                return false;
            }

            return result;
        }

        public SettingSection GetSection(string sectionName)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(string sectionName, SettingItem item)
        {
            throw new NotImplementedException();
        }

        public void Remove(string sectionName, SettingItem item)
        {
            throw new NotImplementedException();
        }

        public void SaveToDisk()
        {
            throw new NotImplementedException();
        }

        public IList<string> GetConfigFilePaths()
        {
            throw new NotImplementedException();
        }

        public IList<string> GetConfigRoots()
        {
            throw new NotImplementedException();
        }

        public event EventHandler SettingsChanged;

        private void SetNuGetValues(string section, IList<KeyValuePair<string, string>> values)
        {
            Argument.IsNotNullOrWhitespace(() => section);

            EnsureSectionExists(section);

            var valuesListKey = GetSectionValuesListKey(section);
            UpdateKeysList(values, valuesListKey);
            foreach (var keyValuePair in values)
            {
                SetNuGetValue(section, keyValuePair.Key, keyValuePair.Value);
            }
        }

        private void EnsureSectionExists(string section)
        {
            Argument.IsNotNullOrWhitespace(() => section);

            var sectionsString = _configurationService.GetRoamingValue(SectionListKey, string.Empty);
            var sections = sectionsString.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!sections.Contains(section))
            {
                sections.Add(section);
                sectionsString = string.Join(Separator.ToString(), sections);
                _configurationService.SetRoamingValue(SectionListKey, sectionsString);
            }
        }

        private void SetNuGetValues(string section, string subsection, IList<KeyValuePair<string, string>> values)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => subsection);

            EnsureSectionExists(section);

            var valuesListKey = GetSubsectionValuesListKey(section, subsection);
            UpdateKeysList(values, valuesListKey);
            foreach (var keyValuePair in values)
            {
                SetNuGetValue(section, subsection, keyValuePair.Key, keyValuePair.Value);
            }
        }

        private void UpdateKeysList(IList<KeyValuePair<string, string>> values, string valuesListKey)
        {
            var valueKeysString = _configurationService.GetRoamingValue<string>(valuesListKey);

            var existedKeys = string.IsNullOrEmpty(valueKeysString) ? Enumerable.Empty<string>() : valueKeysString.Split(Separator);
            var keysToSave = values.Select(x => x.Key);

            var newValueKeysString = string.Join(Separator.ToString(), existedKeys.Union(keysToSave));
            _configurationService.SetRoamingValue(valuesListKey, newValueKeysString);
        }

        private string ConvertToFullPath(string result)
        {
            return result;
        }

        private IList<KeyValuePair<string, string>> GetNuGetValues(string section, bool isPath = false)
        {
            Argument.IsNotNullOrWhitespace(() => section);

            var valuesListKey = GetSectionValuesListKey(section);
            var valueKeysString = _configurationService.GetRoamingValue<string>(valuesListKey);
            if (string.IsNullOrEmpty(valueKeysString))
            {
                return new List<KeyValuePair<string, string>>();
            }
            var keys = valueKeysString.Split(Separator);

            return keys.Select(key => GetNuGetValue(section, key, isPath)).ToList();
        }

        private IList<KeyValuePair<string, string>> GetNuGetValues(string section, string subsection, bool isPath = false)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => subsection);

            var valuesListKey = GetSubsectionValuesListKey(section, subsection);
            var valueKeysString = _configurationService.GetRoamingValue<string>(valuesListKey);
            if (string.IsNullOrEmpty(valueKeysString))
            {
                return new List<KeyValuePair<string, string>>();
            }

            var keys = valueKeysString.Split(Separator);

            return keys.Select(key => GetNuGetValue(section, subsection, key, isPath)).ToList();
        }

        private KeyValuePair<string, string> GetNuGetValue(string section, string key, bool isPath)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            var combinedKey = GetSectionValueKey(section, key);
            var value = _configurationService.GetRoamingValue<string>(combinedKey);

            if (isPath)
            {
                value = ConvertToFullPath(value);
            }

            return new KeyValuePair<string, string>(key, value);
        }

        private KeyValuePair<string, string> GetNuGetValue(string section, string subsection, string key, bool isPath)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => subsection);
            Argument.IsNotNullOrWhitespace(() => key);

            var combinedKey = GetSubsectionValueKey(section, subsection, key);
            var value = _configurationService.GetRoamingValue<string>(combinedKey);

            if (isPath)
            {
                value = ConvertToFullPath(value);
            }

            return new KeyValuePair<string, string>(key, value);
        }

        private void SetNuGetValue(string section, string key, string value)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => key);

            var combinedKey = GetSectionValueKey(section, key);
            _configurationService.SetRoamingValue(combinedKey, value);
        }

        private void SetNuGetValue(string section, string subsection, string key, string value)
        {
            Argument.IsNotNullOrWhitespace(() => section);
            Argument.IsNotNullOrWhitespace(() => subsection);
            Argument.IsNotNullOrWhitespace(() => key);

            var combinedKey = GetSubsectionValueKey(section, subsection, key);
            _configurationService.SetRoamingValue(combinedKey, value);
        }

        private string GetSectionValueKey(string section, string key)
        {
            return $"NuGet_{section}_value_{key}";
        }

        private string GetSubsectionValueKey(string section, string subsection, string key)
        {
            return $"NuGet_{section}_{subsection}_value_{key}";
        }

        private static string GetSectionValuesListKey(string section)
        {
            return $"NuGet_{section}_values";
        }

        private static string GetSubsectionValuesListKey(string section, string subsection)
        {
            return $"NuGet_{section}_{subsection}_values";
        }
        #endregion
    }
#pragma warning restore 618

}
