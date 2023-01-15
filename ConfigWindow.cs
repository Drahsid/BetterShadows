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
        public ConfigWindow(Plugin plugin) : base("Better Shadows Globals.Config")
        {
            this.Size = new Vector2(240, 240);
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

        public override unsafe void Draw()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();
            float charX = ImGui.CalcTextSize("F").X;
            float charY = ImGui.CalcTextSize("F").Y;
            float minWindowWidth = 0;
            float minLeftColWidth = 0;
            float maxLeftColWidth = 0;
            string copysavedelete = "Copy Save Delete";

            if (Globals.Config.ShowConfig)
            {
                for (int index = 0; index < Globals.Config.cascadePresets.Count; index++)
                {
                    if (Globals.Config.cascadePresets[index].Name.Length > minWindowWidth)
                    {
                        minLeftColWidth = Globals.Config.cascadePresets[index].Name.Length;
                    }
                }

                minLeftColWidth += copysavedelete.Length + 2;
                maxLeftColWidth = (float)Math.Floor(ImGui.GetWindowWidth() / charX) - minLeftColWidth;

                // either constrain to window, to the contents of right column
                if (maxLeftColWidth < 32)
                {
                    maxLeftColWidth = 32;
                }

                minLeftColWidth *= charX;
                maxLeftColWidth *= charY;

                minWindowWidth = minLeftColWidth + maxLeftColWidth;


                ImGui.SetNextWindowSizeConstraints(new Vector2(minWindowWidth, ImGui.GetWindowHeight()), ImGui.GetMainViewport().Size);

                ImGui.Begin("Better Shadows Globals.Config");
                {
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

                    Vector2 sxsy = ImGui.GetWindowSize();
                    float height = 11 * charY;
                    float height2 = Globals.Config.cascadePresets.Count * (charY + 4);
                    if (height2 > height)
                    {
                        height = height2;
                    }


                    ImGui.BeginChild("##BSHADOWSCONFCHILD", new Vector2(sxsy.X - 8, height));
                    ImGui.Columns(2, "Presets");
                    {
                        float cw = ImGui.GetColumnWidth(0);
                        if (cw < minLeftColWidth)
                        {
                            ImGui.SetColumnWidth(0, minLeftColWidth);
                        }

                        if (ImGui.GetWindowSize().X - cw < maxLeftColWidth)
                        {

                            ImGui.SetColumnWidth(0, maxLeftColWidth - minLeftColWidth);
                        }
                        cw = ImGui.GetColumnWidth(0);

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
                            ImGui.SetNextItemWidth(cw - ImGui.GetCursorPosX());
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

                        ImGui.NextColumn();
                        ImGui.SetCursorPosY(ImGui.GetScrollY());

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

                        ImGui.Columns();
                    }
                    ImGui.EndChild();

                    ImGui.Separator();
                }
                ImGui.End();
            }

            if (shadowManager != null && Globals.Config.Enabled)
            {
                shadowManager->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                shadowManager->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                shadowManager->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                shadowManager->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
            }
        }

        public void Dispose() { }
    }
}
