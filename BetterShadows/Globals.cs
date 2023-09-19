using Dalamud.Interface.Windowing;

namespace BetterShadows;

internal class Globals
{
    public static Configuration Config;
    public static DtrDisplay DtrDisplay;
    public static WindowSystem WindowSystem;
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
