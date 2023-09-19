using DrahsidLib;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;

public class PresetListWindow : WindowWrapper {
    public static string ConfigWindowName = "Better Shadows Preset List";
    private static Vector2 MinSize = new Vector2(320, 240);

    public PresetListWindow() : base(ConfigWindowName, MinSize) {
    }

    public override void Draw() {
        bool set_override = false;
        ConfigWindowHelpers.DrawPresetSelector(ImGui.GetContentRegionAvail(), ref set_override);

        if (set_override) {
            Globals.Config.EditOverride = set_override;
        }
    }
}
