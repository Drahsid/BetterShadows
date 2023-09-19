using DrahsidLib;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;

public class PresetEditorWindow : WindowWrapper {
    public static string ConfigWindowName = "Better Shadows Preset Editor";
    private static Vector2 MinSize = new Vector2(320, 240);

    public PresetEditorWindow() : base(ConfigWindowName, MinSize) {
    }

    public override void Draw() {
        bool set_override = false;
        ConfigWindowHelpers.DrawPresetEditor(ref set_override);

        if (set_override) {
            Globals.Config.EditOverride = set_override;
        }
    }
}
