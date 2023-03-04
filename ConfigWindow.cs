using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FFXIVClientStructs.Havok;
using Dalamud.Logging;

namespace BetterShadows
{
    public class ConfigWindow : Window, IDisposable
    {
        public static string ConfigWindowName = "Better Shadows Config";
        WindowSizeConstraints WindSizeConstraints;

        public ConfigWindow(Plugin plugin) : base(ConfigWindowName) {
        }

        private bool DrawFloatInput(string text, ref float cvar, float min, float max, string tooltip = "")
        {
            float input_width = ImGui.CalcTextSize("F").X * 10;
            bool result = false;

            ImGui.SetNextItemWidth(input_width * 2);
            result |= ImGui.SliderFloat(text, ref cvar, min, max);
            if (tooltip != "") {
                DrawTooltip(tooltip);
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(input_width);
            result |= ImGui.InputFloat($"##{text}", ref cvar);
            if (tooltip != "") {
                DrawTooltip(tooltip);
            }
            return result;
        }

        private void DrawTooltip(string text) {
            if (ImGui.IsItemHovered() && Globals.Config.HideTooltips == false) {
                ImGui.SetTooltip(text);
            }
        }

        private void DrawMapPresetTree(Dictionary<string, ConfigTreeNode> presets, string[] zone, string parent_path = "???")
        {
            foreach (var preset in presets)
            {
                string[] newZone = new string[zone.Length + 1];
                string zoneFriendly = "";
                Array.Copy(zone, newZone, zone.Length);
                newZone[newZone.Length - 1] = preset.Key;
                zoneFriendly = String.Join("/", newZone);
                if (ImGui.TreeNode($"{preset.Key}##{zoneFriendly}"))
                {
                    ImGui.TextDisabled(zoneFriendly);
                    ImGui.Checkbox($"Default##{zoneFriendly}{preset.Key}", ref preset.Value.Default);

                    string preview = "";
                    foreach (CascadeConfig config in Globals.Config.shared.cascadePresets) {
                        if (preset.Value.Preset == config.GUID) {
                            preview = config.Name;
                            break;
                        }
                    }

                    if (ImGui.BeginCombo($"Preset##{zoneFriendly}{preset.Key}", preview))
                    {
                        foreach (CascadeConfig config in Globals.Config.shared.cascadePresets)
                        {
                            if (ImGui.Selectable(config.Name, preset.Value.Preset == config.GUID))
                            {
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

                    if (preset.Value.Children != null && preset.Value.Children.Count > 0)
                    {
                        DrawMapPresetTree(preset.Value.Children, newZone, parent_path + $"/{preset.Value.Name}");
                    }
                    ImGui.TreePop();
                }
            }
        }

        private void DrawZonePresetList(Vector2 regionAvail) {
            ImGui.Text("Zone Preset Config");
            ImGui.BeginChild("##ZonePresetConfigList", regionAvail);
            foreach (var preset in Globals.Config.shared.mapPresets) {
                DrawMapPresetTree(preset.Value.Children, new string[] { preset.Key });
            }
            ImGui.EndChild();
        }

        private void DrawPresetSelector(Vector2 regionAvail, ref bool set_override) {
            ImGui.Text("Presets");
            ImGui.BeginChild("##PresetConfigList", regionAvail);
            for (int index = 0; index < Globals.Config.shared.cascadePresets.Count; index++) {
                if (ImGui.Button($"Copy##BSHADOWS_COPY_{Globals.Config.shared.cascadePresets[index].Name}_{index}")) {
                    ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.shared.cascadePresets[index]));
                }
                DrawTooltip("Copy saved values to the clipboard.");

                ImGui.SameLine();
                if (ImGui.Button($"Save##BSHADOWS_SAVE_{Globals.Config.shared.cascadePresets[index].Name}_{index}")) {
                    Globals.Config.shared.cascadePresets[index].Name = Globals.Config.cascades.Name;
                    Globals.Config.shared.cascadePresets[index].CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                    Globals.Config.shared.cascadePresets[index].CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                    Globals.Config.shared.cascadePresets[index].CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                    Globals.Config.shared.cascadePresets[index].CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
                }
                DrawTooltip("Save current values in the preset editor to this preset. You can change the name without disconnecting it from zones that use it.");

                ImGui.SameLine();
                if (ImGui.Button($"Delete##BSHADOWS_DELETE_{Globals.Config.shared.cascadePresets[index].Name}_{index}")) {
                    Globals.Config.shared.cascadePresets.RemoveAt(index);
                    break;
                }
                DrawTooltip("Delete the preset from the list. This will cause zones which use this preset to use the default config.");

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
                    if (ImGui.Button("Paste")) {
                        Globals.Config.shared.cascadePresets.Add(JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText()));
                    }
                    DrawTooltip("Paste preset values from the clipboard as a new preset.");
                    ImGui.SameLine();
                    if (ImGui.Button("+")) {
                        Globals.Config.shared.cascadePresets.Add(new CascadeConfig(Globals.Config.cascades));
                    }
                    DrawTooltip("Add a new preset.");
                }
            }
            ImGui.EndChild();
        }

        public override void Draw()
        {
            float charX = ImGui.CalcTextSize("F").X;
            float charY = ImGui.CalcTextSize("F").Y;
            bool set_override = false;

            if (ImGui.Button("Copy Presets and Zone Config")) {
                ImGui.SetClipboardText(Globals.Config.shared.ToBase64());
            }
            DrawTooltip("Copy your entire configuration for sharing.");

            ImGui.SameLine();
            if (ImGui.Button("Paste Presets and Zone Config")) {
                Globals.Config.shared = SharableData.FromBase64(ImGui.GetClipboardText());
            }
            DrawTooltip("Paste a shared configuration. This will destroy your existing config.");

            if (ImGui.Checkbox("Enable Custom Cascade Values", ref Globals.Config.Enabled))
            {
                Globals.ToggleHacks();
            }
            DrawTooltip("Enable or disable the usage of custom shadow cascade values. When this is disabled, the Zone Preset Config section is hidden, since it would be unused.");

            if (ImGui.Checkbox("2048p = 4096p shadowmap", ref Globals.Config.HigherResShadowmap))
            {
                Globals.ToggleShadowmap();
            }
            DrawTooltip("Enable or disable using a 4096p shadowmap when you have the 2048p shadowmap setting. This doubles the resolution of the shadowmap when enabled, making shadows look clearer.");

            ImGui.Checkbox("Hide tooltips", ref Globals.Config.HideTooltips);
            DrawTooltip("Hide tooltips when hovering over settings.");

            ImGui.Checkbox("Show Zone Preset Config before Presets", ref Globals.Config.ZoneConfigBeforePreset);
            DrawTooltip("When enabled, this reorders the config options below to show the Zone Preset Config first.");

            ImGui.Checkbox("Edit Override", ref Globals.Config.EditOverride);
            DrawTooltip("When enabled, ignores the Zone Preset Config, and uses the values that are currently in the preset editor. When making changes to a preset, this is automatically enabled.");

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
            DrawTooltip("The default preset to use when a zone config has no parent.");

            ImGui.Separator();

            // cascade config
            ImGui.Text("Selected Preset: ");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.0f, 1.0f), Globals.Config.cascades.Name);
            DrawFloatInput("Slider Max", ref Globals.Config.SliderMax, 10, 32768, "The max range for the furthest cascade");
            set_override |= DrawFloatInput("Cascade Distance 0", ref Globals.Config.cascades.CascadeDistance0, 0.1f, Globals.Config.cascades.CascadeDistance1, "The distance of the closest cascade. This should have the lowest value.");
            set_override |= DrawFloatInput("Cascade Distance 1", ref Globals.Config.cascades.CascadeDistance1, Globals.Config.cascades.CascadeDistance0, Globals.Config.cascades.CascadeDistance2, "The distance of the second closest cascade. The value of this should be between Cascade Distance 0, and Cascade Distance 2.");
            set_override |= DrawFloatInput("Cascade Distance 2", ref Globals.Config.cascades.CascadeDistance2, Globals.Config.cascades.CascadeDistance1, Globals.Config.cascades.CascadeDistance3, "The distance of the second farthest cascade. The value of this should be between Cascade Distance 1, and Cascade Distance 3.");
            set_override |= DrawFloatInput("Cascade Distance 3", ref Globals.Config.cascades.CascadeDistance3, Globals.Config.cascades.CascadeDistance2, Globals.Config.SliderMax, "The distance of the farthest cascade. The value of this should be the largest.");

            ImGui.SetNextItemWidth(charX * 34);
            ImGui.InputText("Name", ref Globals.Config.cascades.Name, 32);
            DrawTooltip("The name of this preset.");

            if (ImGui.Button($"Copy##BSHADOWS_COPY_RIGHTCOLUMN"))
            {
                ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.cascades));
            }
            DrawTooltip("Copy the values in the preset editor to the clipboard.");

            ImGui.SameLine();
            if (ImGui.Button($"Paste##BSHADOWS_PASTE_RIGHTCOLUMN")) {
                Globals.Config.cascades = JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText());
            }
            DrawTooltip("Paste the values in clipboard to the preset editor.");

            ImGui.Separator();

            const float offset = 32.0f;
            const float thick = 4.0f;

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
                DrawZonePresetList(regionAvail);
                ImGui.SetNextItemWidth(regionAvail.X);
                ImGui.Separator();
                DrawPresetSelector(regionAvail, ref set_override);
            }
            else {
                DrawPresetSelector(regionAvail, ref set_override);
                ImGui.SetNextItemWidth(regionAvail.X);
                ImGui.Separator();
                DrawZonePresetList(regionAvail);
            }

            if (set_override) {
                
                Globals.Config.EditOverride = set_override;
            }
        }

        public void Dispose() { }
    }
}
