using Dalamud.Configuration;
using DrahsidLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterShadows;

public class CascadeConfig {
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
    public CascadeConfig(string name, float cascadeDistance0, float cascadeDistance1, float cascadeDistance2, float cascadeDistance3) {
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

public class SharableData {
    public Guid? defaultPreset = null;
    public List<CascadeConfig> cascadePresets = null;
    public Dictionary<string, ConfigTreeNode> mapPresets;

    public string ToBase64() {
        string json = JsonConvert.SerializeObject(this);
        byte[] bytes = Encoding.Default.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public static SharableData FromBase64(string base64Text) {
        byte[] bytes = Convert.FromBase64String(base64Text);
        string json = Encoding.Default.GetString(bytes);
        return JsonConvert.DeserializeObject<SharableData>(json);
    }
}


public class Configuration : IPluginConfiguration {
    int IPluginConfiguration.Version { get; set; }

    #region Saved configuration values
    public CascadeConfig cascades = new CascadeConfig();
    public SharableData shared;

    // this is just here for config upgrades
    [Obsolete] public List<CascadeConfig> cascadePresets = null;

    public bool HigherResShadowmap = true;
    public float SliderMax = 4096.0f;
    public bool Enabled = true;
    public bool EnabledOverall = true;
    public bool EditOverride = false;
    public bool ZoneConfigBeforePreset = false;
    public bool HideTooltips = false;
    public bool ShowContinent = true;
    #endregion

    public string lastSelectedPreset = "";

    private List<CascadeConfig> defaultCascadePresets = new List<CascadeConfig> {
        new CascadeConfig("Seamless", 28, 56, 112, 196),
        new CascadeConfig("Long Distance", 256, 768, 1536, 3072),
        new CascadeConfig("Balanced", 40, 116, 265, 2154),
        new CascadeConfig("Detailed", 13, 34, 64, 138),
        new CascadeConfig("Compromise", 72, 144, 432, 3072)
    };

    public Configuration() {
        if (shared == null) {
            shared = new SharableData();
            if (cascadePresets != null) {
                shared.cascadePresets = new List<CascadeConfig>();
                foreach (CascadeConfig c in cascadePresets) {
                    shared.cascadePresets.Add(c);
                }
                cascadePresets = null;
            }
        }
    }

    public Guid GetZonePresetGUID(string[] keys) {
        string key = keys[0];

        if (!shared.mapPresets.ContainsKey(key)) {
            var newNode = new ConfigTreeNode(key, shared.defaultPreset, true);
            shared.mapPresets.Add(key, newNode);
        }
        if (keys[1] == "") {
            // (Guid)(node.Default ? defaultPreset : node.Preset);
            return (Guid)(shared.mapPresets[key].Default ? shared.defaultPreset : shared.mapPresets[key].Preset);
        }

        return GetZonePresetGUID_(shared.mapPresets[key], keys.Skip(1).ToArray(), (Guid)shared.mapPresets[key].Preset);
    }

    private Guid GetZonePresetGUID_(ConfigTreeNode node, string[] keys, Guid parent) {
        // if we are the last key, or the next one is invalid
        if (keys.Length == 0 || keys[0] == "") {
            return (Guid)(node.Default ? parent : node.Preset);
        }
        if (!node.Children.ContainsKey(keys[0])) {
            var newNode = new ConfigTreeNode(keys[0], shared.defaultPreset, true);
            node.Children.Add(keys[0], newNode);
        }

        if (node.Default) {
            node.Preset = parent;
        }

        return GetZonePresetGUID_(node.Children[keys[0]], keys.Skip(1).ToArray(), (Guid)(node.Default ? parent : node.Preset));
    }

    public void FixupZoneDefaultPresets() {
        foreach (string key in shared.mapPresets.Keys) {
            if (shared.mapPresets[key].Default) {
                shared.mapPresets[key].Preset = shared.defaultPreset;
            }
            shared.mapPresets[key].RecurseChildrenAndSetDefaultPreset(shared.mapPresets[key].Preset);
        }
    }

    public void ApplyPresetByGuid(Guid presetGuid) {
        foreach (CascadeConfig cascadeConfig in shared.cascadePresets) {
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

    public void Initialize() {
        if (shared.cascadePresets == null) {
            shared.cascadePresets = defaultCascadePresets;
        }

        foreach (CascadeConfig c in shared.cascadePresets) {
            if (c.GUID is null || c.GUID == Guid.Empty) {
                c.GUID = Guid.NewGuid();
            }
        }

        if (shared.defaultPreset is null) {
            if (shared.cascadePresets.Count == 0) {
                shared.cascadePresets = defaultCascadePresets;
            }
            
            shared.defaultPreset = shared.cascadePresets[0].GUID;
        }

        if (shared.mapPresets is null) {
            shared.mapPresets = new Dictionary<string, ConfigTreeNode>();
        }

        Save();

        if (Enabled) {
            CodeManager.DoEnableHacks();
        }

        if (HigherResShadowmap) {
            CodeManager.DoEnableShadowmap();
        }
    }

    public void Save() {
        Service.Interface.SavePluginConfig(this);
    }
}
