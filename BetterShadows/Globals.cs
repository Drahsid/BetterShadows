using Dalamud.Interface.Windowing;

namespace BetterShadows;

internal static class Globals {
    public static Configuration Config { get; set; } = null!;
    public static DtrDisplay DtrDisplay { get; set; } = null!;
    public static bool ReapplyPreset = false;

    public static void ToggleHacks() {
        if (Config.Enabled && Config.EnabledOverall) {
            CodeManager.DoEnableHacks();
        }
        else {
            CodeManager.DoDisableHacks();
        }
    }

    public static void ToggleShadowmap() {
        if (Config.HigherResShadowmap && Config.EnabledOverall) {
            CodeManager.DoEnableShadowmap();
        }
        else {
            CodeManager.DoDisableShadowmap();
        }
    }
}
