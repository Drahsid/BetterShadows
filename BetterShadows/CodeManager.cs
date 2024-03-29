﻿using Dalamud.Hooking;
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
    private static IntPtr Text_ShadowCascade0 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade1 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade2 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade3 = IntPtr.Zero;
    private static byte[] OriginalBytes_ShadowCascade0 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade1 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade2 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade3 = new byte[32];

    private static IntPtr Text_NearFarShadowmap0 = IntPtr.Zero;
    private static IntPtr Text_NearFarShadowmap1 = IntPtr.Zero;
    private static byte[] OriginalBytes_NearFarShadowmap0 = new byte[32];
    private static byte[] OriginalBytes_NearFarShadowmap1 = new byte[256];

    public static bool CascadeOverrideEnabled = false;
    public static bool ShadowmapOverrideEnabled = false;

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

    private static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowmapHook { get; set; } = null!;
    private static Hook<RenderTargetManager_InitializeShadowmapDelegate>? InitializeShadowmapNearFarHook { get; set; } = null!;

    private static unsafe byte InitializeShadowmap(RenderTargetManagerUpdated* thisx, SizeParam* size) {
        SizeParam* _size = stackalloc SizeParam[1];
        var option = ShadowManager->ShadowmapOption;
        var setting = Globals.Config.ShadowmapSettings[option];

        if (size == null) {
            size = _size;
            _size->Width = 0x200;
            _size->Height = 0x200;
            Service.Logger.Warning("InitializeShadowmap: size was null!");
        }

        switch (setting) {
            default:
            case ShadowmapResolution.RES_NONE:
                _size->Width = size->Width;
                _size->Height = size->Height;
                size = _size;
                break;
            case ShadowmapResolution.RES_64:
                size->Width = 64;
                size->Height = 64;
                break;
            case ShadowmapResolution.RES_128:
                size->Width = 128;
                size->Height = 128;
                break;
            case ShadowmapResolution.RES_256:
                size->Width = 256;
                size->Height = 256;
                break;
            case ShadowmapResolution.RES_512:
                size->Width = 512;
                size->Height = 512;
                break;
            case ShadowmapResolution.RES_1024:
                size->Width = 1024;
                size->Height = 1024;
                break;
            case ShadowmapResolution.RES_2048:
                size->Width = 2048;
                size->Height = 2048;
                break;
            case ShadowmapResolution.RES_4096:
                size->Width = 4096;
                size->Height = 4096;
                break;
            case ShadowmapResolution.RES_8192:
                size->Width = 8192;
                size->Height = 8192;
                break;
            case ShadowmapResolution.RES_16384:
                size->Width = 16384;
                size->Height = 16384;
                break;
        }

        var ret = InitializeShadowmapHook.Original(thisx, size);

        //InitializeShadowmapNearFarHook.Original(thisx, size);
        return ret;
    }

    private static unsafe byte InitializeShadowmapNearFar(RenderTargetManagerUpdated* thisx, SizeParam* size) {
        SizeParam* _size = stackalloc SizeParam[1];
        if (size == null) {
            size = _size;
            _size->Width = 0x200;
            _size->Height = 0x200;
            Service.Logger.Warning("InitializeShadowmapNearFar: size was null!");
        }

        return InitializeShadowmapNearFarHook.Original(thisx, size);
    }

    private static unsafe void InvokeInitializeShadowmapNearFar(int size) {
        SizeParam* _size = stackalloc SizeParam[1];
        _size->Width = size;
        _size->Height = size;
        //InitializeShadowmapNearFarHook.OriginalDisposeSafe(rtm, _size);
    }

    public static unsafe void EnableShadowCascadeOverride() {
        // if regalloc ever changes, these will fail; may be better to hijack the whole function
        Text_ShadowCascade0 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? F3 44 0F 5C EC");
        Text_ShadowCascade1 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? F3 41 0F 58 D8 F3 0F 11 57 ??");
        Text_ShadowCascade2 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? 48 8D 9F ?? ?? ?? ?? ?? ?? 00 00 00");
        Text_ShadowCascade3 = Service.SigScanner.ScanText("F3 44 0F 11 6F ?? 48 8B 05");

        BytecodeHelper.ReadWriteNops(Text_ShadowCascade0, ref OriginalBytes_ShadowCascade0, 5);
        BytecodeHelper.ReadWriteNops(Text_ShadowCascade1, ref OriginalBytes_ShadowCascade1, 5);
        BytecodeHelper.ReadWriteNops(Text_ShadowCascade2, ref OriginalBytes_ShadowCascade2, 5);
        BytecodeHelper.ReadWriteNops(Text_ShadowCascade3, ref OriginalBytes_ShadowCascade3, 6);

        CascadeOverrideEnabled = true;
        Service.Logger.Verbose($"CascadeOverrideEnabled: {CascadeOverrideEnabled}");
    }

    public static unsafe void EnableShadowmapOverride() {
        if (ShadowmapOverrideEnabled) {
            return;
        }

        /*Text_NearFarShadowmap0 = Service.SigScanner.ScanText("c1 e0 02 89 81 84 02 00 00 48 8b 89 20 01 00 00");
        BytecodeHelper.ReadWriteCode(Text_NearFarShadowmap0 - 15, ref OriginalBytes_NearFarShadowmap0, // do not << 2 NearShadowMap Height
            "90 90 90 90 90 90 90 90 90 8b 82 04 00 00 00 90 90 90 89 81 84 02 00 00"
        );

        Text_NearFarShadowmap1 = Service.SigScanner.ScanText("48 8b 4c 24 68 8b 01");
        BytecodeHelper.ReadWriteCode(Text_NearFarShadowmap1 + 7, ref OriginalBytes_NearFarShadowmap1, // Don't do a bunch of funky operations on FarShadowMap
            "90 90 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 90 90 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 41 8b 86 8c 02 00 00 c1 e8 02 41 89 86 8c 02 00 00 90 90 90 90 90 90 90 90 90 90" // something about the ratio fixes weird issues in PoTD, but idc, Square should just make it not one massive map
            //"90 90 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 90 90 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90"
        );*/

        var InitializeShadowmapHookPtr0 = Service.SigScanner.ScanText("48 89 5c 24 18 48 89 74 24 20 57 41 56 41 57 48 83 ec 30 48 8b 02 48 8b fa 48 89 81 78 02 00 00");
        var InitializeShadowmapHookPtr1 = Service.SigScanner.ScanText("48 89 54 24 10 53 41 54 41 55 41 56 41 57 48 83 ec 30 ?? ?? ?? ?? ?? ?? ?? 4c 8d a9 80 02 00 00 41 bc 04 00 00 00 4c 8b f1 45 8b fc 44 8b 80 fc 01 00 00 45 3b c4 48 8b 02 49 89 45 00 45 0f 42");
        if (InitializeShadowmapHookPtr0 != IntPtr.Zero) {
            InitializeShadowmapHook = Service.GameInteropProvider.HookFromAddress<RenderTargetManager_InitializeShadowmapDelegate>(InitializeShadowmapHookPtr0, InitializeShadowmap);
            InitializeShadowmapHook.Enable();
        }

        if (InitializeShadowmapHookPtr1 != IntPtr.Zero) {
            InitializeShadowmapNearFarHook = Service.GameInteropProvider.HookFromAddress<RenderTargetManager_InitializeShadowmapDelegate>(InitializeShadowmapHookPtr1, InitializeShadowmapNearFar);
            InitializeShadowmapNearFarHook.Enable();
        }

        ReinitializeShadowmap();

        ShadowmapOverrideEnabled = true;
        Service.Logger.Verbose($"ShadowmapOverrideEnabled: {ShadowmapOverrideEnabled}");
    }

    public static void DisableShadowCascadeOverride() {
        if (CascadeOverrideEnabled) {
            BytecodeHelper.RestoreNops(Text_ShadowCascade0, OriginalBytes_ShadowCascade0, 5);
            BytecodeHelper.RestoreNops(Text_ShadowCascade1, OriginalBytes_ShadowCascade1, 5);
            BytecodeHelper.RestoreNops(Text_ShadowCascade2, OriginalBytes_ShadowCascade2, 5);
            BytecodeHelper.RestoreNops(Text_ShadowCascade3, OriginalBytes_ShadowCascade3, 6);

            Text_ShadowCascade0 = IntPtr.Zero;
            Text_ShadowCascade1 = IntPtr.Zero;
            Text_ShadowCascade2 = IntPtr.Zero;
            Text_ShadowCascade3 = IntPtr.Zero;
        }

        CascadeOverrideEnabled = false;
        Service.Logger.Verbose($"CascadeOverrideEnabled: {CascadeOverrideEnabled}");
    }

    public static void DisableShadowmapOverride() {
        if (ShadowmapOverrideEnabled) {
            //BytecodeHelper.RestoreCode(Text_NearFarShadowmap0 - 15, OriginalBytes_NearFarShadowmap0);
            //BytecodeHelper.RestoreCode(Text_NearFarShadowmap1 + 7, OriginalBytes_NearFarShadowmap1);

            if (InitializeShadowmapHook != null) {
                if (!InitializeShadowmapHook.IsDisposed) {
                    if (InitializeShadowmapHook.IsEnabled) {
                    InitializeShadowmapHook.Disable();
                }
                    InitializeShadowmapHook.Dispose();
                }
            }

            if (InitializeShadowmapNearFarHook != null) {
                if (!InitializeShadowmapNearFarHook.IsDisposed) {
                    if (InitializeShadowmapNearFarHook.IsEnabled) {
                        InitializeShadowmapNearFarHook.Disable();
                        InvokeInitializeShadowmapNearFar(0x200); // revert to default
                    }
                    InitializeShadowmapNearFarHook.Dispose();
                }
            }

            ReinitializeShadowmap();
        }
        ShadowmapOverrideEnabled = false;
        Service.Logger.Verbose($"ShadowmapOverrideEnabled: {ShadowmapOverrideEnabled}");
    }

    public static unsafe void ReinitializeShadowmap() {
        ShadowManager->Unk_Bitfield |= 1; // reinitializes shadowmap next frame

        var option = ShadowManager->ShadowmapOption;
        if (Globals.Config.ShadowmapSettings[option] == ShadowmapResolution.RES_NONE) {
            InvokeInitializeShadowmapNearFar(0x200); // revert to default
        }
    }
}
