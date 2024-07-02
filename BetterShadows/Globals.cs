using System.Collections.Generic;
using System;
using System.Linq;

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
            var setting = Config.ShadowMapGlobalSettings[option];
            if (setting != ShadowmapResolution.RES_NONE) {
                CodeManager.EnableShadowMapOverride();
            }
        }
        else {
            CodeManager.DisableShadowMapOverride();
        }
    }

    internal static Dictionary<string, ConfigTreeNode> SortConfigDictionaryAndChildren(Dictionary<string, ConfigTreeNode> dictionary)
    {
        Dictionary<string, ConfigTreeNode> result = dictionary.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var entry in result.Values)
        {
            if (entry.Children != null)
            {
                entry.Children = SortConfigDictionaryAndChildren(entry.Children);
            }
        }

        return result;
    }
}
