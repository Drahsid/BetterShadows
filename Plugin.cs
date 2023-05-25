using BetterShadows.Attributes;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Reflection.AssemblyVersion("1.1.5")]

namespace BetterShadows;

public class Plugin : IDalamudPlugin
{
    private DalamudPluginInterface PluginInterface;
    private ChatGui Chat;
    private ClientState ClientState;
    private PluginCommandManager<Plugin> CommandManager;
    private ConfigWindow ConfigWnd;
    
    public string Name => "Better Shadows";

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chat, ClientState clientState)
    {
        PluginInterface = pluginInterface;
        Chat = chat;
        ClientState = clientState;

        // Initialize the UI
        Globals.WindowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
        ConfigWnd = new ConfigWindow(this);
        Globals.WindowSystem.AddWindow(ConfigWnd);

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
        if ((Globals.DtrDisplay.locationChanged || Globals.ReapplyPreset) && !Globals.Config.EditOverride) {
            ShadowManager* shadowManager = ShadowManager.Instance();
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

            if (shadowManager != null && Globals.Config.Enabled) {
                shadowManager->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                shadowManager->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                shadowManager->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                shadowManager->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
            }

            Globals.Config.FixupZoneDefaultPresets();
            Globals.Config.shared.mapPresets = SortConfigDictionaryAndChildren(Globals.Config.shared.mapPresets);
        }

        if (Globals.Config.EditOverride) {
            ShadowManager* shadowManager = ShadowManager.Instance();
            if (shadowManager != null && Globals.Config.Enabled) {
                shadowManager->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                shadowManager->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                shadowManager->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                shadowManager->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
            }
        }
    }

    private void DrawUI()
    {
        Globals.WindowSystem.Draw();
        DrawPost();
    }

    public void ToggleConfig()
    {
        ConfigWnd.IsOpen = !ConfigWnd.IsOpen;
    }

    private Dictionary<string, ConfigTreeNode> SortConfigDictionaryAndChildren(Dictionary<string, ConfigTreeNode> dictionary) {
        Dictionary<string, ConfigTreeNode> result = dictionary.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(entry => entry.Key, entry => entry.Value);
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

        if (Globals.Config.EnabledOverall) {
            CodeManager.DoDisableHacks();
            CodeManager.DoDisableShadowmap();
        }
        else {
            if (Globals.Config.Enabled) {
                CodeManager.DoEnableHacks();
            }

            if (Globals.Config.HigherResShadowmap) {
                CodeManager.DoEnableShadowmap();
            }
        }
    }

    [Command("/bshedit")]
    [HelpMessage("Open the preset editor window")]
    public void Command_toggleeditor(string command, string args) {
        ConfigWnd.TogglePresetEditorPopout();
    }

    [Command("/bshlist")]
    [HelpMessage("Open the preset list window")]
    public void Command_togglelist(string command, string args) {
        ConfigWnd.TogglePresetListPopout();
    }

    [Command("/bshzone")]
    [HelpMessage("Open the zone editor window")]
    public void Command_togglezone(string command, string args) {
        ConfigWnd.TogglePresetZonePopout();
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

        Globals.WindowSystem.RemoveWindow(ConfigWnd);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
