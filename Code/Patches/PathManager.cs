using HarmonyLib;


namespace MorePathUnits
{
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
            Patcher.PatchMods();

            Logging.Message("mod patching complete");
        }
    }
}