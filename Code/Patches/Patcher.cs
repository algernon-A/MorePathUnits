// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public sealed class Patcher : PatcherBase
    {
        /// <summary>
        /// Updates TM:PE's internal data.
        /// </summary>
        public void UpdateTMPE()
        {
            // Apply transpiler to TM:PE internal hardcoded limits.
            ModLimitTranspiler.PatchMods(new Harmony(HarmonyID));

            // Via simulation thread.
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                Logging.KeyMessage("checking for TM:PE CustomPathManager");

                Type tmpePMtype = Type.GetType("TrafficManager.Custom.PathFinding.CustomPathManager,TrafficManager", false);
                if (tmpePMtype != null)
                {
                    Logging.Message("found TM:PE CustomPathManager type");

                    // If there's an active TM:PE CustomPathManager instance, invoke the UpdateWithPathManagerValues method to reset TM:PE's internal values.
                    FieldInfo tmpePMinstanceField = AccessTools.Field(tmpePMtype, "_instance");
                    if (tmpePMinstanceField != null && tmpePMinstanceField.GetValue(null) is object tmpePathManager)
                    {
                        Logging.KeyMessage("setting TM:PE CustomPathManager values");
                        AccessTools.Method(tmpePMtype, "UpdateWithPathManagerValues").Invoke(tmpePathManager, new object[] { Singleton<PathManager>.instance });
                    }
                }
            });
        }
    }
}