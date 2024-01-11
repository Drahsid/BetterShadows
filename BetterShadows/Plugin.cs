using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BetterShadows;

public class Plugin : IDalamudPlugin {
    private DalamudPluginInterface PluginInterface;
    private IChatGui Chat { get; init; }
    private IClientState ClientState { get; init; }
    private ICommandManager CommandManager { get; init; }

    private bool WasGPosing = false;

    public string Name => "Better Shadows";

    public Plugin(DalamudPluginInterface pluginInterface, ICommandManager commandManager, IChatGui chat, IClientState clientState) {
        PluginInterface = pluginInterface;
        Chat = chat;
        ClientState = clientState;
        CommandManager = commandManager;

        DrahsidLib.DrahsidLib.Initialize(pluginInterface, ConfigWindowHelpers.DrawTooltip);

        InitializeCommands();
        InitializeConfig();
        InitializeUI();

        Globals.DtrDisplay = new DtrDisplay();
    }

    private void InitializeCommands() {
        Commands.Initialize();
    }

    private void InitializeConfig() {
        Globals.Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Globals.Config.Initialize();
    }

    private void InitializeUI() {
        Windows.Initialize();
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += Commands.ToggleConfig;
    }

    private unsafe void DrawPost() {
        if ((Globals.DtrDisplay.locationChanged || Globals.ReapplyPreset) && !Globals.Config.EditOverride) {
            ShadowManager* shadowManager = ShadowManager.Instance();
            string continent = "";
            string territory = "";
            string region = "";
            string subArea = "";

            Globals.DtrDisplay.locationChanged = false;

            if (Globals.DtrDisplay.currentContinent != null) {
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

    private void DrawUI() {
         if (Service.ClientState.IsGPosing != WasGPosing && Service.ClientState.IsGPosing == true) {
            if (Globals.Config.OpenInGPose) {
                Windows.Config.IsOpen = true;
            }
        }

        WasGPosing = Service.ClientState.IsGPosing;

        Windows.System.Draw();
        DrawPost();

        if (Globals.Config.EnabledOverall) {
             unsafe {
                 var option = CodeManager.ShadowManager->ShadowmapOption;
                 if (CodeManager.ShadowmapOverrideEnabled && Globals.Config.ShadowmapSettings[option] == ShadowmapResolution.RES_NONE) {
                     CodeManager.DisableShadowmapOverride();
                 }
                 else if (CodeManager.ShadowmapOverrideEnabled == false && Globals.Config.ShadowmapSettings[option] != ShadowmapResolution.RES_NONE) {
                     CodeManager.EnableShadowmapOverride();
                 }
             }
        }
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

    

    #region IDisposable Support
    protected virtual void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        Globals.DtrDisplay.Dispose();

        PluginInterface.SavePluginConfig(Globals.Config);

        PluginInterface.UiBuilder.Draw -= DrawUI;
        Windows.Dispose();
        PluginInterface.UiBuilder.OpenConfigUi -= Commands.ToggleConfig;

        Commands.Dispose();

        if (CodeManager.CascadeOverrideEnabled) {
            CodeManager.DisableShadowCascadeOverride();
        }

        if (CodeManager.ShadowmapOverrideEnabled) {
            CodeManager.DisableShadowmapOverride();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
