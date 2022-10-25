using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Runtime.InteropServices;

namespace BetterShadows
{
    public class Configuration : IPluginConfiguration
    {
        int IPluginConfiguration.Version { get; set; }

        #region Saved configuration values
        public float CascadeDistance0 = 256.0f;
        public float CascadeDistance1 = 768.0f;
        public float CascadeDistance2 = 1536.0f;
        public float CascadeDistance3 = 3072.0f;
        public float SliderMax = 4096.0f;
        public bool Enabled = true;
        #endregion

        public bool ShowConfig = false;

        private readonly DalamudPluginInterface pluginInterface;

        public Configuration(DalamudPluginInterface pi)
        {
            this.pluginInterface = pi;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
