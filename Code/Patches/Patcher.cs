// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using AlgernonCommons.Patching;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public sealed class Patcher : PatcherBase
    {
        /// <summary>
        ///  Apply Harmony patches to mods.
        /// </summary>
        public void PatchMods() => ModLimitTranspiler.PatchMods(new Harmony(HarmonyID));
    }
}