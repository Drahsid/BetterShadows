using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System;


namespace BetterShadows;

public class Plugin : IDalamudPlugin {
    private IDalamudPluginInterface PluginInterface;
    private IChatGui Chat { get; init; }
    private IClientState ClientState { get; init; }
    private ICommandManager CommandManager { get; init; }

    private bool WasGPosing = false;
    private int LastSoftSetting = 0;

    public string Name => "Better Shadows";

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager, IChatGui chat, IClientState clientState) {
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

    private void DrawUI() {
         if (Service.ClientState.IsGPosing != WasGPosing && Service.ClientState.IsGPosing == true) {
            if (Globals.Config.OpenInGPose) {
                Windows.Config.IsOpen = true;
            }
        }

        WasGPosing = Service.ClientState.IsGPosing;

        Windows.System.Draw();

        unsafe
        {
            var shadows = ShadowManager.Instance();
            if (shadows != null) {
                if (shadows->ShadowSofteningSetting != LastSoftSetting) {
                    CodeManager.ReinitializeShadowMap();
                    LastSoftSetting = shadows->ShadowSofteningSetting;
                }
            }

            if (Globals.Config.ShadowMapCombatOverride != ShadowmapResolution.RES_NONE)
            {
                var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
                if (rtm != null)
                {
                    bool combat_flag = Service.Condition[ConditionFlag.InCombat];
                    ShadowmapOverlord.SetCombat(combat_flag);
                }
            }
        }

        if (Globals.Config.EnabledOverall) {
            unsafe {
                var shadowManager = CodeManager.ShadowManager;
                var option = CodeManager.ShadowManager->ShadowmapOption;
                if (CodeManager.ShadowMapOverrideEnabled && Globals.Config.ShadowMapGlobalSettings[option] == ShadowmapResolution.RES_NONE) {
                    CodeManager.DisableShadowMapOverride();
                }
                else if (CodeManager.ShadowMapOverrideEnabled == false && Globals.Config.ShadowMapGlobalSettings[option] != ShadowmapResolution.RES_NONE) {
                    CodeManager.EnableShadowMapOverride();
                }
            }
        }
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        Globals.DtrDisplay.Dispose();

        Globals.Config.Save();

        PluginInterface.UiBuilder.Draw -= DrawUI;
        Windows.Dispose();
        PluginInterface.UiBuilder.OpenConfigUi -= Commands.ToggleConfig;

        Commands.Dispose();

        ShadowmapOverlord.Dispose();

        if (CodeManager.CascadeOverrideEnabled) {
            CodeManager.DisableShadowCascadeOverride();
        }

        if (CodeManager.ShadowMapOverrideEnabled) {
            CodeManager.DisableShadowMapOverride();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
