using Dalamud.Interface.Windowing;

namespace BetterShadows;

internal static class Globals {
    public static Configuration Config { get; set; } = null!;
    public static DtrDisplay DtrDisplay { get; set; } = null!;
    public static bool ReapplyPreset = false;

    public static void ToggleHacks() {
        if (Config.EnabledOverall) {
            if (Config.Enabled) {
                CodeManager.EnableShadowCascadeOverride();
                return;
            }
        }

        CodeManager.DisableShadowCascadeOverride();
    }

    public static unsafe void ToggleShadowmap() {
        if (Config.EnabledOverall) {
            var option = CodeManager.ShadowManager->ShadowmapOption;
            var setting = Config.ShadowmapSettings[option];
            if (setting != ShadowmapResolution.RES_NONE) {
                CodeManager.EnableShadowmapOverride();
            }
        }
        else {
            CodeManager.DisableShadowmapOverride();
        }
    }
}
