// <copyright file="PathDeserialize.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace MorePathUnits
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to handle deserialization of game PathUnit data.
    /// </summary>
    [HarmonyPatch(typeof(PathManager.Data), nameof(PathManager.Data.Deserialize))]
    public static class PathDeserialize
    {
        /// <summary>
        /// Expanded PathUnit count.
        /// </summary>
        internal const int NewUnitCount = ExtraUnitCount + OriginalUnitCount;

        // Private constants.
        private const int OriginalUnitCount = 262144;
        private const int ExtraUnitCount = OriginalUnitCount;

        // Status flag - are we loading an expanded PathUnit array?
        private static bool s_loadingExpanded = false;

        /// <summary>
        /// Gets the correct size to deserialize a saved game array.
        /// </summary>
        public static int DeserializeSize => s_loadingExpanded ? NewUnitCount : OriginalUnitCount;

        /// <summary>
        /// Gets or sets a value indicating whether PathUnit limits should be automatically doubled on virgin savegames.
        /// </summary>
        internal static bool DoubleLimit { get; set; } = true;

        /// <summary>
        /// Harmony Transpiler for PathManager.Data.Deserialize to increase the size of the PathUnit array at deserialization.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Status flag.
            bool inserted = false;

            Logging.Message("starting PathManager.Data.Deserialize transpiler");

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get current instruction.
                instruction = instructionsEnumerator.Current;

                // If we haven't already inserted, look for our start indicator (first ldloc 1 in code).
                if (!inserted)
                {
                    // Is this ldloc.1?
                    if (instruction.opcode == OpCodes.Ldloc_1)
                    {
                        Logging.Message("dropping from Ldloc_1");

                        // Yes - set flag.
                        inserted = true;

                        // Insert new instruction, calling DeserializeSize to determine correct buffer size to deserialize.
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(PathDeserialize), nameof(DeserializeSize)).GetGetMethod());

                        // Iterate forward, dropping all instructions until we reach our target (next stloc.2), then continue on as normal.
                        do
                        {
                            instructionsEnumerator.MoveNext();
                            instruction = instructionsEnumerator.Current;
                        }
                        while (!(instruction.opcode == OpCodes.Stloc_2));
                        Logging.Message("resuming from Stloc_2");
                    }
                }

                // Add current instruction to output.
                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony Prefix patch for PathManager.Data.Deserialize to determine if this mod was active when the game was saved.
        /// Highest priority, to try and make sure array setup is done before any other mod tries to read the array.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            Logging.Message("starting PathManager.Data.Deserialize Prefix");

            // If we're expanding from vanilla saved data, ensure the PathUnit array is clear to start with.
            // Are we loading expanded data, or expanding from vanilla size?
            s_loadingExpanded = MetaData.LoadingExtended;
            if (s_loadingExpanded || DoubleLimit)
            {
                Logging.KeyMessage("expanding PathUnit buffer size to ", NewUnitCount);
                Array32<PathUnit> expandedArray = new Array32<PathUnit>(NewUnitCount);

                // Ensure expanded array is clear if we're not going to be overwriting it with expanded savegame data.
                if (!s_loadingExpanded)
                {
                    Array.Clear(expandedArray.m_buffer, 0, expandedArray.m_buffer.Length);
                }

                // Set expanded array (using Singleton, not immediate instance; otherwise can end up with wrong instance if e.g. TM:PE custom manager is in effect).
                Singleton<PathManager>.instance.m_pathUnits = expandedArray;

                // Update TM:PE custom path manager.
                Type tmpePathManagerType = Type.GetType("TrafficManager.Custom.PathFinding.CustomPathManager,TrafficManager", false);
                if (tmpePathManagerType != null)
                {
                    Logging.KeyMessage("found TM:PE CustomPathManager");
                    object tmpePathManagerInstance = AccessTools.Field(tmpePathManagerType, "_instance")?.GetValue(null);

                    if (tmpePathManagerInstance == null)
                    {
                        Logging.KeyMessage("TM:PE CustomPathManager instance was null; hopefully not an error");
                    }
                    else
                    {
                        Logging.Message("re-initializing TM:PE CustomPathManager");
                        AccessTools.Method(tmpePathManagerType, "Reinitialize").Invoke(tmpePathManagerInstance, new object[] { expandedArray });
                    }
                }

                // Update vanilla pathfinder.
                PathManager pathManager = Singleton<PathManager>.instance;
                FieldInfo pathFindUnits = AccessTools.Field(typeof(PathFind), "m_pathUnits");
                if (AccessTools.Field(typeof(PathManager), "m_pathfinds")?.GetValue(pathManager) is PathFind[] pathFinds && pathFindUnits != null)
                {
                    // Update PathUnit array reference for all created PathFinds.
                    for (int i = 0; i < pathFinds.Length; ++i)
                    {
                        Logging.Message("setting PathFind unit array reference for vanilla pathfinding thread ", i);
                        pathFindUnits.SetValue(pathFinds[i], expandedArray);
                    }
                }
                else
                {
                    Logging.Error("couldn't find vanilla PathFinds");
                }
            }
            else
            {
                Logging.Message("preserving Vanilla buffer size");
            }

            Logging.Message("finished PathManager.Data.Deserialize Prefix");
        }

        /// <summary>
        /// Harmony Postfix patch for PathManager.Data.Deserialize to ensure proper unused item allocation and count after conversion from vanilla save data.
        /// Highest priority, to try and make sure array setup is done before any other mod tries to read the array.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            // Local references.
            Array32<PathUnit> unitArray = Singleton<PathManager>.instance.m_pathUnits;
            PathUnit[] unitBuffer = unitArray.m_buffer;

            Logging.Message("starting PathManager.Data.Deserialize Postfix");

            // Only need to do this if converting from vanilla saved data.
            if (!s_loadingExpanded)
            {
                // Clear unused elements array and list, and establish a debugging counter.
                Logging.Message("resetting unused instances");
                unitArray.ClearUnused();
                uint freedUnits = 0;

                // Iterate through each unit in buffer.
                for (uint i = 0; i < unitBuffer.Length; ++i)
                {
                    // Check if this unit is valid.
                    if ((unitBuffer[i].m_simulationFlags & PathUnit.FLAG_CREATED) == 0)
                    {
                        // Invalid unit - properly release it to ensure m_units array's internals are correctly set.
                        unitArray.ReleaseItem(i);

                        // Increment debugging message counter.
                        ++freedUnits;
                    }
                }

                Logging.Message("completed resetting unused instances; freed unit count was ", freedUnits);
            }

            Logging.Message("finished PathManager.Data.Deserialize Postfix");
        }
    }
}