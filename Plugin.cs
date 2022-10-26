using BetterShadows.Attributes;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;

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

        private IntPtr bytes0 = IntPtr.Zero;
        private IntPtr bytes1 = IntPtr.Zero;
        private IntPtr bytes2 = IntPtr.Zero;
        private IntPtr bytes3 = IntPtr.Zero;
        private IntPtr bytes4 = IntPtr.Zero;
        private byte[] originalBytes0 = new byte[8];
        private byte[] originalBytes1 = new byte[8];
        private byte[] originalBytes2 = new byte[8];
        private byte[] originalBytes3 = new byte[8];
        private byte[] originalBytes4 = new byte[8];

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

            // if regalloc ever changes, these will fail; may be better to hijack the whole function
            bytes0 = Service.SigScanner.ScanText("F3 0F 11 4F 44 F3 44 0F 5C");
            bytes1 = Service.SigScanner.ScanText("F3 0F 11 47 48 F3 41 0F 58");
            bytes2 = Service.SigScanner.ScanText("F3 0F 11 5F 4C 48 8D 9F 18");
            bytes3 = Service.SigScanner.ScanText("F3 44 0F 11 6F 50 48 8B 05");

            bytes4 = Service.SigScanner.ScanText("BA ?? ?? ?? ?? EB 0C BA 00 04");

            ReadWriteCode(bytes0, ref originalBytes0);
            ReadWriteCode(bytes1, ref originalBytes1);
            ReadWriteCode(bytes2, ref originalBytes2);
            ReadWriteCode(bytes3, ref originalBytes3, 6);

            if (config.HigherResShadowmap)
            {
                ReadWriteShadowmapCode(bytes4, ref originalBytes4);
                if (shadowManager != null)
                {
                    shadowManager->Unk_Bitfield |= 1;
                }
            }
        }

        private void DoDisable()
        {
            RestoreCode(bytes0, originalBytes0);
            RestoreCode(bytes1, originalBytes1);
            RestoreCode(bytes2, originalBytes2);
            RestoreCode(bytes3, originalBytes3, 6);

            if (config.HigherResShadowmap)
            {
                RestoreShadowmapCode(bytes4, originalBytes4);
            }

            bytes0 = IntPtr.Zero;
            bytes1 = IntPtr.Zero;
            bytes2 = IntPtr.Zero;
            bytes3 = IntPtr.Zero;
            bytes4 = IntPtr.Zero;
        }

        public unsafe void OnDraw()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();
            bool shouldEnable = false;

            if (config.ShowConfig)
            {
                ImGui.Begin("Better Shadows Config");
                {
                    if (ImGui.Checkbox("Enabled", ref config.Enabled))
                    {
                        if (config.Enabled)
                        {
                            shouldEnable = true;
                        }
                        else
                        {
                            DoDisable();
                        }
                    }

                    if (config.Enabled) {
                        if (ImGui.Checkbox("4k shadowmap", ref config.HigherResShadowmap))
                        {
                            if (config.HigherResShadowmap)
                            {
                                shouldEnable |= true;
                            }
                            else
                            {
                                RestoreShadowmapCode(bytes4, originalBytes4);
                            }
                        }
                    }

                    if (shouldEnable)
                    {
                        DoEnable();
                    }

                    ImGui.Separator();

                    ImGui.SetNextItemWidth(ImGui.CalcTextSize("F").X * 34);
                    if (ImGui.BeginCombo("Presets", $"{config.lastSelectedPreset}##NothingToPreviewLol"))
                    {
                        for (int index = 0; index < config.cascadePresets.Count; index++)
                        {
                            if (ImGui.Selectable(config.cascadePresets[index].Name))
                            {
                                config.lastSelectedPreset = config.cascadePresets[index].Name;
                                config.cascades.CascadeDistance0 = config.cascadePresets[index].CascadeDistance0;
                                config.cascades.CascadeDistance1 = config.cascadePresets[index].CascadeDistance1;
                                config.cascades.CascadeDistance2 = config.cascadePresets[index].CascadeDistance2;
                                config.cascades.CascadeDistance3 = config.cascadePresets[index].CascadeDistance3;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    DrawFloatInput("Slider Max", ref config.SliderMax, 10, 32768);
                    DrawFloatInput("Cascade Distance 0", ref config.cascades.CascadeDistance0, 0.1f, config.cascades.CascadeDistance1);
                    DrawFloatInput("Cascade Distance 1", ref config.cascades.CascadeDistance1, config.cascades.CascadeDistance0, config.cascades.CascadeDistance2);
                    DrawFloatInput("Cascade Distance 2", ref config.cascades.CascadeDistance2, config.cascades.CascadeDistance1, config.cascades.CascadeDistance3);
                    DrawFloatInput("Cascade Distance 3", ref config.cascades.CascadeDistance3, config.cascades.CascadeDistance2, config.SliderMax);

                    ImGui.SetNextItemWidth(ImGui.CalcTextSize("F").X * 34);
                    ImGui.InputText("Name", ref config.cascades.Name, 32);
                    ImGui.SameLine();
                    if (ImGui.Button("Save Preset"))
                    {
                        config.cascadePresets.Add(new CascadeConfig(config.cascades));
                    }
                    if (ImGui.Button("Delete"))
                    {
                        for (int index = 0; index < config.cascadePresets.Count; index++)
                        {
                            if (config.cascadePresets[index].Name == config.cascades.Name)
                            {
                                config.cascadePresets.RemoveAt(index);
                                break;
                            }
                        }
                    }

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

            if (config.Enabled)
            {
                DoDisable();
            }

            this.commandManager.Dispose();

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.Draw -= this.OnDraw;
            this.windowSystem.RemoveAllWindows();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
