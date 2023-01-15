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
        private DalamudPluginInterface PluginInterface;
        private ChatGui Chat;
        private ClientState ClientState;
        private PluginCommandManager<Plugin> CommandManager;
        private WindowSystem WindowSystem;

        public string Name => "Better Shadows";

        public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chat, ClientState clientState)
        {
            PluginInterface = pluginInterface;
            Chat = chat;
            ClientState = clientState;

            // Initialize the UI
            WindowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
            WindowSystem.AddWindow(new ConfigWindow(this));

            // Load all of our commands
            CommandManager = new PluginCommandManager<Plugin>(this, commandManager);

            PluginInterface.Create<Service>();

            // Get or create a configuration object
            Globals.Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Globals.Config.Initialize(PluginInterface);

            pluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        }

        public void ToggleConfig()
        {
            Globals.Config.ShowConfig = !Globals.Config.ShowConfig;
        }

        [Command("/pbshadows")]
        [HelpMessage("Toggle configuration window")]
        public void Command_pbshadows(string command, string args)
        {
            ToggleConfig();
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;

            if (Globals.Config.Enabled || Globals.Config.HigherResShadowmap)
            {
                CodeManager.DoDisableHacks();
                CodeManager.DoDisableShadowmap();
            }

            CommandManager.Dispose();

            PluginInterface.SavePluginConfig(Globals.Config);

            WindowSystem.RemoveAllWindows();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
