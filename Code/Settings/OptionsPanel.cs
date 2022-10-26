// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// The mod's options panel.
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = Margin * 2f;
        private const float LeftMargin = 24f;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        internal OptionsPanel()
        {
            // Add controls.
            // Y position indicator.
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Double units title and checkbox.
            UISpacers.AddOptionsSpacer(this, Margin, currentY, OptionsPanelManager<OptionsPanel>.PanelWidth - Margin - Margin);
            currentY += TitleMargin;
            UILabel label = UILabels.AddLabel(this, Margin, currentY, Translations.Translate("INCREASE_UNITS"), textScale: 1.2f);
            currentY += label.height + TitleMargin;
            UICheckBox doubleCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("DOUBLE_UNITS"));
            doubleCheck.isChecked = PathDeserialize.DoubleLimit;
            doubleCheck.eventCheckChanged += (c, isChecked) => { PathDeserialize.DoubleLimit = isChecked; };
            currentY += doubleCheck.height + Margin;

            // Add double units text.
            UILabel boost1 = UILabels.AddLabel(this, LeftMargin, currentY, Translations.Translate("EXPANDED_WARN"), OptionsPanelManager<OptionsPanel>.PanelWidth - LeftMargin - LeftMargin, 0.9f);
            currentY += boost1.height + TitleMargin;
            UILabel boost2 = UILabels.AddLabel(this, LeftMargin, currentY, Translations.Translate("EXPANDED_ALWAYS"), OptionsPanelManager<OptionsPanel>.PanelWidth - LeftMargin - LeftMargin, 0.9f);
            currentY += boost2.height + TitleMargin;
        }
    }
}