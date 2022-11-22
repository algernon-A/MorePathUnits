// <copyright file="PathDeserialize.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>


namespace MorePathUnits
{
    using System;
    using System.Collections.Generic;
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
        // Constants.
        private const int OriginalUnitCount = 262144;
        private const int ExtraUnitCount = OriginalUnitCount;
        internal const int NewUnitCount = ExtraUnitCount + OriginalUnitCount;

        // Status flag - are we loading an expanded PathUnit array?
        private static bool loadingExpanded = false;

        /// <summary>
        /// Harmony Transpilier for PathManager.Data.Deserialize to increase the size of the PathUnit array at deserialization.
        /// </summary>
        /// <param name="instructions">Original ILCode instructions</param>
        /// <returns></returns>
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
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(PathDeserialize), nameof(PathDeserialize.DeserialiseSize)).GetGetMethod());

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

            // Check to see if PathUnit array has been correctly resized.
            Array32<PathUnit> units = Singleton<PathManager>.instance.m_pathUnits;
            if (units.m_buffer.Length == NewUnitCount)
            {
                // Detect if we're loading an expanded or original PathUnit array.
                loadingExpanded = MetaData.LoadingExtended;

                // If we're expanding from vanilla saved data, ensure the PathUnit array is clear to start with.
                if (!loadingExpanded)
                {
                    Logging.Message("expanding from Vanilla save data");
                    Array.Clear(units.m_buffer, 0, units.m_buffer.Length);
                }
            }
            else
            {
                // Buffer wasn't extended.
                Logging.Error("PathUnit buffer not extended");
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
            if (!loadingExpanded)
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

        /// <summary>
        /// Returns the correct size to deserialize a saved game array.
        /// </summary>
        public static int DeserialiseSize => loadingExpanded ? NewUnitCount : OriginalUnitCount;
    }
}