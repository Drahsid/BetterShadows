using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;
public class PresetListWindow : Window, IDisposable {
    public static string ConfigWindowName = "Better Shadows Preset List";
    private Vector2 MinSize = new Vector2(320, 240);
    private Vector2 AdjustedMinSize = new Vector2(320, 240);

    public PresetListWindow(Plugin plugin) : base(ConfigWindowName) {
    }

    public override void PreDraw() {
        AdjustedMinSize = MinSize * ImGuiHelpers.GlobalScale;
        ImGui.SetNextWindowSizeConstraints(AdjustedMinSize, new Vector2(float.MaxValue, float.MaxValue));
    }

    public override void Draw() {
        bool set_override = false;
        ConfigWindowHelpers.DrawPresetSelector(ImGui.GetContentRegionAvail(), ref set_override);

        if (set_override) {
            Globals.Config.EditOverride = set_override;
        }
    }

    public void Dispose() { }
}
