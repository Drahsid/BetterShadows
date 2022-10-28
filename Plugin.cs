using BetterShadows.Attributes;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

[assembly: System.Reflection.AssemblyVersion("1.0.0.*")]

namespace BetterShadows
{
    public class Plugin : IDalamudPlugin
    {
        private readonly DalamudPluginInterface pluginInterface;
        private readonly ChatGui chat;
        private readonly ClientState clientState;

        private readonly PluginCommandManager<Plugin> commandManager;
        private readonly Configuration config;
        private readonly WindowSystem windowSystem;

        private IntPtr Text_ShadowCascade0 = IntPtr.Zero;
        private IntPtr Text_ShadowCascade1 = IntPtr.Zero;
        private IntPtr Text_ShadowCascade2 = IntPtr.Zero;
        private IntPtr Text_ShadowCascade3 = IntPtr.Zero;
        private IntPtr Text_ShadowmapResolution = IntPtr.Zero;
        private byte[] OriginalBytes_ShadowCascade0 = new byte[8];
        private byte[] OriginalBytes_ShadowCascade1 = new byte[8];
        private byte[] OriginalBytes_ShadowCascade2 = new byte[8];
        private byte[] OriginalBytes_ShadowCascade3 = new byte[8];
        private byte[] OriginalBytes_ShadowmapResolution = new byte[8];

        public string Name => "Better Shadows";

        public Plugin(DalamudPluginInterface pi, CommandManager com, ChatGui ch, ClientState cs)
        {
            pluginInterface = pi;
            chat = ch;
            clientState = cs;

            // Get or create a configuration object
            config = (Configuration)pluginInterface.GetPluginConfig()
                          ?? pluginInterface.Create<Configuration>();

            // Initialize the UI
            windowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);

            pluginInterface.UiBuilder.Draw += this.OnDraw;

            // Load all of our commands
            commandManager = new PluginCommandManager<Plugin>(this, com);

            pluginInterface.Create<Service>();

            if (config.Enabled)
            {
                DoEnable();
            }

            if (config.HigherResShadowmap)
            {
                DoEnableShadowmap();
            }

            pluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
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

        private void ReadWriteCode(IntPtr addr, ref byte[] originalBytes, int byteCount = 5)
        {
            const byte NOP = 0x90;
            if (addr != IntPtr.Zero)
            {
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
                for (int index = 0; index < byteCount; index++)
                {
                    originalBytes[index] = Marshal.ReadByte(addr + index);
                    Marshal.WriteByte(addr + index, NOP);
                }
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
            }
        }

        private void ReadWriteShadowmapCode(IntPtr addr, ref byte[] originalBytes, int byteCount = 5)
        {
            if (addr != IntPtr.Zero)
            {
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
                for (int index = 0; index < byteCount; index++)
                {
                    originalBytes[index] = Marshal.ReadByte(addr + index);
                }
                Marshal.WriteInt32(addr + 1, 0x00001000);
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
            }
        }

        private void RestoreCode(IntPtr addr, byte[] originalBytes, int byteCount = 5)
        {
            if (addr != IntPtr.Zero)
            {
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
                for (int index = 0; index < byteCount; index++)
                {
                    Marshal.WriteByte(addr + index, originalBytes[index]);
                }
                MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
            }
        }

        private unsafe void RestoreShadowmapCode(IntPtr addr, byte[] originalBytes)
        {
            ShadowManager* shadowManager = ShadowManager.Instance();

            if (addr != IntPtr.Zero)
            {
                RestoreCode(addr, originalBytes);
                shadowManager->Unk_Bitfield |= 1;
            }
        }

        private unsafe void DoEnable()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();

            if (shadowManager == null)
            {
                PluginLog.Error("shadowManager is null!");
                return;
            }

            // if regalloc ever changes, these will fail; may be better to hijack the whole function
            Text_ShadowCascade0 = Service.SigScanner.ScanText("F3 0F 11 4F 44 F3 44 0F 5C");
            Text_ShadowCascade1 = Service.SigScanner.ScanText("F3 0F 11 47 48 F3 41 0F 58");
            Text_ShadowCascade2 = Service.SigScanner.ScanText("F3 0F 11 5F 4C 48 8D 9F 18");
            Text_ShadowCascade3 = Service.SigScanner.ScanText("F3 44 0F 11 6F 50 48 8B 05");

            ReadWriteCode(Text_ShadowCascade0, ref OriginalBytes_ShadowCascade0);
            ReadWriteCode(Text_ShadowCascade1, ref OriginalBytes_ShadowCascade1);
            ReadWriteCode(Text_ShadowCascade2, ref OriginalBytes_ShadowCascade2);
            ReadWriteCode(Text_ShadowCascade3, ref OriginalBytes_ShadowCascade3, 6);
        }

        private unsafe void DoEnableShadowmap()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();

            if (shadowManager == null)
            {
                PluginLog.Error("shadowManager is null!");
                return;
            }

            Text_ShadowmapResolution = Service.SigScanner.ScanText("BA ?? ?? ?? ?? EB 0C BA 00 04");

            ReadWriteShadowmapCode(Text_ShadowmapResolution, ref OriginalBytes_ShadowmapResolution);
            if (shadowManager != null)
            {
                shadowManager->Unk_Bitfield |= 1;
            }
        }

        private void DoDisable()
        {
            RestoreCode(Text_ShadowCascade0, OriginalBytes_ShadowCascade0);
            RestoreCode(Text_ShadowCascade1, OriginalBytes_ShadowCascade1);
            RestoreCode(Text_ShadowCascade2, OriginalBytes_ShadowCascade2);
            RestoreCode(Text_ShadowCascade3, OriginalBytes_ShadowCascade3, 6);

            Text_ShadowCascade0 = IntPtr.Zero;
            Text_ShadowCascade1 = IntPtr.Zero;
            Text_ShadowCascade2 = IntPtr.Zero;
            Text_ShadowCascade3 = IntPtr.Zero;
        }

        private void DoDisableShadowmap()
        {
            RestoreShadowmapCode(Text_ShadowmapResolution, OriginalBytes_ShadowmapResolution);
            Text_ShadowmapResolution = IntPtr.Zero;
        }

        public void ToggleConfig()
        {
            config.ShowConfig = !config.ShowConfig;
        }

        public unsafe void OnDraw()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();
            bool shouldEnable = false;
            float charX = ImGui.CalcTextSize("F").X;
            float charY = ImGui.CalcTextSize("F").Y;
            float minWindowWidth = 0;
            float minLeftColWidth = 0;
            float maxLeftColWidth = 0;
            string copysavedelete = "Copy Save Delete";

            if (config.ShowConfig)
            {
                for (int index = 0; index < config.cascadePresets.Count; index++)
                {
                    if (config.cascadePresets[index].Name.Length > minWindowWidth) {
                        minLeftColWidth = config.cascadePresets[index].Name.Length;
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

                ImGui.Begin("Better Shadows Config");
                {
                    if (ImGui.Checkbox("Enable Custom Cascade Values", ref config.Enabled))
                    {
                        if (config.Enabled)
                        {
                            DoEnable();
                        }
                        else
                        {
                            DoDisable();
                        }
                    }

                    if (ImGui.Checkbox("2048p = 4096p shadowmap", ref config.HigherResShadowmap))
                    {
                        if (config.HigherResShadowmap)
                        {
                            DoEnableShadowmap();
                        }
                        else
                        {
                            DoDisableShadowmap();
                        }
                    }

                    ImGui.Separator();

                    Vector2 sxsy = ImGui.GetWindowSize();
                    float height = 11 * charY;
                    float height2 = config.cascadePresets.Count * (charY + 4);
                    if (height2 > height)
                    {
                        height = height2;
                    }


                    ImGui.BeginChild("##BSHADOWSCONFCHILD", new Vector2(sxsy.X - 8, height));
                    ImGui.Columns(2, "Presets"); {
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

                        for (int index = 0; index < config.cascadePresets.Count; index++)
                        {
                            if (ImGui.Button($"Copy##BSHADOWS_COPY_{config.cascadePresets[index].Name}_{index}"))
                            {
                                ImGui.SetClipboardText(JsonConvert.SerializeObject(config.cascadePresets[index]));
                            }

                            ImGui.SameLine();
                            if (ImGui.Button($"Save##BSHADOWS_SAVE_{config.cascadePresets[index].Name}_{index}"))
                            {
                                config.cascadePresets[index].Name = config.cascades.Name;
                                config.cascadePresets[index].CascadeDistance0 = config.cascades.CascadeDistance0;
                                config.cascadePresets[index].CascadeDistance1 = config.cascades.CascadeDistance1;
                                config.cascadePresets[index].CascadeDistance2 = config.cascades.CascadeDistance2;
                                config.cascadePresets[index].CascadeDistance3 = config.cascades.CascadeDistance3;
                            }

                            ImGui.SameLine();
                            if (ImGui.Button($"Delete##BSHADOWS_DELETE_{config.cascadePresets[index].Name}_{index}"))
                            {
                                config.cascadePresets.RemoveAt(index);
                                break;
                            }

                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(cw - ImGui.GetCursorPosX());
                            if (ImGui.Selectable(config.cascadePresets[index].Name))
                            {
                                config.lastSelectedPreset = config.cascadePresets[index].Name;
                                config.cascades.Name = config.cascadePresets[index].Name;
                                config.cascades.CascadeDistance0 = config.cascadePresets[index].CascadeDistance0;
                                config.cascades.CascadeDistance1 = config.cascadePresets[index].CascadeDistance1;
                                config.cascades.CascadeDistance2 = config.cascadePresets[index].CascadeDistance2;
                                config.cascades.CascadeDistance3 = config.cascadePresets[index].CascadeDistance3;
                            }

                            if (index == config.cascadePresets.Count - 1)
                            {
                                if (ImGui.Button("Paste"))
                                {
                                    config.cascadePresets.Add(JsonConvert.DeserializeObject<CascadeConfig>(ImGui.GetClipboardText()));
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("+"))
                                {
                                    config.cascadePresets.Add(new CascadeConfig(config.cascades));
                                }
                            }
                        }

                        ImGui.NextColumn();
                        ImGui.SetCursorPosY(ImGui.GetScrollY());

                        DrawFloatInput("Slider Max", ref config.SliderMax, 10, 32768);
                        DrawFloatInput("Cascade Distance 0", ref config.cascades.CascadeDistance0, 0.1f, config.cascades.CascadeDistance1);
                        DrawFloatInput("Cascade Distance 1", ref config.cascades.CascadeDistance1, config.cascades.CascadeDistance0, config.cascades.CascadeDistance2);
                        DrawFloatInput("Cascade Distance 2", ref config.cascades.CascadeDistance2, config.cascades.CascadeDistance1, config.cascades.CascadeDistance3);
                        DrawFloatInput("Cascade Distance 3", ref config.cascades.CascadeDistance3, config.cascades.CascadeDistance2, config.SliderMax);

                        ImGui.SetNextItemWidth(charX * 34);
                        ImGui.InputText("Name", ref config.cascades.Name, 32);

                        if (ImGui.Button($"Copy##BSHADOWS_COPY_RIGHTCOLUMN"))
                        {
                            ImGui.SetClipboardText(JsonConvert.SerializeObject(config.cascades));
                        }

                        ImGui.Columns();
                    }
                    ImGui.EndChild();

                    ImGui.Separator();
                }
                ImGui.End();
            }

            if (shadowManager != null && config.Enabled) {
                shadowManager->CascadeDistance0 = config.cascades.CascadeDistance0;
                shadowManager->CascadeDistance1 = config.cascades.CascadeDistance1;
                shadowManager->CascadeDistance2 = config.cascades.CascadeDistance2;
                shadowManager->CascadeDistance3 = config.cascades.CascadeDistance3;
            }
        }

        [Command("/pbshadows")]
        [HelpMessage("Toggle configuration window")]
        public void ExampleCommand1(string command, string args)
        {
            config.ShowConfig = !config.ShowConfig;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;

            if (config.Enabled || config.HigherResShadowmap)
            {
                DoDisable();
                DoDisableShadowmap();
            }

            commandManager.Dispose();

            pluginInterface.SavePluginConfig(this.config);

            pluginInterface.UiBuilder.Draw -= this.OnDraw;
            windowSystem.RemoveAllWindows();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
