// <copyright file="PathManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patch to transpile TM:PE.
    /// </summary>
    [HarmonyPatch(typeof(PathManager), "Awake")]
    public static class PathManagerPath
    {
        /// <summary>
        /// Harmony Prefix for PathManager.Awake to transpile TM:PE at just the right time in the loading sequence for this to work (after loading has started but before TM:PE's CustomPathManager.Awake() is invoked).
        /// </summary>
        public static void Prefix()
        {
            Logging.Message("patching mods");

            // Patch mods.
            PatcherManager<Patcher>.Instance.PatchMods();

            Logging.Message("mod patching complete");
        }
    }
}