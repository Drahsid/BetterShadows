using BetterShadows.Attributes;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Reflection.AssemblyVersion("1.1.0")]

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

            Globals.DtrDisplay = new DtrDisplay();

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        }

        private unsafe void DrawPost()
        {
            ShadowManager* shadowManager = ShadowManager.Instance();

            if ((Globals.DtrDisplay.locationChanged || Globals.ReapplyPreset) && !Globals.Config.EditOverride) {
                string continent = "";
                string territory = "";
                string region = "";
                string subArea = "";

                Globals.DtrDisplay.locationChanged = false;

                if (Globals.DtrDisplay.currentContinent != null)
                {
                    continent = Globals.DtrDisplay.currentContinent.Name.RawString;
                }

                if (Globals.DtrDisplay.currentTerritory != null) {
                    territory = Globals.DtrDisplay.currentTerritory.Name.RawString;
                }

                if (Globals.DtrDisplay.currentRegion != null) {
                    region = Globals.DtrDisplay.currentRegion.Name.RawString;
                }

                if (Globals.DtrDisplay.currentSubArea != null) {
                    subArea = Globals.DtrDisplay.currentSubArea.Name.RawString;
                }

                Globals.Config.ApplyPresetByGuid(Globals.Config.GetZonePresetGUID(new string[] { continent, territory, region, subArea }));
            }

            Globals.Config.FixupZoneDefaultPresets();

            if (shadowManager != null && Globals.Config.Enabled) {
                shadowManager->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                shadowManager->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                shadowManager->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                shadowManager->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
            }
        }

        private void DrawUI()
        {
            Globals.Config.shared.mapPresets = SortConfigDictionaryAndChildren(Globals.Config.shared.mapPresets);
            WindowSystem.Draw();
            DrawPost();
        }

        public void ToggleConfig()
        {
            WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen = !WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen;
        }

        private Dictionary<string, ConfigTreeNode> SortConfigDictionaryAndChildren(Dictionary<string, ConfigTreeNode> dictionary) {
            Dictionary<string, ConfigTreeNode> result = dictionary.OrderBy(entry => entry.Key).ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var entry in result.Values) {
                if (entry.Children != null) {
                    entry.Children = SortConfigDictionaryAndChildren(entry.Children);
                }
            }

            return result;
        }

        [Command("/pbshadows")]
        [HelpMessage("Toggle configuration window")]
        public void Command_pbshadows(string command, string args)
        {
            ToggleConfig();
        }

        [Command("/tbshadows")]
        [HelpMessage("Toggle the functionality")]
        public void Command_togglebshadows(string command, string args)
        {
            Globals.Config.EnabledOverall = !Globals.Config.EnabledOverall;
            Globals.ToggleHacks();
            Globals.ToggleShadowmap();
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

            Globals.DtrDisplay.Dispose();

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
