// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.XML;
    using ColossalFramework.IO;

    /// <summary>
    /// The mod's XML settings file.
    /// </summary>
    [XmlRoot("MorePathUnits")]
    public class ModSettings : SettingsXMLBase
    {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "MorePathUnits.xml");

        /// <summary>
        /// Gets or sets a value indicating whether the PathUnit limit should be doubled on virgin savegames.
        /// </summary>
        [XmlElement("DoubleLimit")]
        public bool XMLDoubleLimit { get => PathDeserialize.DoubleLimit; set => PathDeserialize.DoubleLimit = value; }

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);
    }
}
