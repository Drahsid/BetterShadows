using DrahsidLib;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;

public class PresetZoneListWindow : WindowWrapper {
    public static string ConfigWindowName = "Better Shadows Zone Config";
    private static Vector2 MinSize = new Vector2(320, 240);

    public PresetZoneListWindow() : base(ConfigWindowName, MinSize) {
    }

    public override void Draw() {
        bool set_override = false;
        ConfigWindowHelpers.DrawZonePresetList(ImGui.GetContentRegionAvail());

        if (set_override) {
            Globals.Config.EditOverride = set_override;
        }
    }
}
