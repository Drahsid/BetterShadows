using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BetterShadows
{
    public class CascadeConfig
    {
        public string Name = "";
        public float CascadeDistance0 = 72.0f;
        public float CascadeDistance1 = 144.0f;
        public float CascadeDistance2 = 432.0f;
        public float CascadeDistance3 = 3072.0f;

        public CascadeConfig() { }
        public CascadeConfig(CascadeConfig copy) {
            Name = copy.Name;
            CascadeDistance0 = copy.CascadeDistance0;
            CascadeDistance1 = copy.CascadeDistance1;
            CascadeDistance2 = copy.CascadeDistance2;
            CascadeDistance3 = copy.CascadeDistance3;
        }
        public CascadeConfig(string name, float cascadeDistance0, float cascadeDistance1, float cascadeDistance2, float cascadeDistance3)
        {
            Name = name;
            CascadeDistance0 = cascadeDistance0;
            CascadeDistance1 = cascadeDistance1;
            CascadeDistance2 = cascadeDistance2;
            CascadeDistance3 = cascadeDistance3;
        }
    }

    public class Configuration : IPluginConfiguration
    {
        int IPluginConfiguration.Version { get; set; }

        #region Saved configuration values
        public CascadeConfig cascades = new CascadeConfig();
        public List<CascadeConfig> cascadePresets = new List<CascadeConfig> {
            new CascadeConfig("Long Distance", 256, 768, 1536, 3072),
            new CascadeConfig("Balanced", 40, 116, 265, 2154),
            new CascadeConfig("Detailed", 13, 34, 64, 138),
            new CascadeConfig("Seamless", 28, 56, 112, 196),
            new CascadeConfig("Compromise", 72, 144, 432, 3072)
        };
        public bool HigherResShadowmap = true;
        public float SliderMax = 4096.0f;
        public bool Enabled = true;
        #endregion

        public bool ShowConfig = false;
        public string lastSelectedPreset = "";

        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pi) {
            this.pluginInterface = pi;

            if (Enabled)
            {
                CodeManager.DoEnableHacks();
            }

            if (HigherResShadowmap)
            {
                CodeManager.DoEnableShadowmap();
            }
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
