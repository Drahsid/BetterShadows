using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BetterShadows
{
    public class CascadeConfig
    {
        public string Name = "";
        public Guid? GUID = Guid.Empty;
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

    public class ConfigTreeNode {
        public string Name;
        public bool Default;
        public Guid? Preset;
        public Dictionary<string, ConfigTreeNode> Children;

        public ConfigTreeNode(string name, Guid? preset, bool usingDefault) {
            Name = name;
            Default = usingDefault;
            Preset = preset;
            Children = new Dictionary<string, ConfigTreeNode>();
        }

        public ConfigTreeNode this[string childName] {
            get {
                if (Children.ContainsKey(childName)) {
                    return Children[childName];
                }
                else {
                    throw new KeyNotFoundException($"Child node with name {childName} not found.");
                }
            }
        }

        public void RecurseChildrenAndSetDefaultPreset(Guid? preset) {
            foreach (string key in Children.Keys) {
                if (Children[key].Default) {
                    Children[key].Preset = preset;
                }
                else {
                    preset= Children[key].Preset;
                }

                Children[key].RecurseChildrenAndSetDefaultPreset(preset);
            }
        }
    }


    public class Configuration : IPluginConfiguration
    {
        int IPluginConfiguration.Version { get; set; }

        #region Saved configuration values
        public CascadeConfig cascades = new CascadeConfig();
        public Guid? defaultPreset = null;
        public List<CascadeConfig> cascadePresets = null;
        public Dictionary<string, ConfigTreeNode> mapPresets;

        public bool HigherResShadowmap = true;
        public float SliderMax = 4096.0f;
        public bool Enabled = true;
        public bool EnabledOverall = true;
        public bool EditOverride = false;
        #endregion

        public string lastSelectedPreset = "";

        private DalamudPluginInterface pluginInterface;

        public Guid GetZonePresetGUID(string[] keys) {
            string key = keys[0];

            if (!mapPresets.ContainsKey(key)) {
                var newNode = new ConfigTreeNode(key, defaultPreset, true);
                mapPresets.Add(key, newNode);
            }
            if (keys[1] == "") {
                // (Guid)(node.Default ? defaultPreset : node.Preset);
                return (Guid)(mapPresets[key].Default ? defaultPreset : mapPresets[key].Preset);
            }

            return GetZonePresetGUID_(mapPresets[key], keys.Skip(1).ToArray(), (Guid)mapPresets[key].Preset);
        }

        private Guid GetZonePresetGUID_(ConfigTreeNode node, string[] keys, Guid parent) {
            // if we are the last key, or the next one is invalid
            if (keys.Length == 0 || keys[0] == "") {
                return (Guid)(node.Default ? parent : node.Preset);
            }
            if (!node.Children.ContainsKey(keys[0])) {
                var newNode = new ConfigTreeNode(keys[0], defaultPreset, true);
                node.Children.Add(keys[0], newNode);
            }

            if (node.Default) {
                node.Preset = parent;
            }

            return GetZonePresetGUID_(node.Children[keys[0]], keys.Skip(1).ToArray(), (Guid)(node.Default ? parent : node.Preset));
        }

        public void FixupZoneDefaultPresets() {
            foreach (string key in mapPresets.Keys) {
                if (mapPresets[key].Default) {
                    mapPresets[key].Preset = defaultPreset;
                }
                mapPresets[key].RecurseChildrenAndSetDefaultPreset(mapPresets[key].Preset);
            }
        }

        public void ApplyPresetByGuid(Guid presetGuid) {
            foreach (CascadeConfig cascadeConfig in cascadePresets) {
                if (cascadeConfig.GUID == presetGuid) {
                    cascades.CascadeDistance0 = cascadeConfig.CascadeDistance0;
                    cascades.CascadeDistance1 = cascadeConfig.CascadeDistance1;
                    cascades.CascadeDistance2 = cascadeConfig.CascadeDistance2;
                    cascades.CascadeDistance3 = cascadeConfig.CascadeDistance3;
                    cascades.Name = cascadeConfig.Name;
                    cascades.GUID = cascadeConfig.GUID;
                    break;
                }
            }
        }

        public void Initialize(DalamudPluginInterface pi) {
            this.pluginInterface = pi;

            if (cascadePresets == null)
            {
                cascadePresets = new List<CascadeConfig> {
                    new CascadeConfig("Long Distance", 256, 768, 1536, 3072),
                    new CascadeConfig("Balanced", 40, 116, 265, 2154),
                    new CascadeConfig("Detailed", 13, 34, 64, 138),
                    new CascadeConfig("Seamless", 28, 56, 112, 196),
                    new CascadeConfig("Compromise", 72, 144, 432, 3072)
                };
            }

            foreach (CascadeConfig c in cascadePresets) {
                if (c.GUID is null || c.GUID == Guid.Empty) {
                    c.GUID = Guid.NewGuid();
                }
            }

            if (defaultPreset is null) {
                defaultPreset = cascadePresets[3].GUID;
            }

            if (mapPresets is null) {
                mapPresets = new Dictionary<string, ConfigTreeNode>();
            }

            Save();

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
