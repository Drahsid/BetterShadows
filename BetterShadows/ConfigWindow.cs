using ImGuiNET;
using System.Numerics;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System;

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

    private void DrawCascadeToggleCheckbox() {
        if (WindowDrawHelpers.DrawCheckboxTooltip(
                "Enable Custom Cascade Values",
                ref Globals.Config.Enabled,
                "Enable or disable the usage of custom shadow cascade values. When this is disabled, the Zone Preset Config section is hidden, since it would be unused."))
        {
            Globals.ToggleHacks();
        }

        if (WindowDrawHelpers.DrawCheckboxTooltip("Use Dynamic Cascade Values",
            ref Globals.Config.DynamicCascadeMode,
            "Dynamically adjust the cascade values based on your shadowmap settings. If this is off, the plugin will use the per-territory settings")) {
            //
        }
    }

    private unsafe void DrawGlobalShadowmapSetting()
    {
        if (ImGui.BeginCombo("Shadow Map Override: High", Globals.Config.ShadowMapGlobalSettings[2].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapGlobalSettings[2] == rez))
                {
                    Globals.Config.ShadowMapGlobalSettings[2] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 2)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'High' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Normal", Globals.Config.ShadowMapGlobalSettings[1].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapGlobalSettings[1] == rez))
                {
                    Globals.Config.ShadowMapGlobalSettings[1] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 1)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Normal' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Low", Globals.Config.ShadowMapGlobalSettings[0].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapGlobalSettings[0] == rez))
                {
                    Globals.Config.ShadowMapGlobalSettings[0] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 0)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Low' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Combat Override", Globals.Config.ShadowMapCombatOverride.ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapCombatOverride == rez))
                {
                    Globals.Config.ShadowMapCombatOverride = rez;
                    if (CodeManager.ShadowMapOverrideEnabled)
                    {
                        if (rez == ShadowmapResolution.RES_NONE)
                        {
                            CombatShadowmap.Dispose();
                        }
                        else
                        {
                            CombatShadowmap.SetCombatTextureSize(rez);
                        }
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of global shadows while in combat. Settings other than NONE have a slight vram cost, with potentially beneficial performance during combat.");
    }

    private unsafe void DrawNearShadowmapSetting()
    {
        if (ImGui.BeginCombo("Shadow Map Override: High", Globals.Config.ShadowMapNearSettings[2].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapNearSettings[2] == rez))
                {
                    Globals.Config.ShadowMapNearSettings[2] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 2)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'High' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Normal", Globals.Config.ShadowMapNearSettings[1].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapNearSettings[1] == rez))
                {
                    Globals.Config.ShadowMapNearSettings[1] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 1)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Normal' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Low", Globals.Config.ShadowMapNearSettings[0].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapNearSettings[0] == rez))
                {
                    Globals.Config.ShadowMapNearSettings[0] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 0)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Low' shadow resolution setting");
    }

    private unsafe void DrawFarShadowmapSetting()
    {
        if (ImGui.BeginCombo("Shadow Map Override: High", Globals.Config.ShadowMapFarSettings[2].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapFarSettings[2] == rez))
                {
                    Globals.Config.ShadowMapFarSettings[2] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 2)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'High' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Normal", Globals.Config.ShadowMapFarSettings[1].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapFarSettings[1] == rez))
                {
                    Globals.Config.ShadowMapFarSettings[1] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 1)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Normal' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Low", Globals.Config.ShadowMapFarSettings[0].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapFarSettings[0] == rez))
                {
                    Globals.Config.ShadowMapFarSettings[0] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 0)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Low' shadow resolution setting");
    }

    private unsafe void DrawDistanceShadowmapSetting()
    {
        if (ImGui.BeginCombo("Shadow Map Override: High", Globals.Config.ShadowMapDistanceSettings[2].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapDistanceSettings[2] == rez))
                {
                    Globals.Config.ShadowMapDistanceSettings[2] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 2)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'High' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Normal", Globals.Config.ShadowMapDistanceSettings[1].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapDistanceSettings[1] == rez))
                {
                    Globals.Config.ShadowMapDistanceSettings[1] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 1)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Normal' shadow resolution setting");

        if (ImGui.BeginCombo("Shadow Map Override: Low", Globals.Config.ShadowMapDistanceSettings[0].ToString()))
        {
            for (int index = 0; index < (int)ShadowmapResolution.RES_COUNT; index++)
            {
                ShadowmapResolution rez = (ShadowmapResolution)index;
                if (ImGui.Selectable(rez.ToString(), Globals.Config.ShadowMapDistanceSettings[0] == rez))
                {
                    Globals.Config.ShadowMapDistanceSettings[0] = rez;
                    if (CodeManager.ShadowMapOverrideEnabled && CodeManager.ShadowManager->ShadowmapOption == 0)
                    {
                        CodeManager.ReinitializeShadowMap();
                    }
                }
            }
            ImGui.EndCombo();
        }
        ConfigWindowHelpers.DrawTooltip("Override the resolution of the 'Low' shadow resolution setting");
    }

    public override unsafe void Draw() {
        bool set_override = false;

        WindowDrawHelpers.DrawCheckboxTooltip("Show Debug", ref Globals.Config.Debug, "Show advanced debug options and info.");
        if (Globals.Config.Debug)
        {
            if (ImGui.TreeNode("Debug")) {
                var _rtm = RenderTargetManager.Instance();
                RenderTargetManagerUpdated* rtm = (RenderTargetManagerUpdated*)_rtm;

                ImGui.TextDisabled("Setting these options to non-zero values will force that value for the related shadowmap resolution");

                ImGui.InputInt("Global Map Width", ref Globals.Config.ForceMapX);
                ImGui.InputInt("Global Map Height", ref Globals.Config.ForceMapY);

                ImGui.InputInt("Dynamic Near Map Width", ref Globals.Config.ForceNearMapX);
                ImGui.InputInt("Dynamic Near Map Height", ref Globals.Config.ForceNearMapY);

                ImGui.InputInt("Dynamic Far Map Width", ref Globals.Config.ForceFarMapX);
                ImGui.InputInt("Dynamic Far Map Height", ref Globals.Config.ForceFarMapY);

                ImGui.InputInt("Distance Map Width", ref Globals.Config.ForceDistanceMapX);
                ImGui.InputInt("Distance Map Height", ref Globals.Config.ForceDistanceMapY);

                ImGui.Text($"Global ShadowMap Dimensions: {rtm->ShadowMap_Width}, {rtm->ShadowMap_Height}");
                ImGui.Text($"Near Dynamic Light ShadowMap Dimensions: {rtm->NearShadowMap_Width}, {rtm->NearShadowMap_Height}");
                ImGui.Text($"Far Dynamic Light ShadowMap Dimensions: {rtm->FarShadowMap_Width}, {rtm->FarShadowMap_Height}");
                ImGui.Text($"Distance ShadowMap Dimensions: {rtm->DistanceShadowMap_Width}, {rtm->DistanceShadowMap_Height}");

                if (ImGui.Button("Reinit"))
                {
                    CodeManager.ReinitializeShadowMap();
                }

                if (Globals.Config.SuperDebug)
                {
                    var shadows = ShadowManager.Instance();
                    ImGui.Separator();
                    ImGui.Text($"{(IntPtr)(&rtm->Resolution_Width):X}");
                    ImGui.Text($"{(IntPtr)(shadows):X}");
                    ImGui.Text($"{(IntPtr)(&shadows->CascadeDistance0):X}");

                    ImGui.Text($"Shadow Cascades: {shadows->CascadeDistance0}, {shadows->CascadeDistance1}, {shadows->CascadeDistance2}, {shadows->CascadeDistance3}, {shadows->CascadeDistance4}");
                    ImGui.Text($"Shadow Biases: {shadows->Bias0}, {shadows->Bias1}, {shadows->Bias2}, {shadows->Bias3}");

                    ImGui.Text($"Near: {shadows->NearDistance}, Far: {shadows->FarDistance}");

                    ImGui.Text($"ShadowSofteningSetting: {shadows->ShadowSofteningSetting:X} -- {(IntPtr)(&shadows->ShadowSofteningSetting):X}");
                    ImGui.Text($"Unk_0x10: {shadows->Unk_0x10:X}");
                    ImGui.Text($"Unk_0x14: {shadows->Unk_0x14:X}");
                    ImGui.Text($"ShadowmapOption: {shadows->ShadowmapOption} -- {(IntPtr)(&shadows->ShadowmapOption):X}");
                    ImGui.Text($"ShadowCascadeCount0: {shadows->ShadowCascadeCount0:X}");
                    ImGui.Text($"ShadowCascadeCount1: {shadows->ShadowCascadeCount1}");
                    ImGui.Text($"Unk_0x24: {shadows->Unk_0x24:X}");
                    ImGui.Text($"Unk_0x28: {shadows->Unk_0x28}");
                    ImGui.Text($"Unk_0x2C: {shadows->Unk_0x2C}");
                    ImGui.Text($"Unk_0x1E1: {shadows->Unk_0x1E1}");
                    ImGui.Text($"Unk_0x1E8: {shadows->ShadowapBlending} -- {(IntPtr)(&shadows->ShadowapBlending):X}");
                }


                ImGui.TreePop();
            }
        }

        if (ImGui.Button("Save"))
        {
            Globals.Config.Save();
        }

        WindowDrawHelpers.DrawCheckboxTooltip("Hide tooltips",
            ref Globals.Config.HideTooltips,
            "Hide tooltips when hovering over settings.");

        WindowDrawHelpers.DrawCheckboxTooltip("Show Window in GPose",
            ref Globals.Config.OpenInGPose,
            "Open configuration window when you begin GPosing");

        ImGui.Separator();

        if (ImGui.TreeNode("Shadow Map Settings"))
        {
            if (WindowDrawHelpers.DrawCheckboxTooltip("Try to maintain shadow resolution aspect ratio", ref Globals.Config.MaintainGameAspect, "Tries to maintain the shadowmap resolution aspect ratios naturally seen in the game, as opposed to using a square shadowmap.\nThis will make lower resolution options look better since it may increase the shadowmap sizes,\nwhile having no effect on options higher than 2048 for Global shaodws, and 8192 for Near shadows."))
            {
                CodeManager.ReinitializeShadowMap();
                if (Globals.Config.ShadowMapCombatOverride != ShadowmapResolution.RES_NONE)
                {
                    CombatShadowmap.SetCombatTextureSize(Globals.Config.ShadowMapCombatOverride);
                }
            }

            if (ImGui.TreeNode("Global Sun Shadow Map Settings"))
            {
                DrawGlobalShadowmapSetting();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Dynamic Light (Near) Shadow Map Settings"))
            {
                DrawNearShadowmapSetting();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Dynamic Light (Far) Shadow Map Settings"))
            {
                ImGui.TextColored(new Vector4(1, 0.25f, 0.25f, 1), "Warning: Dynamic Light (Far) takes significantly more vram than the other shadow maps.");
                DrawFarShadowmapSetting();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Distance Shadow Map Settings"))
            {
                DrawDistanceShadowmapSetting();
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Shadow Cascade Settings"))
        {
            ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.1f, 1.0f), "Warning: Using \"Strongest\" Shadow Softening will make seams more visible.");
            DrawCascadeToggleCheckbox();
            if (Globals.Config.Enabled && !Globals.Config.DynamicCascadeMode)
            {
                ImGui.Separator();
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
                if (WindowDrawHelpers.DrawButtonTooltip("Preset Editor", "Popout the preset editor."))
                {
                    TogglePresetEditorPopout();
                }

                ImGui.SameLine();
                if (WindowDrawHelpers.DrawButtonTooltip("Preset List", "Popout the preset list."))
                {
                    TogglePresetListPopout();
                }

                ImGui.SameLine();
                if (WindowDrawHelpers.DrawButtonTooltip("Zone List", "Popout the zone list."))
                {
                    TogglePresetZonePopout();
                }

                if (WindowDrawHelpers.DrawButtonTooltip("Copy Presets and Zone Config", "Copy your entire configuration for sharing."))
                {
                    ImGui.SetClipboardText(Globals.Config.shared.ToBase64());
                }

                ImGui.SameLine();
                if (WindowDrawHelpers.DrawButtonTooltip("Paste Presets and Zone Config", "Paste a shared configuration. This will destroy your existing config."))
                {
                    Globals.Config.shared = SharableData.FromBase64(ImGui.GetClipboardText());
                }

                ImGui.SameLine();
                if (ImGui.Button("Recover Default Presets"))
                {
                    Globals.Config.RecoverStockPresets();
                }

                string preview = "";
                foreach (CascadeConfig config in Globals.Config.shared.cascadePresets)
                {
                    if (Globals.Config.shared.defaultPreset == config.GUID)
                    {
                        preview = config.Name;
                        break;
                    }
                }

                if (ImGui.BeginCombo("Default Preset", preview))
                {
                    foreach (CascadeConfig config in Globals.Config.shared.cascadePresets)
                    {
                        if (ImGui.Selectable(config.Name, Globals.Config.shared.defaultPreset == config.GUID))
                        {
                            Globals.Config.shared.defaultPreset = config.GUID;
                            Globals.ReapplyPreset = true;
                        }
                    }
                    ImGui.EndCombo();
                }
                ConfigWindowHelpers.DrawTooltip("The default preset to use when a zone config has no parent.");
            }

            ImGui.Separator();

            // preset editor stuff
            if (Globals.Config.Enabled && !Globals.Config.DynamicCascadeMode)
            {
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

                if (Globals.Config.ZoneConfigBeforePreset)
                {
                    ConfigWindowHelpers.DrawZonePresetList(regionAvail);
                    ImGui.SetNextItemWidth(regionAvail.X);
                    regionAvail = ImGui.GetContentRegionAvail();
                    ConfigWindowHelpers.DrawPresetSelector(regionAvail, ref set_override);
                }
                else
                {
                    ConfigWindowHelpers.DrawPresetSelector(regionAvail, ref set_override);
                    ImGui.SetNextItemWidth(regionAvail.X);
                    regionAvail = ImGui.GetContentRegionAvail();
                    ConfigWindowHelpers.DrawZonePresetList(regionAvail);
                }

                if (set_override)
                {
                    Globals.Config.EditOverride = set_override;
                }
            }

            ImGui.TreePop();
        }
        else
        {
            ImGui.Separator();
        }
    }
}
