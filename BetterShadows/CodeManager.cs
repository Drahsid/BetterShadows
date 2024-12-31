using Dalamud.Hooking;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.System.String.Utf8String.Delegates;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

namespace BetterShadows;

[StructLayout(LayoutKind.Sequential)]
public struct SizeParam {
    public int Width;
    public int Height;
}

internal static class CodeManager {
    private static IntPtr Text_ShadowCascade = IntPtr.Zero;
    private static IntPtr Text_NearFarShadowmap0 = IntPtr.Zero;
    private static IntPtr Text_NearFarShadowmap1 = IntPtr.Zero;

    public static bool CascadeOverrideEnabled = false;
    public static bool ShadowMapOverrideEnabled = false;

    private static unsafe RenderTargetManagerUpdated* _rtm = null;
    private static unsafe RenderTargetManagerUpdated* rtm {
        get {
            if (_rtm == null) {
                _rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
            }
            return _rtm;
        }
    }

    private static unsafe ShadowManager* _shadowManager = null;
    public static unsafe ShadowManager* ShadowManager {
        get {
            if (_shadowManager == null) {
                _shadowManager = BetterShadows.ShadowManager.Instance();
            }
            return _shadowManager;
        }
    }


    [return: MarshalAs(UnmanagedType.U1)]
    public unsafe delegate byte RenderTargetManager_InitializeShadowmapDelegate(RenderTargetManagerUpdated* thisx, SizeParam* size);

    private unsafe delegate void ShadowManager_UpdateCascadeValuesDelegate(ShadowManager* thisx, float unk1);

    private unsafe delegate void Unk_ShadowSofteningInitDelegate(IntPtr unk1, IntPtr unk2);

    public static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowMapHook { get; set; } = null!;
    public static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowMapNearFarHook { get; set; } = null!;

    private static Hook<ShadowManager_UpdateCascadeValuesDelegate>? UpdateCascadeValuesHook { get; set; } = null!;
    private static Hook<Unk_ShadowSofteningInitDelegate>? ShadowSofteningInitHook { get; set; } = null!;

    private static int LastShadowmapWidth = 2048;
    private static int LastShadowmapHeight = 10240;

    private unsafe static void ShadowSofteningInit(IntPtr unk1, IntPtr unk2) {
        rtm->ShadowMap_Width = 2048;
        rtm->ShadowMap_Height = 10240;
        ShadowSofteningInitHook.Original(unk1, unk2);
        rtm->ShadowMap_Width = LastShadowmapWidth;
        rtm->ShadowMap_Height = LastShadowmapHeight;
    }

    private unsafe static SizeParam GetShadowmapSettingSize(ShadowmapResolution setting, SizeParam* _size)
    {
        SizeParam sizeXY = new SizeParam();
        int size = 1024;
        bool set = true;

        switch (setting)
        {
            default:
            case ShadowmapResolution.RES_NONE:
                sizeXY.Width = _size->Width;
                sizeXY.Height = _size->Height;
                set = false;
                break;
            case ShadowmapResolution.RES_64:
                size = 64;
                break;
            case ShadowmapResolution.RES_128:
                size = 128;
                break;
            case ShadowmapResolution.RES_256:
                size = 256;
                break;
            case ShadowmapResolution.RES_512:
                size = 512;
                break;
            case ShadowmapResolution.RES_1024:
                size = 1024;
                break;
            case ShadowmapResolution.RES_2048:
                size = 2048;
                break;
            case ShadowmapResolution.RES_4096:
                size = 4096;
                break;
            case ShadowmapResolution.RES_8192:
                size = 8192;
                break;
            case ShadowmapResolution.RES_16384:
                size = 16384;
                break;
        }

        if (set)
        {
            sizeXY.Width = size;
            sizeXY.Height = size;
        }

        return sizeXY;
    }

    private static unsafe byte InitializeShadowmap(RenderTargetManagerUpdated* thisx, SizeParam* _size) {
        var option = ShadowManager->ShadowmapOption;
        var setting = Globals.Config.ShadowMapGlobalSettings[option];
        SizeParam sizeXY = GetShadowmapSettingSize(setting, _size);
        int width = sizeXY.Width;
        int height = sizeXY.Height;

        SizeParam _combat = GetShadowmapSettingSize(Globals.Config.ShadowMapCombatOverride, _size);
        int combat_width = _combat.Width;
        int combat_height = _combat.Height;

        // Fix strange behavior with strongest shadow softening by forcing the 1:5 shadowmap ratio
        if (ShadowManager->ShadowSofteningSetting == 3)
        {
            width = height = Math.Min(3276, width);
            height *= 5;

            combat_width = combat_height = Math.Min(3276, combat_width);
            combat_height *= 5;
        }
        else if (Globals.Config.MaintainGameAspect)
        {
            height *= 5;
            combat_height *= 5;
        }

        if (Globals.Config.ForceMapX != 0)
        {
            width = Globals.Config.ForceMapX;
        }

        if (Globals.Config.ForceMapY != 0)
        {
            height = Globals.Config.ForceMapY;
        }

        width = Math.Max(0, Math.Min(16384, width));
        height = Math.Max(0, Math.Min(16384, height));

        combat_width = Math.Max(0, Math.Min(16384, combat_width));
        combat_height = Math.Max(0, Math.Min(16384, combat_height));

        if (ShadowmapOverlord.OverlordInitialized0 == false)
        {
            ShadowmapOverlord.OverlordInitializeGlobal(width / 2, height / 2);
            InitializeShadowmapNearFar(thisx, _size); // make sure overlord gets fully initialized
        }

        ShadowmapOverlord.WasInCombat = false;
        if (ShadowmapOverlord.SetGlobalTextureSize(width, height, combat_width, combat_height, Globals.Config.ShadowMapCombatOverride != ShadowmapResolution.RES_NONE))
        {
            thisx->ShadowMap_Width = width;
            thisx->ShadowMap_Height = height;
            LastShadowmapWidth = width;
            LastShadowmapHeight = height;
            return 1;
        }
        else
        {
            Service.Logger.Error("global failed?");
            var ret = InitializeShadowMapHook.Original(thisx, _size);
            LastShadowmapWidth = rtm->ShadowMap_Width;
            LastShadowmapHeight = rtm->ShadowMap_Height;
            return ret;
        }
    }

    private static unsafe byte InitializeShadowmapNearFar(RenderTargetManagerUpdated* thisx, SizeParam* _size)
    {
        var option = ShadowManager->ShadowmapOption;
        SizeParam size_near = GetShadowmapSettingSize(Globals.Config.ShadowMapNearSettings[option], _size);
        SizeParam size_far = GetShadowmapSettingSize(Globals.Config.ShadowMapFarSettings[option], _size);
        SizeParam size_dist = GetShadowmapSettingSize(Globals.Config.ShadowMapDistanceSettings[option], _size);

        SizeParam size_global = GetShadowmapSettingSize(Globals.Config.ShadowMapGlobalSettings[option], _size);
        SizeParam _combat = GetShadowmapSettingSize(Globals.Config.ShadowMapCombatOverride, _size);
        int combat_nwidth = _combat.Width;
        int combat_nheight = _combat.Height;
        int combat_fwidth = _combat.Width;
        int combat_fheight = _combat.Height;
        int combat_dwidth = _combat.Width;
        int combat_dheight = _combat.Height;

        int width_near = size_near.Width;
        int height_near = size_near.Height;

        int width_far = size_far.Width;
        int height_far = size_far.Height;

        int width_dist = size_dist.Width;
        int height_dist = size_dist.Height;

        if (Globals.Config.MaintainGameAspect)
        {
            width_near *= 2;
            height_near *= 2;
            width_dist *= 4;

            combat_nwidth *= 2;
            combat_nheight *= 2;
            combat_dwidth *= 4;
        }

        width_near = Math.Max(0, Math.Min(16384, width_near));
        height_near = Math.Max(0, Math.Min(16384, height_near));

        width_far = Math.Max(0, Math.Min(16384, width_far));
        height_far = Math.Max(0, Math.Min(16384, height_far));

        width_dist = Math.Max(0, Math.Min(16384, width_dist));
        height_dist = Math.Max(0, Math.Min(16384, height_dist));

        combat_nwidth = Math.Max(0, Math.Min(16384, combat_nwidth));
        combat_nheight = Math.Max(0, Math.Min(16384, combat_nheight));
        combat_fwidth = Math.Max(0, Math.Min(16384, combat_fwidth));
        combat_fheight = Math.Max(0, Math.Min(16384, combat_fheight));
        combat_dwidth = Math.Max(0, Math.Min(16384, combat_dwidth));
        combat_dheight = Math.Max(0, Math.Min(16384, combat_dheight));

        if (Globals.Config.MaintainGameAspect)
        {
            height_dist = Math.Max(0, Math.Min(4096, height_dist));
            combat_dheight = Math.Max(0, Math.Min(4096, combat_dheight));
        }

        if (Globals.Config.Debug)
        {
            if (Globals.Config.ForceNearMapX != 0)
            {
                width_near = Globals.Config.ForceNearMapX;
            }

            if (Globals.Config.ForceNearMapY != 0)
            {
                height_near = Globals.Config.ForceNearMapY;
            }

            if (Globals.Config.ForceFarMapX != 0)
            {
                width_far = Globals.Config.ForceFarMapX;
            }

            if (Globals.Config.ForceFarMapY != 0)
            {
                height_far = Globals.Config.ForceFarMapY;
            }

            if (Globals.Config.ForceDistanceMapX != 0)
            {
                width_dist = Globals.Config.ForceDistanceMapX;
            }

            if (Globals.Config.ForceDistanceMapY != 0)
            {
                height_dist = Globals.Config.ForceDistanceMapY;
            }
        }

        if (ShadowmapOverlord.OverlordInitialized1 == false)
        {
            ShadowmapOverlord.OverlordInitializeNFD(width_near / 2, height_near / 2, width_far / 2, height_far / 2, width_dist / 2, height_dist / 2);
        }

        ShadowmapOverlord.WasInCombat = false;

        bool near = ShadowmapOverlord.SetNearTextureSize(width_near, height_near, combat_nwidth, combat_nheight);
        bool far = ShadowmapOverlord.SetFarTextureSize(width_far, height_far, combat_fwidth, combat_fheight);
        bool dist = ShadowmapOverlord.SetDistanceTextureSize(width_dist, height_dist, combat_dwidth, combat_dheight);
        if (near && far && dist)
        {
            return 1;
        }
        else
        {
            Service.Logger.Error($"InitializeShadowmapNearFar failed? {near}, {far}, {dist}");
            return InitializeShadowMapNearFarHook.Original(thisx, _size);
        }
    }

    private static unsafe void UpdateCascadeValues_Manual(ShadowManager* thisx) {
        // a change in location or preset occured
        if ((Globals.DtrDisplay.locationChanged || Globals.ReapplyPreset) && !Globals.Config.EditOverride) {
            string continent = "";
            string territory = "";
            string region = "";
            string subArea = "";

            Globals.DtrDisplay.locationChanged = false;

            if (Globals.DtrDisplay.currentContinent != null)
            {
                continent = Globals.DtrDisplay.currentContinent.Value.Name.ExtractText();
            }

            if (Globals.DtrDisplay.currentTerritory != null)
            {
                territory = Globals.DtrDisplay.currentTerritory.Value.Name.ExtractText();
            }

            if (Globals.DtrDisplay.currentRegion != null)
            {
                region = Globals.DtrDisplay.currentRegion.Value.Name.ExtractText();
            }

            if (Globals.DtrDisplay.currentSubArea != null)
            {
                subArea = Globals.DtrDisplay.currentSubArea.Value.Name.ExtractText();
            }

            Globals.Config.ApplyPresetByGuid(Globals.Config.GetZonePresetGUID(new string[] { continent, territory, region, subArea }));

            if (Globals.Config.Enabled)
            {
                thisx->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                thisx->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                thisx->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                thisx->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
                thisx->CascadeDistance4 = Globals.Config.cascades.CascadeDistance4;
            }

            Globals.Config.FixupZoneDefaultPresets();
            Globals.Config.shared.mapPresets = Globals.SortConfigDictionaryAndChildren(Globals.Config.shared.mapPresets);
        }

        // use edit override if it is toggled
        if (Globals.Config.EditOverride)
        {
            if (Globals.Config.Enabled)
            {
                thisx->CascadeDistance0 = Globals.Config.cascades.CascadeDistance0;
                thisx->CascadeDistance1 = Globals.Config.cascades.CascadeDistance1;
                thisx->CascadeDistance2 = Globals.Config.cascades.CascadeDistance2;
                thisx->CascadeDistance3 = Globals.Config.cascades.CascadeDistance3;
                thisx->CascadeDistance4 = Globals.Config.cascades.CascadeDistance4;
            }
        }
    }

    private static unsafe void UpdateCascadeValues_Dynamic(ShadowManager* thisx) {
        var _rtm = RenderTargetManager.Instance();
        RenderTargetManagerUpdated* rtm = (RenderTargetManagerUpdated*)_rtm;

        const UInt64 sizeDefault = 2048 * 10240;
        UInt64 size = (UInt64)rtm->ShadowMap_Width * (UInt64)rtm->ShadowMap_Height;
        if (size > sizeDefault)
        {
            float _ratio = MathF.Log(size / sizeDefault, 2);
            float ratio = MathF.Max(1.0f, 1.0f + 0.5f * Math.Clamp(_ratio, 0, 3));
            thisx->CascadeDistance0 *= ratio;
            thisx->CascadeDistance1 *= ratio;
            thisx->CascadeDistance2 *= ratio;
            thisx->CascadeDistance3 *= ratio;
            thisx->CascadeDistance4 *= ratio;
            thisx->ShadowapBlending = 3 * MathF.Max(1.0f, _ratio / 2);
        }
    }

    private static unsafe void UpdateCascadeValues(ShadowManager* thisx, float zoom) {
        if (UpdateCascadeValuesHook == null || UpdateCascadeValuesHook.IsDisposed)
        {
            return;
        }

        if (Globals.Config.DynamicCascadeMode)
        {
            var _rtm = RenderTargetManager.Instance();
            RenderTargetManagerUpdated* rtm = (RenderTargetManagerUpdated*)_rtm;

            const UInt64 sizeDefault = 2048;
            UInt64 size = (UInt64)rtm->ShadowMap_Width;
            if (size > sizeDefault)
            {
                float ratio = size / sizeDefault;
                zoom /= ratio;
            }
        }

        UpdateCascadeValuesHook.Original(thisx, zoom);

        if (Globals.Config.DynamicCascadeMode)
        {
            UpdateCascadeValues_Dynamic(thisx);
        }
        else {
            UpdateCascadeValues_Manual(thisx);
        }
    }

    public static unsafe void EnableShadowCascadeOverride() {
        if (CascadeOverrideEnabled) {
            return;
        }

        var ShadowCascadePtr = Service.SigScanner.ScanText("e8 ?? ?? ?? ?? 48 8b ?? ?? ?? ?? ?? 0f 28 d6 48 8b d3 e8 ?? ?? ?? ?? 0f");
        if (ShadowCascadePtr != IntPtr.Zero)
        {
            UpdateCascadeValuesHook = Service.GameInteropProvider.HookFromAddress<ShadowManager_UpdateCascadeValuesDelegate>(ShadowCascadePtr, UpdateCascadeValues);
            UpdateCascadeValuesHook.Enable();
        }

        /*var ShadowSoftPtr = Service.SigScanner.ScanText("48 8b c4 48 89 58 08 48 89 70 10 48 89 78 18 55 41 54 41 55 41 56 41 57 48 8d ?? ?? ?? ?? ?? 48 81 ?? 00 02 00 00"); // TODO
        if (ShadowSoftPtr != 0)
        {
            ShadowSofteningInitHook = Service.GameInteropProvider.HookFromAddress<Unk_ShadowSofteningInitDelegate>(ShadowSoftPtr, ShadowSofteningInit);
            ShadowSofteningInitHook.Enable();
        }*/

        CascadeOverrideEnabled = true;
        Service.Logger.Verbose($"CascadeOverrideEnabled: {CascadeOverrideEnabled}");
    }

    public static unsafe void EnableShadowMapOverride() {
        if (ShadowMapOverrideEnabled) {
            return;
        }

        // 48 89 5c 24 10 48 89 74 24 18 57 41 56 41 57 48 83 ec 30 48 8b 02 48 8b fa 48 89 81 ?? ?? 00 00
        var InitializeShadowMapHookPtr0 = Service.SigScanner.ScanText("e8 ?? ?? ?? 00 84 c0 0f 84 ?? 00 00 00 8b 4f");
        var InitializeShadowMapHookPtr1 = Service.SigScanner.ScanText("e8 ?? ?? ?? 00 84 c0 74 ?? 33 d2 48 8d 8f");
        var aaa = Service.SigScanner.ScanText("4c 8b d9 0f b6 d2 49 b9 01 01 01 01 01 01 01 01");

        if (InitializeShadowMapHookPtr0 != IntPtr.Zero) {
            InitializeShadowMapHook = Service.GameInteropProvider.HookFromAddress<RenderTargetManager_InitializeShadowmapDelegate>(InitializeShadowMapHookPtr0, InitializeShadowmap);
            InitializeShadowMapHook.Enable();
        }

        if (InitializeShadowMapHookPtr1 != IntPtr.Zero) {
            InitializeShadowMapNearFarHook = Service.GameInteropProvider.HookFromAddress<RenderTargetManager_InitializeShadowmapDelegate>(InitializeShadowMapHookPtr1, InitializeShadowmapNearFar);
            InitializeShadowMapNearFarHook.Enable();
        }

        ReinitializeShadowMap();

        ShadowMapOverrideEnabled = true;
        Service.Logger.Verbose($"ShadowMapOverrideEnabled: {ShadowMapOverrideEnabled}");
    }

    public static void DisableShadowCascadeOverride() {
        if (CascadeOverrideEnabled)
        {
            if (UpdateCascadeValuesHook != null)
            {
                if (UpdateCascadeValuesHook.IsEnabled)
                {
                    UpdateCascadeValuesHook.Disable();
                }

                if (!UpdateCascadeValuesHook.IsDisposed)
                {
                    UpdateCascadeValuesHook.Dispose();
                }
            }
        }

        /*if (ShadowSofteningInitHook != null) {
            ShadowSofteningInitHook.Disable();
            ShadowSofteningInitHook.Dispose();
        }*/

        CascadeOverrideEnabled = false;
        Service.Logger.Verbose($"CascadeOverrideEnabled: {CascadeOverrideEnabled}");
    }

    public static void DisableShadowMapOverride() {
        if (ShadowMapOverrideEnabled)
        {
            if (InitializeShadowMapHook != null) {
                if (InitializeShadowMapHook.IsEnabled)
                {
                    InitializeShadowMapHook.Disable();
                }

                if (!InitializeShadowMapHook.IsDisposed)
                {
                    InitializeShadowMapHook.Dispose();
                }
            }

            if (InitializeShadowMapNearFarHook != null)
            {
                if (InitializeShadowMapNearFarHook.IsEnabled)
                {
                    InitializeShadowMapNearFarHook.Disable();
                }

                if (!InitializeShadowMapNearFarHook.IsDisposed)
                {
                    InitializeShadowMapNearFarHook.Dispose();
                }
            }

            ReinitializeShadowMap();
        }

        ShadowMapOverrideEnabled = false;
        Service.Logger.Verbose($"ShadowMapOverrideEnabled: {ShadowMapOverrideEnabled}");
    }

    public static unsafe void ReinitializeShadowMap() {
        if (ShadowManager != null)
        {
            ShadowManager->Unk_Bitfield |= 1; // reinitializes shadowmap next frame
        }
    }
}
