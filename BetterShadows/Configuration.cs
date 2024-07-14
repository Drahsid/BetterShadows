using Dalamud.Configuration;
using DrahsidLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterShadows;

public enum ShadowmapResolution {
    RES_NONE, // No override
    RES_64,
    RES_128,
    RES_256,
    RES_512,
    RES_1024,
    RES_2048,
    RES_4096,
    RES_8192,
    RES_16384,
    RES_COUNT
}

public class CascadeConfig {
    public string Name = "";
    public Guid? GUID = Guid.Empty;
    public float CascadeDistance0 = 50.0f;
    public float CascadeDistance1 = 120.0f;
    public float CascadeDistance2 = 300.0f;
    public float CascadeDistance3 = 652.0f;
    public float CascadeDistance4 = 1300.0f;

    public CascadeConfig() { }
    public CascadeConfig(CascadeConfig copy) {
        Name = copy.Name;
        CascadeDistance0 = copy.CascadeDistance0;
        CascadeDistance1 = copy.CascadeDistance1;
        CascadeDistance2 = copy.CascadeDistance2;
        CascadeDistance3 = copy.CascadeDistance3;
        CascadeDistance4 = copy.CascadeDistance4;
    }
    public CascadeConfig(string name, float cascadeDistance0, float cascadeDistance1, float cascadeDistance2, float cascadeDistance3, float cascadeDistance4) {
        Name = name;
        CascadeDistance0 = cascadeDistance0;
        CascadeDistance1 = cascadeDistance1;
        CascadeDistance2 = cascadeDistance2;
        CascadeDistance3 = cascadeDistance3;
        CascadeDistance4 = cascadeDistance4;
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
    public SharableData shared = new SharableData();

    // this is just here for config upgrades
    [Obsolete] public List<CascadeConfig>? cascadePresets = null;
    [Obsolete] public bool HigherResShadowmap = true;

    #region Debug
    public bool Debug = false;
    public bool SuperDebug = false;

    public int ForceMapX = 0;
    public int ForceMapY = 0;
    public int ForceNearMapX = 0;
    public int ForceNearMapY = 0;
    public int ForceFarMapX = 0;
    public int ForceFarMapY = 0;
    public int ForceDistanceMapX = 0;
    public int ForceDistanceMapY = 0;
    #endregion

    public ShadowmapResolution[] ShadowMapGlobalSettings = { ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_4096 };
    public ShadowmapResolution[] ShadowMapNearSettings = { ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_4096 };
    public ShadowmapResolution[] ShadowMapFarSettings = { ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_4096 };
    public ShadowmapResolution[] ShadowMapDistanceSettings = { ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_NONE, ShadowmapResolution.RES_4096 };
    public float SliderMax = 9000.0f;
    public bool Enabled = true;
    public bool EnabledOverall = true;
    public bool EditOverride = false;
    public bool ZoneConfigBeforePreset = false;
    public bool HideTooltips = false;
    public bool ShowContinent = true;
    public bool OpenInGPose = true;
    public bool MaintainGameAspect = true;
    public bool DynamicCascadeMode = true;
    #endregion

    public string lastSelectedPreset = "";

    private readonly List<CascadeConfig> defaultCascadePresets = new List<CascadeConfig> {
        new CascadeConfig("Seamless (4k)", 28, 56, 112, 196, 250),
        new CascadeConfig("Long Distance (4k)", 256, 768, 1536, 2500, 3072),
        new CascadeConfig("Balanced (4k)", 40, 116, 265, 500, 2154),
        new CascadeConfig("Detailed (4k)", 13, 34, 64, 138, 210),
        new CascadeConfig("Compromise (4k)", 72, 144, 432, 1000, 3072),
        new CascadeConfig("Long Distance (16k)", 320, 853, 1920, 4053, 8320),
        new CascadeConfig("Detailed (16k)", 52, 136, 256, 552, 1920),
        new CascadeConfig("Compromise (16k)", 160, 426, 960, 1920, 2154),
    };

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

    public void RecoverStockPresets() {
        foreach (var stock in defaultCascadePresets) {
            bool recover = true;
            stock.GUID = Guid.NewGuid();
            foreach (var preset in shared.cascadePresets) {
                if (   preset.CascadeDistance0 == stock.CascadeDistance0
                    && preset.CascadeDistance1 == stock.CascadeDistance1
                    && preset.CascadeDistance2 == stock.CascadeDistance2
                    && preset.CascadeDistance3 == stock.CascadeDistance3) {
                    recover = false;
                    preset.Name = stock.Name;
                    Service.ChatGui.Print($"Stock Preset {stock.Name} does not need to be recovered!");
                    break;
                }

                if (preset.Name == stock.Name) {
                    // Settings are not identical at this point
                    preset.Name = $"{preset.Name}* [{preset.GUID}]";
                    Service.ChatGui.Print($"Stock Preset {stock.Name} was modified. Renamed to \"{preset.Name}* [{preset.GUID}]\"");
                }
            }

            if (recover) {
                shared.cascadePresets.Add(stock);
                Service.ChatGui.Print($"Stock Preset {stock.Name} recovered!");
            }
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
        if (shared == null) {
            shared = new SharableData();
            if (cascadePresets is not null) {
                shared.cascadePresets = new List<CascadeConfig>();
                foreach (CascadeConfig c in cascadePresets) {
                    shared.cascadePresets.Add(c);
                    Service.Logger.Info($"Added {c.Name}");
                }
                cascadePresets = null;
            }
        }

        if (shared.cascadePresets is null) {
            shared.cascadePresets = defaultCascadePresets;
        }

        foreach (CascadeConfig c in shared.cascadePresets) {
            if (c.GUID is null || c.GUID == Guid.Empty) {
                c.GUID = Guid.NewGuid();
            }

            // deal with 7.0 upgrade
            if ((float?)c.CascadeDistance4 is null)
            {
                c.CascadeDistance4 = c.CascadeDistance3 + 10;
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
            CodeManager.EnableShadowCascadeOverride();
        }
    }

    public void Save() {
        Service.Interface.SavePluginConfig(this);
    }
}
