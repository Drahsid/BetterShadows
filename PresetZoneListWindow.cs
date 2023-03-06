using Dalamud.Interface;
using Dalamud.Interface.Animation.EasingFunctions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;
public class PresetZoneListWindow : Window, IDisposable {
    public static string ConfigWindowName = "Better Shadows Zone Config";
    private Vector2 MinSize = new Vector2(320, 240);
    private Vector2 AdjustedMinSize = new Vector2(320, 240);

    public PresetZoneListWindow(Plugin plugin) : base(ConfigWindowName) {
    }

    public override void PreDraw() {
        AdjustedMinSize = MinSize * ImGuiHelpers.GlobalScale;
        ImGui.SetNextWindowSizeConstraints(AdjustedMinSize, new Vector2(float.MaxValue, float.MaxValue));
    }

    public override void Draw() {
        bool set_override = false;
        ConfigWindowHelpers.DrawZonePresetList(ImGui.GetContentRegionAvail());

        if (set_override) {
            Globals.Config.EditOverride = set_override;
        }
    }

    public void Dispose() { }
}
