// <copyright file="ModLimitTranspiler.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony transpilers to replace hardcoded CitizenUnit limits in mods.
    /// </summary>
    internal static class ModLimitTranspiler
    {
        /// <summary>
        /// Determines list of target methods to patch - in this case, identified mods and methods with hardcoded CitizenUnit limits.
        /// </summary>
        /// <returns>List of target methods to patch</returns>
        internal static void PatchMods(Harmony harmony)
        {
            if (harmony == null)
            {
                Logging.Error("null harmony instance passed to PatchMods");
                return;
            }

            // TM:PE.
            Assembly tmpe = AssemblyUtils.GetEnabledAssembly("TrafficManager");
            if (tmpe != null)
            {
                Logging.Message("reflecting TM:PE");
                PatchModMethod(harmony, tmpe.GetType("TrafficManager.Custom.PathFinding.CustomPathFind"), "Awake");
                PatchModMethod(harmony, tmpe.GetType("TrafficManager.Custom.PathFinding.CustomPathFind"), "PathFindImplementation");
                PatchModMethod(harmony, tmpe.GetType("TrafficManager.Custom.PathFinding.CustomPathManager"), "CustomReleasePath");
                PatchModMethod(harmony, tmpe.GetType("TrafficManager.Custom.PathFinding.CustomPathManager"), "UpdateWithPathManagerValues");
            }
        }

        /// <summary>
        /// Attempts to transpile hardcoded CitizenUnit limits in the given method from the given type. 
        /// </summary>
        /// <param name="harmony">Harmony instance.</param>
        /// <param name="type">Type to reflect.</param>
        /// <param name="methodName">Method name to reflect.</param>
        private static void PatchModMethod(Harmony harmony, Type type, string methodName)
        {
            // Check that reflection succeeded before proceeding,
            if (type == null)
            {
                Logging.Error("null type when attempting to patch ", methodName ?? "null");
                return;
            }

            MethodInfo method = AccessTools.Method(type, methodName);

            // Report error and return false if reflection failed.
            if (method == null)
            {
                Logging.Error("unable to reflect ", methodName);
                return;
            }

            // If we got here, all good; apply transpiler.
            Logging.Message("transpiling ", type, ":", methodName);
            harmony.Patch(method, transpiler: new HarmonyMethod(typeof(GameLimitTranspiler), nameof(GameLimitTranspiler.Transpiler)));
        }
    }
}