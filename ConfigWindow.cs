using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace BetterShadows
{
    public class ConfigWindow : Window, IDisposable
    {
        public static string ConfigWindowName = "Better Shadows Config";
        WindowSizeConstraints WindSizeConstraints;

        public ConfigWindow(Plugin plugin) : base(ConfigWindowName) {
        }

        private void DrawFloatInput(string text, ref float cvar, float min, float max)
        {
            float input_width = ImGui.CalcTextSize("F").X * 10;

            ImGui.SetNextItemWidth(input_width * 2);
            ImGui.SliderFloat(text, ref cvar, min, max);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(input_width);
            ImGui.InputFloat($"##{text}", ref cvar);
        }

        public override void Draw()
        {
            float charX = ImGui.CalcTextSize("F").X;
            float charY = ImGui.CalcTextSize("F").Y;

            if (ImGui.Checkbox("Enable Custom Cascade Values", ref Globals.Config.Enabled))
            {
                if (Globals.Config.Enabled)
                {
                    CodeManager.DoEnableHacks();
                }
                else
                {
                    CodeManager.DoDisableHacks();
                }
            }

            if (ImGui.Checkbox("2048p = 4096p shadowmap", ref Globals.Config.HigherResShadowmap))
            {
                if (Globals.Config.HigherResShadowmap)
                {
                    CodeManager.DoEnableShadowmap();
                }
                else
                {
                    CodeManager.DoDisableShadowmap();
                }
            }

            ImGui.Separator();

            // cascade config
            ImGui.Text("Config: " + Globals.Config.cascades.Name);
            DrawFloatInput("Slider Max", ref Globals.Config.SliderMax, 10, 32768);
            DrawFloatInput("Cascade Distance 0", ref Globals.Config.cascades.CascadeDistance0, 0.1f, Globals.Config.cascades.CascadeDistance1);
            DrawFloatInput("Cascade Distance 1", ref Globals.Config.cascades.CascadeDistance1, Globals.Config.cascades.CascadeDistance0, Globals.Config.cascades.CascadeDistance2);
            DrawFloatInput("Cascade Distance 2", ref Globals.Config.cascades.CascadeDistance2, Globals.Config.cascades.CascadeDistance1, Globals.Config.cascades.CascadeDistance3);
            DrawFloatInput("Cascade Distance 3", ref Globals.Config.cascades.CascadeDistance3, Globals.Config.cascades.CascadeDistance2, Globals.Config.SliderMax);

            ImGui.SetNextItemWidth(charX * 34);
            ImGui.InputText("Name", ref Globals.Config.cascades.Name, 32);

            if (ImGui.Button($"Copy##BSHADOWS_COPY_RIGHTCOLUMN"))
            {
                ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.cascades));
            }

            ImGui.Separator();

            Vector2 sxsy = ImGui.GetWindowSize();
            float height = ImGui.GetCursorPosY();
            ImGui.BeginChild("##BSHADOWSCONFCHILD", new Vector2(sxsy.X - 8, (sxsy.Y - 8) - height));
            ImGui.Text("Presets");
            for (int index = 0; index < Globals.Config.cascadePresets.Count; index++)
            {
                if (ImGui.Button($"Copy##BSHADOWS_COPY_{Globals.Config.cascadePresets[index].Name}_{index}"))
                {
                    ImGui.SetClipboardText(JsonConvert.SerializeObject(Globals.Config.cascadePresets[index]));
                }

                ImGui.SameLine();
                if (ImGui.Button($"Save##BSHADOWS_SAVE_{Globals.Config.cascadePresets[index].Name}_{index}"))
                {
                    Globals.Config.cascadePresets[index].Name = Globals.Config.cascades.Name;
                    Globals.Config.cascadePresets[index].CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                    Globals.Config.cascadePresets[index].CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                    Globals.Config.cascadePresets[index].CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                    Globals.Config.cascadePresets[index].CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
                }

                ImGui.SameLine();
                if (ImGui.Button($"Delete##BSHADOWS_DELETE_{Globals.Config.cascadePresets[index].Name}_{index}"))
                {
                    Globals.Config.cascadePresets.RemoveAt(index);
                    break;
                }

                ImGui.SameLine();
                if (ImGui.Selectable(Globals.Config.cascadePresets[index].Name))
                {
                    Globals.Config.lastSelectedPreset = Globals.Config.cascadePresets[index].Name;
                    Globals.Config.cascades.Name = Globals.Config.cascadePresets[index].Name;
                    Globals.Config.cascades.CascadeDistance0 = Globals.Config.cascadePresets[index].CascadeDistance0;
                    Globals.Config.cascades.CascadeDistance1 = Globals.Config.cascadePresets[index].CascadeDistance1;
                    Globals.Config.cascades.CascadeDistance2 = Globals.Config.cascadePresets[index].CascadeDistance2;
                    Globals.Config.cascades.CascadeDistance3 = Globals.Config.cascadePresets[index].CascadeDistance3;
                }

                if (index == Globals.Config.cascadePresets.Count - 1)
                {
                    if (ImGui.Button("Paste"))
                    {
                        Globals.Config.cascadePresets.Add(JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText()));
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("+"))
                    {
                        Globals.Config.cascadePresets.Add(new CascadeConfig(Globals.Config.cascades));
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Separator();
        }

        public void Dispose() { }
    }
}
