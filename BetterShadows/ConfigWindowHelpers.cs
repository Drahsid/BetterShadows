using DrahsidLib;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterShadows; 
internal static class ConfigWindowHelpers {
    private static float Deadzone = 64;

    public static void DrawTooltip(string text) {
        if (ImGui.IsItemHovered() && Globals.Config.HideTooltips == false) {
            ImGui.SetTooltip(text);
        }
    }

    public static void DrawMapPresetTree(Dictionary<string, ConfigTreeNode> presets, string[] zone, string parent_path = "???") {
        foreach (var preset in presets) {
            string[] newZone = new string[zone.Length + 1];
            string zoneFriendly = "";
            Array.Copy(zone, newZone, zone.Length);
            newZone[newZone.Length - 1] = preset.Key;
            zoneFriendly = String.Join("/", newZone);
            if (ImGui.TreeNode($"{preset.Key}##{zoneFriendly}")) {
                ImGui.TextDisabled(zoneFriendly);
                ImGui.Checkbox($"Default##{zoneFriendly}{preset.Key}", ref preset.Value.Default);

                string preview = "";
                foreach (CascadeConfig config in Globals.Config.shared.cascadePresets) {
                    if (preset.Value.Preset == config.GUID) {
                        preview = config.Name;
                        break;
                    }
                }

                if (ImGui.BeginCombo($"Preset##{zoneFriendly}{preset.Key}", preview)) {
                    foreach (CascadeConfig config in Globals.Config.shared.cascadePresets) {
                        if (ImGui.Selectable(config.Name, preset.Value.Preset == config.GUID)) {
                            preset.Value.Default = false;
                            preset.Value.Preset = config.GUID;
                            Globals.ReapplyPreset = true;
                        }
                    }
                    ImGui.EndCombo();
                }
                if (preset.Value.Default) {
                    ImGui.SameLine();
                    if (parent_path == "???") {
                        ImGui.TextColored(new Vector4(0.75f, 1.0f, 0.75f, 1.0f), "(Inherited from default)");
                    }
                    else {
                        ImGui.TextColored(new Vector4(0.75f, 1.0f, 0.75f, 1.0f), $"(Inherited from {parent_path})");
                    }
                }

                if (preset.Value.Children != null && preset.Value.Children.Count > 0) {
                    DrawMapPresetTree(preset.Value.Children, newZone, parent_path + $"/{preset.Value.Name}");
                }
                ImGui.TreePop();
            }
        }
    }

    public static void DrawZonePresetList(Vector2 regionAvail) {
        float y_fix = ImGui.GetCursorPosY();
        ImGui.TextDisabled("Zone Preset Config");
        y_fix = ImGui.GetCursorPosY() - y_fix;
        regionAvail.Y -= 2.0f * y_fix;

        if (regionAvail.Y < Deadzone) {
            if (regionAvail.Y > 0) {
                regionAvail.Y += Deadzone;
            }
            else {
                regionAvail.Y = Deadzone;
            }
        }

        ImGui.BeginChild("##ZonePresetConfigList", regionAvail);
        if (Globals.Config.ShowContinent) {
            foreach (var preset in Globals.Config.shared.mapPresets) {
                if (ImGui.TreeNode($"{preset.Key}##{preset.Key}")) {
                    DrawMapPresetTree(preset.Value.Children, new string[] { preset.Key });
                    ImGui.TreePop();
                }
            }
        }
        else {
            foreach (var preset in Globals.Config.shared.mapPresets) {
                DrawMapPresetTree(preset.Value.Children, new string[] { preset.Key });
            }
        }
        ImGui.EndChild();

        ImGui.SetNextItemWidth(regionAvail.X);
        ImGui.Separator();
    }

    public static void DrawPresetSelector(Vector2 regionAvail, ref bool set_override) {
        float y_fix = ImGui.GetCursorPosY();
        ImGui.TextDisabled("Presets");
        y_fix = ImGui.GetCursorPosY() - y_fix;
        regionAvail.Y -= 2.0f * y_fix;

        if (regionAvail.Y < Deadzone) {
            if (regionAvail.Y > 0) {
                regionAvail.Y += Deadzone;
            }
            else {
                regionAvail.Y = Deadzone;
            }
        }

        ImGui.BeginChild("##PresetConfigList", regionAvail);
        for (int index = 0; index < Globals.Config.shared.cascadePresets.Count; index++) {
            if (WindowDrawHelpers.DrawButtonTooltip(
                $"Copy##BSHADOWS_COPY_{Globals.Config.shared.cascadePresets[index].Name}_{index}",
                "Copy saved values to the clipboard."))
            {
                ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.shared.cascadePresets[index]));
            }

            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip(
                $"Save##BSHADOWS_SAVE_{Globals.Config.shared.cascadePresets[index].Name}_{index}",
                "Save current values in the preset editor to this preset. You can change the name without disconnecting it from zones that use it.")) {
                Globals.Config.shared.cascadePresets[index].Name = Globals.Config.cascades.Name;
                Globals.Config.shared.cascadePresets[index].CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                Globals.Config.shared.cascadePresets[index].CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                Globals.Config.shared.cascadePresets[index].CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                Globals.Config.shared.cascadePresets[index].CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
            }

            ImGui.SameLine();
            if (WindowDrawHelpers.DrawButtonTooltip(
                $"Delete##BSHADOWS_DELETE_{Globals.Config.shared.cascadePresets[index].Name}_{index}",
                "Delete the preset from the list. This will cause zones which use this preset to use the default config."))
            {
                Globals.Config.shared.cascadePresets.RemoveAt(index);
                break;
            }

            ImGui.SameLine();
            if (ImGui.Selectable(Globals.Config.shared.cascadePresets[index].Name, Globals.Config.shared.cascadePresets[index].GUID == Globals.Config.cascades.GUID)) {
                set_override = true;
                Globals.Config.lastSelectedPreset = Globals.Config.shared.cascadePresets[index].Name;
                Globals.Config.cascades.Name = Globals.Config.shared.cascadePresets[index].Name;
                Globals.Config.cascades.CascadeDistance0 = Globals.Config.shared.cascadePresets[index].CascadeDistance0;
                Globals.Config.cascades.CascadeDistance1 = Globals.Config.shared.cascadePresets[index].CascadeDistance1;
                Globals.Config.cascades.CascadeDistance2 = Globals.Config.shared.cascadePresets[index].CascadeDistance2;
                Globals.Config.cascades.CascadeDistance3 = Globals.Config.shared.cascadePresets[index].CascadeDistance3;
                Globals.Config.cascades.GUID = Globals.Config.shared.cascadePresets[index].GUID;
            }

            if (index == Globals.Config.shared.cascadePresets.Count - 1) {
                if (WindowDrawHelpers.DrawButtonTooltip("Paste", "Paste preset values from the clipboard as a new preset.")) {
                    Globals.Config.shared.cascadePresets.Add(JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText()));
                }
                ImGui.SameLine();
                if (WindowDrawHelpers.DrawButtonTooltip("+", "Add a new preset.")) {
                    Globals.Config.shared.cascadePresets.Add(new CascadeConfig(Globals.Config.cascades));
                }
            }
        }
        ImGui.EndChild();

        ImGui.SetNextItemWidth(regionAvail.X);
        ImGui.Separator();
    }

    public static void DrawPresetEditor(ref bool set_override) {
        float charX = ImGui.CalcTextSize("F").X;
        float charY = ImGui.CalcTextSize("F").Y;

        ImGui.TextDisabled("Preset Editor");
        ImGui.Text("Selected Preset: ");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.0f, 1.0f), Globals.Config.cascades.Name);
        WindowDrawHelpers.DrawFloatInputTooltip(
            "Slider Max",
            ref Globals.Config.SliderMax,
            "The max range for the furthest cascade",
            Globals.Config.cascades.CascadeDistance3,
            32768.0f);

        set_override |= WindowDrawHelpers.DrawFloatInputTooltip(
            "Cascade Distance 0",
            ref Globals.Config.cascades.CascadeDistance0,
            "The distance of the closest cascade. This should have the lowest value.",
            0.1f,
            Globals.Config.cascades.CascadeDistance1);

        set_override |= WindowDrawHelpers.DrawFloatInputTooltip(
            "Cascade Distance 1",
            ref Globals.Config.cascades.CascadeDistance1,
            "The distance of the second closest cascade. The value of this should be between Cascade Distance 0, and Cascade Distance 2.",
            Globals.Config.cascades.CascadeDistance0,
            Globals.Config.cascades.CascadeDistance2);

        set_override |= WindowDrawHelpers.DrawFloatInputTooltip(
            "Cascade Distance 2",
            ref Globals.Config.cascades.CascadeDistance2,
            "The distance of the second farthest cascade. The value of this should be between Cascade Distance 1, and Cascade Distance 3.",
            Globals.Config.cascades.CascadeDistance1,
            Globals.Config.cascades.CascadeDistance3);

        set_override |= WindowDrawHelpers.DrawFloatInputTooltip(
            "Cascade Distance 3",
            ref Globals.Config.cascades.CascadeDistance3,
            "The distance of the farthest cascade. The value of this should be the largest.",
            Globals.Config.cascades.CascadeDistance2,
            Globals.Config.SliderMax);

        ImGui.SetNextItemWidth(charX * 34);
        WindowDrawHelpers.DrawInputTextTooltip("Name", ref Globals.Config.cascades.Name, "The name of this preset.");

        if (WindowDrawHelpers.DrawButtonTooltip($"Copy##BSHADOWS_COPY_RIGHTCOLUMN", "Copy the values in the preset editor to the clipboard.")) {
            ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.cascades));
        }

        ImGui.SameLine();
        if (WindowDrawHelpers.DrawButtonTooltip($"Paste##BSHADOWS_PASTE_RIGHTCOLUMN", "Paste the values in clipboard to the preset editor.")) {
            Globals.Config.cascades = JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText());
        }

        ImGui.Separator();
    }
}
