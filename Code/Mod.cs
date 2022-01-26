using ICities;
using CitiesHarmony.API;


namespace MorePathUnits
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class MPUMod : IUserMod
    {
        public static string ModName => "More PathUnits";
        public static string Version => "0.2";

        public string Name => ModName + " " + Version;
        public string Description => Translations.Translate("MPU_DESC");


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }
    }
}
