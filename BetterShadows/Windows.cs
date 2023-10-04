using Dalamud.Interface.Windowing;

namespace BetterShadows;

internal static class Windows {
    public static WindowSystem System { get; set; } = null!;
    public static ConfigWindow Config { get; set; } = null!;

    public static void Initialize() {
        System = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
        Config = new ConfigWindow();
        System.AddWindow(Config);
    }

    public static void Dispose() {
        Config?.Dispose();
        System.RemoveAllWindows();
    }
}


