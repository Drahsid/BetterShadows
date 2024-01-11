using DrahsidLib;
using Dalamud.Game.Command;

namespace BetterShadows;

internal static class Commands {
    public static void Initialize() {
        Service.CommandManager.AddHandler("/pbshadows", new CommandInfo(OnPBShadows)
        {
            ShowInHelp = true,
            HelpMessage = "Toggle the configuration window."
        });

        Service.CommandManager.AddHandler("/tbshadows", new CommandInfo(OnTBShadows)
        {
            ShowInHelp = true,
            HelpMessage = "Toggle the functionality."
        });

        Service.CommandManager.AddHandler("/bshedit", new CommandInfo(OnBSHEdit)
        {
            ShowInHelp = true,
            HelpMessage = "Open the preset editor window."
        });

        Service.CommandManager.AddHandler("/bshlist", new CommandInfo(OnBSHList)
        {
            ShowInHelp = true,
            HelpMessage = "Open the preset list window."
        });

        Service.CommandManager.AddHandler("/bshzone", new CommandInfo(OnBSHZone)
        {
            ShowInHelp = true,
            HelpMessage = "Open the zone editor window."
        });
    }

    public static void Dispose() {
        Service.CommandManager.RemoveHandler("/pbshadows");
        Service.CommandManager.RemoveHandler("/tbshadows");
        Service.CommandManager.RemoveHandler("/bshedit");
        Service.CommandManager.RemoveHandler("/bshlist");
        Service.CommandManager.RemoveHandler("/bshzone");
    }

    public static void ToggleConfig() {
        Windows.Config.IsOpen = !Windows.Config.IsOpen;
    }

    public static void OnPBShadows(string command, string args) {
        Windows.Config.IsOpen = !Windows.Config.IsOpen;
    }

    public static void OnTBShadows(string command, string args) {
        Globals.Config.EnabledOverall = !Globals.Config.EnabledOverall;

        if (Globals.Config.EnabledOverall) {
            Service.ChatGui.Print("Better Shadows Enabled"); 
        }
        else {
            Service.ChatGui.Print("Better Shadows Disabled");
        }

        Globals.ToggleHacks();
        Globals.ToggleShadowmap();
    }

    public static void OnBSHEdit(string command, string args) {
        Windows.Config.TogglePresetEditorPopout();
    }

    public static void OnBSHList(string command, string args) {
        Windows.Config.TogglePresetListPopout();
    }

    public static void OnBSHZone(string command, string args) {
        Windows.Config.TogglePresetZonePopout();
    }
}

