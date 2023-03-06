using Dalamud.Interface.Animation.EasingFunctions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace BetterShadows;

public class ConfigWindow : Window, IDisposable
{
    public static string ConfigWindowName = "Better Shadows Config";

    private Vector2 MinSize = new Vector2(500, 240);
    private Vector2 AdjustedMinSize = new Vector2(500, 240);

    private PresetEditorWindow PresetEditorWnd;
    private PresetListWindow PresetListWnd;
    private PresetZoneListWindow PresetZoneWnd;

    public ConfigWindow(Plugin plugin) : base(ConfigWindowName) {
        PresetEditorWnd = new PresetEditorWindow(plugin);
        PresetListWnd = new PresetListWindow(plugin);
        PresetZoneWnd = new PresetZoneListWindow(plugin);

        Globals.WindowSystem.AddWindow(PresetEditorWnd);
        Globals.WindowSystem.AddWindow(PresetListWnd);
        Globals.WindowSystem.AddWindow(PresetZoneWnd);
    }

    public void TogglePresetEditorPopout() {
        PresetEditorWnd.IsOpen = !PresetEditorWnd.IsOpen;
    }

    public void TogglePresetListPopout() {
        PresetListWnd.IsOpen = !PresetListWnd.IsOpen;
    }

    public void TogglePresetZonePopout() {
        PresetZoneWnd.IsOpen = !PresetZoneWnd.IsOpen;
    }

    public override void PreDraw() {
        AdjustedMinSize = MinSize * ImGuiHelpers.GlobalScale;
        ImGui.SetNextWindowSizeConstraints(AdjustedMinSize, new Vector2(float.MaxValue, float.MaxValue));
    }

    public override void Draw()
    {
        bool set_override = false;

        if (ImGui.Button("Copy Presets and Zone Config")) {
            ImGui.SetClipboardText(Globals.Config.shared.ToBase64());
        }
        ConfigWindowHelpers.DrawTooltip("Copy your entire configuration for sharing.");

        ImGui.SameLine();
        if (ImGui.Button("Paste Presets and Zone Config")) {
            Globals.Config.shared = SharableData.FromBase64(ImGui.GetClipboardText());
        }
        ConfigWindowHelpers.DrawTooltip("Paste a shared configuration. This will destroy your existing config.");

        if (ImGui.Checkbox("Enable Custom Cascade Values", ref Globals.Config.Enabled))
        {
            Globals.ToggleHacks();
        }
        ConfigWindowHelpers.DrawTooltip("Enable or disable the usage of custom shadow cascade values. When this is disabled, the Zone Preset Config section is hidden, since it would be unused.");

        if (ImGui.Checkbox("2048p = 4096p shadowmap", ref Globals.Config.HigherResShadowmap))
        {
            Globals.ToggleShadowmap();
        }
        ConfigWindowHelpers.DrawTooltip("Enable or disable using a 4096p shadowmap when you have the 2048p shadowmap setting. This doubles the resolution of the shadowmap when enabled, making shadows look clearer.");

        ImGui.Checkbox("Hide tooltips", ref Globals.Config.HideTooltips);
        ConfigWindowHelpers.DrawTooltip("Hide tooltips when hovering over settings.");

        if (Globals.Config.Enabled) {
            ImGui.SameLine();
            ImGui.Checkbox("Show Continent", ref Globals.Config.ShowContinent);
            ConfigWindowHelpers.DrawTooltip("Show the Continent in the zone list");

            ImGui.Checkbox("Show Zone Preset Config before Presets", ref Globals.Config.ZoneConfigBeforePreset);
            ConfigWindowHelpers.DrawTooltip("When enabled, this reorders the config options below to show the Zone Preset Config first.");

            ImGui.Checkbox("Edit Override", ref Globals.Config.EditOverride);
            ConfigWindowHelpers.DrawTooltip("When enabled, ignores the Zone Preset Config, and uses the values that are currently in the preset editor. When making changes to a preset, this is automatically enabled.");

            ImGui.Text("Popout: ");
            ImGui.SameLine();
            if (ImGui.Button("Preset Editor")) {
                TogglePresetEditorPopout();
            }
            ConfigWindowHelpers.DrawTooltip("Popout the preset editor.");

            ImGui.SameLine();
            if (ImGui.Button("Preset List")) {
                TogglePresetListPopout();
            }
            ConfigWindowHelpers.DrawTooltip("Popout the preset list.");

            ImGui.SameLine();
            if (ImGui.Button("Zone List")) {
                TogglePresetZonePopout();
            }
            ConfigWindowHelpers.DrawTooltip("Popout the zone list.");

            string preview = "";
            foreach (CascadeConfig config in Globals.Config.shared.cascadePresets) {
                if (Globals.Config.shared.defaultPreset == config.GUID) {
                    preview = config.Name;
                    break;
                }
            }

            if (ImGui.BeginCombo("Default Preset", preview)) {
                foreach (CascadeConfig config in Globals.Config.shared.cascadePresets) {
                    if (ImGui.Selectable(config.Name, Globals.Config.shared.defaultPreset == config.GUID)) {
                        Globals.Config.shared.defaultPreset = config.GUID;
                        Globals.ReapplyPreset = true;
                    }
                }
                ImGui.EndCombo();
            }
            ConfigWindowHelpers.DrawTooltip("The default preset to use when a zone config has no parent.");
        }

        ImGui.Separator();

        if (Globals.Config.Enabled) {
            const float offset = 32.0f;
            const float thick = 4.0f;

            ConfigWindowHelpers.DrawPresetEditor(ref set_override);

            // safe scroll line
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 regionMax = ImGui.GetContentRegionMax();
            Vector2 regionAvail = ImGui.GetContentRegionAvail();
            float pos_X = regionMax.X - (offset - (thick * 0.5f));
            float pos_Y = ImGui.GetCursorPosY();
            pos_X += windowPos.X;
            pos_Y += windowPos.Y;
            ImGui.GetWindowDrawList().AddLine(new Vector2(pos_X, pos_Y - ImGui.GetScrollY()), new Vector2(pos_X, pos_Y + regionAvail.Y), ImGui.GetColorU32(ImGuiCol.Separator), thick);

            regionAvail.X -= offset; // add space to scroll the base window
            regionAvail.Y /= 2.0f;

            if (Globals.Config.ZoneConfigBeforePreset) {
                ConfigWindowHelpers.DrawZonePresetList(regionAvail);
                ImGui.SetNextItemWidth(regionAvail.X);
                regionAvail = ImGui.GetContentRegionAvail();
                ConfigWindowHelpers.DrawPresetSelector(regionAvail, ref set_override);
            }
            else {
                ConfigWindowHelpers.DrawPresetSelector(regionAvail, ref set_override);
                ImGui.SetNextItemWidth(regionAvail.X);
                regionAvail = ImGui.GetContentRegionAvail();
                ConfigWindowHelpers.DrawZonePresetList(regionAvail);
            }

            if (set_override) {
                Globals.Config.EditOverride = set_override;
            }
        }
    }

    public void Dispose() {
        Globals.WindowSystem.RemoveWindow(PresetEditorWnd);
        Globals.WindowSystem.RemoveWindow(PresetListWnd);
        Globals.WindowSystem.RemoveWindow(PresetZoneWnd);
    }
}
