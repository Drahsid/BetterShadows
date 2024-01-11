using ImGuiNET;
using System;
using System.Numerics;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace BetterShadows;

public class ConfigWindow : WindowWrapper {
    public static string ConfigWindowName = "Better Shadows Config";
    private static Vector2 MinSize = new Vector2(500, 240);

    private PresetEditorWindow PresetEditorWnd;
    private PresetListWindow PresetListWnd;
    private PresetZoneListWindow PresetZoneWnd;

    public ConfigWindow() : base(ConfigWindowName, MinSize) {
        PresetEditorWnd = new PresetEditorWindow();
        PresetListWnd = new PresetListWindow();
        PresetZoneWnd = new PresetZoneListWindow();

        Windows.System.AddWindow(PresetEditorWnd);
        Windows.System.AddWindow(PresetListWnd);
        Windows.System.AddWindow(PresetZoneWnd);
    }

    public override void Dispose() {
        Windows.System.RemoveWindow(PresetEditorWnd);
        Windows.System.RemoveWindow(PresetListWnd);
        Windows.System.RemoveWindow(PresetZoneWnd);
        base.Dispose();
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

    public override unsafe void Draw() {
        bool set_override = false;

        /*var _rtm = RenderTargetManager.Instance();
        RenderTargetManagerUpdated* rtm = (RenderTargetManagerUpdated*)_rtm;
        ImGui.Text($"{(IntPtr)(&rtm->ShadowMap_Width):X}");
        ImGui.Text($"Shadowmap: {rtm->ShadowMap_Width}, {rtm->ShadowMap_Height}");
        ImGui.Text($"Near Shadowmap: {rtm->NearShadowMap_Width}, {rtm->NearShadowMap_Height}");
        ImGui.Text($"Far Shadowmap: {rtm->FarShadowMap_Width}, {rtm->FarShadowMap_Height}");*/

        WindowDrawHelpers.DrawCheckboxTooltip("Hide tooltips",
            ref Globals.Config.HideTooltips,
            "Hide tooltips when hovering over settings.");

        WindowDrawHelpers.DrawCheckboxTooltip("Show Window in GPose",
            ref Globals.Config.OpenInGPose,
            "Open configuration window when you begin GPosing");

        ImGui.Separator();
        
        if (ImGui.BeginCombo("Shadowmap Res Override: High", Globals.Config.ShadowmapSettings[2].ToString())) {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++) {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowmapSettings[2] == rez)) {
                    Globals.Config.ShadowmapSettings[2] = rez;
                    if (CodeManager.ShadowmapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 2) {
                        CodeManager.DisableShadowmapOverride();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'High' shadow setting");

        if (ImGui.BeginCombo("Shadowmap Res Override: Normal", Globals.Config.ShadowmapSettings[1].ToString())) {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++) {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowmapSettings[1] == rez)) {
                    Globals.Config.ShadowmapSettings[1] = rez;
                    if (CodeManager.ShadowmapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 1) {
                        CodeManager.DisableShadowmapOverride();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Normal' shadow setting");

        if (ImGui.BeginCombo("Shadowmap Res Override: Low", Globals.Config.ShadowmapSettings[0].ToString())) {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++) {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowmapSettings[0] == rez)) {
                    Globals.Config.ShadowmapSettings[0] = rez;
                    if (CodeManager.ShadowmapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 0) {
                        CodeManager.DisableShadowmapOverride();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Low' shadow setting");

        ImGui.Separator();

        if (Globals.Config.Enabled) {
            ImGui.SameLine();
            WindowDrawHelpers.DrawCheckboxTooltip(
                "Show Continent",
                ref Globals.Config.ShowContinent,
                "Show the Continent in the zone list");

            WindowDrawHelpers.DrawCheckboxTooltip(
                "Show Zone Preset Config before Presets",
                ref Globals.Config.ZoneConfigBeforePreset,
                "When enabled, this reorders the config options below to show the Zone Preset Config first.");

            WindowDrawHelpers.DrawCheckboxTooltip(
                "Edit Override",
                ref Globals.Config.EditOverride,
                "When enabled, ignores the Zone Preset Config, and uses the values that are currently in the preset editor. When making changes to a preset, this is automatically enabled.");

            ImGui.Text("Popout:");
            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip("Preset Editor", "Popout the preset editor.")) {
                TogglePresetEditorPopout();
            }

            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip("Preset List", "Popout the preset list.")) {
                TogglePresetListPopout();
            }

            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip("Zone List", "Popout the zone list.")) {
                TogglePresetZonePopout();
            }

            if (WindowDrawHelpers.DrawButtonTooltip("Copy Presets and Zone Config", "Copy your entire configuration for sharing.")) {
                ImGui.SetClipboardText(Globals.Config.shared.ToBase64());
            }

            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip("Paste Presets and Zone Config", "Paste a shared configuration. This will destroy your existing config.")) {
                Globals.Config.shared = SharableData.FromBase64(ImGui.GetClipboardText());
            }

            ImGui.SameLine();
            if (ImGui.Button("Recover Default Presets")) {
                Globals.Config.RecoverStockPresets();
            }

            if (WindowDrawHelpers.DrawCheckboxTooltip(
                "Enable Custom Cascade Values",
                ref Globals.Config.Enabled,
                "Enable or disable the usage of custom shadow cascade values. When this is disabled, the Zone Preset Config section is hidden, since it would be unused.")) {
                Globals.ToggleHacks();
            }


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
}
