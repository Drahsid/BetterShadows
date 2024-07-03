using Dalamud.Hooking;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

[StructLayout(LayoutKind.Sequential)]
internal struct SizeParam {
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
    private unsafe delegate byte RenderTargetManager_InitializeShadowmapDelegate(RenderTargetManagerUpdated* thisx, SizeParam* size);

    private unsafe delegate void ShadowManager_UpdateCascadeValuesDelegate(ShadowManager* thisx, float unk1);

    private static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowMapHook { get; set; } = null!;
    private static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowMapNearFarHook { get; set; } = null!;

    private static Hook<ShadowManager_UpdateCascadeValuesDelegate>? UpdateCascadeValuesHook { get; set; } = null!;

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

        if (Globals.Config.MaintainGameAspect)
        {
            height = Math.Min(16384, width * 5);
        }

        // if debug and axis != 0
        if (Globals.Config.Debug)
        {
            if (Globals.Config.ForceMapX != 0)
            {
                width = Globals.Config.ForceMapX;
            }

            if (Globals.Config.ForceMapY != 0)
            {
                height = Globals.Config.ForceMapY;
            }
        }

        if (RenderTargetManagerUpdated.InitializeShadowmap(thisx, width, height) != 0)
        {
            thisx->ShadowMap_Width = width;
            thisx->ShadowMap_Height = height;
            return 1;
        }
        else
        {
            Service.Logger.Error("InitializeShadowmap failed?");
            return InitializeShadowMapHook.Original(thisx, _size);
        }
    }

    private static unsafe byte InitializeShadowmapNearFar(RenderTargetManagerUpdated* thisx, SizeParam* _size) {
        var option = ShadowManager->ShadowmapOption;
        SizeParam size_near = GetShadowmapSettingSize(Globals.Config.ShadowMapNearSettings[option], _size);
        SizeParam size_far = GetShadowmapSettingSize(Globals.Config.ShadowMapFarSettings[option], _size);
        SizeParam size_dist = GetShadowmapSettingSize(Globals.Config.ShadowMapDistanceSettings[option], _size);

        int width_near = size_near.Width;
        int height_near = size_near.Height;

        int width_far = size_far.Width;
        int height_far = size_far.Height;

        int width_dist = size_dist.Width;
        int height_dist = size_dist.Height;

        if (Globals.Config.MaintainGameAspect)
        {
            width_near = Math.Min(16384, width_near * 2);
            height_near = Math.Min(16384, height_near * 2);
            width_dist = Math.Min(16384, width_dist * 4);
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

        byte near = RenderTargetManagerUpdated.InitializeNearShadowmap(thisx, width_near, height_near);
        byte far = RenderTargetManagerUpdated.InitializeFarShadowmap(thisx, width_far, height_far);
        byte dist = RenderTargetManagerUpdated.InitializeDistanceShadowmap(thisx, width_dist, height_dist);

        if (near != 0 && far != 0 && dist != 0)
        {
            thisx->NearShadowMap_Width = width_near;
            thisx->NearShadowMap_Height = height_near;

            thisx->FarShadowMap_Width = width_far;
            thisx->FarShadowMap_Height = height_far;

            thisx->DistanceShadowMap_Width = width_dist;
            thisx->DistanceShadowMap_Height = height_dist;

            return 1;
        }
        else
        {
            Service.Logger.Error($"InitializeShadowmapNearFar failed? {near}, {far}, {dist}");
            return InitializeShadowMapNearFarHook.Original(thisx, _size);
        }
    }

    private static unsafe void UpdateCascadeValues(ShadowManager* thisx, float unk1)
    {
        if (UpdateCascadeValuesHook == null || UpdateCascadeValuesHook.IsDisposed)
        {
            return;
        }

        UpdateCascadeValuesHook.Original(thisx, unk1);

        // a change in location or preset occured
        if ((Globals.DtrDisplay.locationChanged || Globals.ReapplyPreset) && !Globals.Config.EditOverride)
        {
            string continent = "";
            string territory = "";
            string region = "";
            string subArea = "";

            Globals.DtrDisplay.locationChanged = false;

            if (Globals.DtrDisplay.currentContinent != null)
            {
                continent = Globals.DtrDisplay.currentContinent.Name.RawString;
            }

            if (Globals.DtrDisplay.currentTerritory != null)
            {
                territory = Globals.DtrDisplay.currentTerritory.Name.RawString;
            }

            if (Globals.DtrDisplay.currentRegion != null)
            {
                region = Globals.DtrDisplay.currentRegion.Name.RawString;
            }

            if (Globals.DtrDisplay.currentSubArea != null)
            {
                subArea = Globals.DtrDisplay.currentSubArea.Name.RawString;
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

        CascadeOverrideEnabled = true;
        Service.Logger.Verbose($"CascadeOverrideEnabled: {CascadeOverrideEnabled}");
    }

    public static unsafe void EnableShadowMapOverride() {
        if (ShadowMapOverrideEnabled) {
            return;
        }

        // 48 89 5c 24 10 48 89 74 24 18 57 41 56 41 57 48 83 ec 30 48 8b 02 48 8b fa 48 89 81 ?? ?? 00 00
        var InitializeShadowMapHookPtr0 = Service.SigScanner.ScanText("e8 ?? ?? ?? 00 84 c0 0f 84 ?? 00 00 00 8b 4f");
        var InitializeShadowMapHookPtr1 = Service.SigScanner.ScanText("e8 ?? ?? ?? 00 84 c0 74 53 33 d2 48 8d 8f");
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
