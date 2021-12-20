using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


namespace MorePathUnits
{
    /// <summary>
    /// Harmony transpilers to replace hardcoded PathUnit limits in the game.
    /// </summary>
    [HarmonyPatch]
    public static class GameLimitTranspiler
    {
        /// <summary>
        /// Determines list of target methods to patch - in this case, identified methods with hardcoded PathUnit limits.
        /// This includes PathFind.Awake and PathManager.Awake where the PathFind arrays are created; overriding the values here automatically creates arrays of the correct new size.
        /// </summary>
        /// <returns>List of target methods to patch</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(PathFind), "Awake");
            yield return AccessTools.Method(typeof(PathFind), "PathFindImplementation");
            yield return AccessTools.Method(typeof(PathManager), "Awake");
            yield return AccessTools.Method(typeof(PathManager), nameof(PathManager.ReleasePath));
        }


        /// <summary>
        /// Harmony transpiler to replace hardcoded PathUnit limits.
        /// Finds ldc.i4 262144 (which is unique in game code to the PathUnit limit checks) and replaces the operand with our updated maximum.
        /// </summary>
        /// <param name="original">Original (target) method</param>
        /// <param name="instructions">Original ILCode</param>
        /// <returns>Patched ILCode</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Status flag.
            bool foundTarget = false;

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                instruction = instructionsEnumerator.Current;

                // Is this ldc.i4 262144?
                if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int thisInt && thisInt == 262144)
                {
                    // Yes - change operand to our new unit count max.
                    instruction.operand = PathDeserialize.NewUnitCount;

                    Logging.Message("changed 262144 in ", original.FullDescription(), " to ", PathDeserialize.NewUnitCount);

                    // Set flag.
                    foundTarget = true;
                }

                // Output instruction.
                yield return instruction;
            }

            // If we got here without finding our target, something went wrong.
            if (!foundTarget)
            {
                Logging.Error("no ldc.i4 262144 found for ", original);
            }
        }
    }
}