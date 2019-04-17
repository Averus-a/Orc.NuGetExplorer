// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettings.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Catel;
    using Catel.IoC;
    using NuGet.Configuration;
    using Path = Catel.IO.Path;

    public class NuGetSettings : ISettings
    {
        #region Fields
        private readonly ITypeFactory _typeFactory;
        private readonly LegacyNuGetSettings _legacyNuGetSettings;
        private readonly ISettings _settings;
        #endregion

        #region Constructors
        public NuGetSettings(ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => typeFactory);

            _typeFactory = typeFactory;

            _legacyNuGetSettings = _typeFactory.CreateInstance<LegacyNuGetSettings>();
            
            var root = Path.Combine(Path.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming));
            const string configFileName = "NuGet.config";

            var fullName = Path.Combine(root, configFileName);
            if (!File.Exists(fullName))
            {
                var settings = new NuGet.Configuration.Settings(root, configFileName);
                settings.SaveToDisk();
            }

            _settings = NuGet.Configuration.Settings.LoadDefaultSettings(root, configFileName, null);

            _settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            SettingsChanged?.Invoke(sender, e);
        }
        #endregion

        #region Methods
        [Obsolete("GetValue(...) is deprecated. Please use GetSection(...) to interact with the setting values instead.")]
        public string GetValue(string section, string key, bool isPath = false)
        {
            throw new NotImplementedException();
        }

        [Obsolete("GetAllSubsections(...) is deprecated. Please use GetSection(...) to interact with the setting values instead.")]
        public IReadOnlyList<string> GetAllSubsections(string section)
        {
            throw new NotImplementedException();
        }

        [Obsolete("GetSettingValues(...) is deprecated. Please use GetSection(...) to interact with the setting values instead.")]
        public IList<SettingValue> GetSettingValues(string section, bool isPath = false)
        {
            throw new NotImplementedException();
        }

        [Obsolete("GetNestedValues(...) is deprecated. Please use GetSection(...) to interact with the setting values instead.")]
        public IList<KeyValuePair<string, string>> GetNestedValues(string section, string subSection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("GetNestedSettingValues(...) is deprecated. Please use GetSection(...) to interact with the setting values instead.")]
        public IReadOnlyList<SettingValue> GetNestedSettingValues(string section, string subSection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("SetValue(...) is deprecated. Please use AddOrUpdate(...) to add an item to a section or interact directly with the SettingItem you want.")]
        public void SetValue(string section, string key, string value)
        {
            throw new NotImplementedException();
        }

        [Obsolete("SetValues(...) is deprecated. Please use AddOrUpdate(...) to add an item to a section or interact directly with the SettingItem you want.")]
        public void SetValues(string section, IReadOnlyList<SettingValue> values)
        {
            throw new NotImplementedException();
        }

        [Obsolete("UpdateSections(...) is deprecated. Please use AddOrUpdate(...) to update an item in a section or interact directly with the SettingItem you want.")]
        public void UpdateSections(string section, IReadOnlyList<SettingValue> values)
        {
            throw new NotImplementedException();
        }

        [Obsolete("UpdateSubsections(...) is deprecated. Please use AddOrUpdate(...) to update an item in a section or interact directly with the SettingItem you want.")]
        public void UpdateSubsections(string section, string subsection, IReadOnlyList<SettingValue> values)
        {
            throw new NotImplementedException();
        }

        [Obsolete("SetNestedValues(...) is deprecated. Please use AddOrUpdate(...) to update an item in a section or interact directly with the SettingItem you want.")]
        public void SetNestedValues(string section, string subsection, IList<KeyValuePair<string, string>> values)
        {
            throw new NotImplementedException();
        }

        [Obsolete("SetNestedSettingValues(...) is deprecated. Please use AddOrUpdate(...) to update an item in a section or interact directly with the SettingItem you want.")]
        public void SetNestedSettingValues(string section, string subsection, IList<SettingValue> values)
        {
            throw new NotImplementedException();
        }

        [Obsolete("DeleteValue(...) is deprecated. Please use Remove(...) with the item you want to remove from the setttings.")]
        public bool DeleteValue(string section, string key)
        {
            throw new NotImplementedException();
        }

        [Obsolete("DeleteSection(...) is deprecated,. Please use Remove(...) with all the items in the section you want to remove from the setttings.")]
        public bool DeleteSection(string section)
        {
            throw new NotImplementedException();
        }

        public SettingSection GetSection(string sectionName)
        {
            var legacyItems = _legacyNuGetSettings.GetValues(sectionName, false);
            
            if (legacyItems.Any())
            {
                foreach (var item in legacyItems)
                {
                    _settings.AddOrUpdate(sectionName, item);
                }

                _settings.SaveToDisk();
                _legacyNuGetSettings.DeleteSection(sectionName);
            }

            var settingSection = _settings.GetSection(sectionName);

            return settingSection;
        }

        public void AddOrUpdate(string sectionName, SettingItem item)
        {
            _settings.AddOrUpdate(sectionName, item);
        }

        public void Remove(string sectionName, SettingItem item)
        {
            _settings.Remove(sectionName, item);
        }

        public void SaveToDisk()
        {
            _settings.SaveToDisk();
        }

        public IList<string> GetConfigFilePaths()
        {
            return _settings.GetConfigFilePaths();
        }

        public IList<string> GetConfigRoots()
        {
            return _settings.GetConfigRoots();
        }

        public event EventHandler SettingsChanged;
        #endregion
    }
}
