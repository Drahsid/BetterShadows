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
        private byte[] originalBytes0 = new byte[8];
        private byte[] originalBytes1 = new byte[8];
        private byte[] originalBytes2 = new byte[8];
        private byte[] originalBytes3 = new byte[8];

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

        public unsafe void OnDraw()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();

            if (config.ShowConfig)
            {
                ImGui.Begin("Better Shadows Config");
                {
                    if (shadowManager != null)
                    {
                        ImGui.Text($"Addr is {((IntPtr)shadowManager).ToString("x")}");
                        ImGui.Text($"CascadeDistance0 {shadowManager->CascadeDistance0}");
                        ImGui.Text($"CascadeDistance1 {shadowManager->CascadeDistance1}");
                        ImGui.Text($"CascadeDistance2 {shadowManager->CascadeDistance2}");
                        ImGui.Text($"CascadeDistance3 {shadowManager->CascadeDistance3}");
                    }
                    DrawFloatInput("Slider Max", ref config.SliderMax, 10, 32768);
                    DrawFloatInput("Cascade Distance 0", ref config.CascadeDistance0, 0.1f, config.CascadeDistance1);
                    DrawFloatInput("Cascade Distance 1", ref config.CascadeDistance1, config.CascadeDistance0, config.CascadeDistance2);
                    DrawFloatInput("Cascade Distance 2", ref config.CascadeDistance2, config.CascadeDistance1, config.CascadeDistance3);
                    DrawFloatInput("Cascade Distance 3", ref config.CascadeDistance3, config.CascadeDistance2, config.SliderMax);
                    if (ImGui.Checkbox("Enabled", ref config.Enabled))
                    {
                        if (config.Enabled)
                        {
                            // if regalloc ever changes, these will fail; may be better to hijack the whole function
                            bytes0 = Service.SigScanner.ScanText("F3 0F 11 4F 44 F3 44 0F 5C");
                            bytes1 = Service.SigScanner.ScanText("F3 0F 11 47 48 F3 41 0F 58");
                            bytes2 = Service.SigScanner.ScanText("F3 0F 11 5F 4C 48 8D 9F 18");
                            bytes3 = Service.SigScanner.ScanText("F3 44 0F 11 6F 50 48 8B 05");

                            ReadWriteCode(bytes0, ref originalBytes0);
                            ReadWriteCode(bytes1, ref originalBytes1);
                            ReadWriteCode(bytes2, ref originalBytes2);
                            ReadWriteCode(bytes3, ref originalBytes3, 6);
                        }
                        else
                        {
                            RestoreCode(bytes0, originalBytes0);
                            RestoreCode(bytes1, originalBytes1);
                            RestoreCode(bytes2, originalBytes2);
                            RestoreCode(bytes3, originalBytes3, 6);

                            bytes0 = IntPtr.Zero;
                            bytes1 = IntPtr.Zero;
                            bytes2 = IntPtr.Zero;
                            bytes3 = IntPtr.Zero;
                        }
                    }
                }
                ImGui.End();
            }

            if (shadowManager != null && config.Enabled) {
                shadowManager->CascadeDistance0 = config.CascadeDistance0;
                shadowManager->CascadeDistance1 = config.CascadeDistance1;
                shadowManager->CascadeDistance2 = config.CascadeDistance2;
                shadowManager->CascadeDistance3 = config.CascadeDistance3;
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
