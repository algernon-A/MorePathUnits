// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// The mod's options panel.
    /// </summary>
    public class OptionsPanel : OptionsPanelBase
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = Margin * 3f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 55f;

        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
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
            float titleWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (TitleMargin * 2f);
            UISpacers.AddTitleSpacer(this, Margin, currentY, titleWidth, Translations.Translate("INCREASE_UNITS"));
            currentY += GroupMargin;
            UICheckBox doubleCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("DOUBLE_UNITS"));
            doubleCheck.isChecked = PathDeserialize.DoubleLimit;
            doubleCheck.eventCheckChanged += (c, isChecked) => { PathDeserialize.DoubleLimit = isChecked; };
            currentY += doubleCheck.height + Margin;

            // Add double units text.
            UILabel boost1 = UILabels.AddLabel(this, TitleMargin, currentY, Translations.Translate("EXPANDED_WARN"), titleWidth, 0.9f);
            currentY += boost1.height + TitleMargin;
            UILabel boost2 = UILabels.AddLabel(this, TitleMargin, currentY, Translations.Translate("EXPANDED_ALWAYS"), titleWidth, 0.9f);
            currentY += boost2.height + GroupMargin;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(this, TitleMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
        }
    }
}