// <copyright file="OnLevelLoaded.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patch for OnLevelLoaded.  This enables us to perform setup tasks after all loading has been completed.
    /// </summary>
    [HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLevelLoaded")]
    public static class OnLevelLoadedPatch
    {
        /// <summary>
        /// Harmony Postfix to perform actions required after the level has loaded.
        /// </summary>
        public static void Postfix()
        {
            // Get buffer size.
            Array32<PathUnit> units = Singleton<PathManager>.instance.m_pathUnits;
            int bufferSize = units.m_buffer.Length;
            Logging.Message("current PathUnits array size is ", bufferSize.ToString("N0"), " with m_size ", units.m_size.ToString("N0"));

            // Check for successful implementation.
            if (bufferSize == PathDeserialize.NewUnitCount)
            {
                // Buffer successfully enlarged - set simulation metatdata flag.
                MetaData.SetMetaData();
                Logging.KeyMessage("loading complete");
            }
            else
            {
                // Buffer size not changed - log error and undo Harmony patches.
                Logging.Error("PathUnits array size not increased; aborting operation and reverting Harmony patches");
                PatcherManager<Patcher>.Instance.UnpatchAll();
            }
        }
    }
}