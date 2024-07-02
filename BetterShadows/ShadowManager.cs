using DrahsidLib;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

[StructLayout(LayoutKind.Explicit, Size = 0x3E0)]
public unsafe partial struct ShadowManager {
    [FieldOffset(0x18)] public int ShadowmapOption;
    [FieldOffset(0x20)] public int Unk_0x20;
    [FieldOffset(0x24)] public byte Unk_0x24;
    [FieldOffset(0x30)] public float NearDistance;
    [FieldOffset(0x34)] public float FarDistance;
    [FieldOffset(0x38)] public float Bias0;
    [FieldOffset(0x3C)] public float Bias1;
    [FieldOffset(0x40)] public float Bias2;
    [FieldOffset(0x48)] public float CascadeDistance0;
    [FieldOffset(0x4C)] public float CascadeDistance1;
    [FieldOffset(0x50)] public float CascadeDistance2;
    [FieldOffset(0x54)] public float CascadeDistance3;
    [FieldOffset(0x58)] public float CascadeDistance4;
    [FieldOffset(0x5C)] public uint Unk_Bitfield;

    public static ShadowManager* Instance() {
        IntPtr addr = Service.SigScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 49 8B 0C 00");
        if (addr != IntPtr.Zero) {
            ShadowManager* ret = *((ShadowManager**)addr);
            return *((ShadowManager**)addr);
        }
        else {
            return null;
        }
    }
}

