using DrahsidLib;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

[StructLayout(LayoutKind.Explicit, Size = 0x3E0)]
public unsafe partial struct ShadowManager {
    [FieldOffset(0x0C)] public int ShadowSofteningSetting; // 0 = Weak, 1 = Strong, 2 = Stronger (not in config ui), 3 = Strongest. > 3 treated as Weak
    [FieldOffset(0x10)] public int Unk_0x10;
    [FieldOffset(0x14)] public int Unk_0x14;
    [FieldOffset(0x18)] public int ShadowmapOption; // 0 = 512, 1 = 1024, 2 = 2048
    [FieldOffset(0x1C)] public int ShadowCascadeCount0;
    [FieldOffset(0x20)] public int ShadowCascadeCount1;
    [FieldOffset(0x24)] public byte Unk_0x24;
    [FieldOffset(0x28)] public float Unk_0x28;
    [FieldOffset(0x2C)] public float Unk_0x2C;
    [FieldOffset(0x30)] public float NearDistance;
    [FieldOffset(0x34)] public float FarDistance;
    [FieldOffset(0x38)] public float Bias0; // float[4], used to weigh the cascade distances towards the near/far distance
    [FieldOffset(0x3C)] public float Bias1;
    [FieldOffset(0x40)] public float Bias2;
    [FieldOffset(0x44)] public float Bias3;
    [FieldOffset(0x48)] public float CascadeDistance0; // float[5]
    [FieldOffset(0x4C)] public float CascadeDistance1;
    [FieldOffset(0x50)] public float CascadeDistance2;
    [FieldOffset(0x54)] public float CascadeDistance3;
    [FieldOffset(0x58)] public float CascadeDistance4;
    [FieldOffset(0x5C)] public uint Unk_Bitfield; // != 0 resets shadowmap
    [FieldOffset(0x1E1)] public byte Unk_0x1E1;
    [FieldOffset(0x1E8)] public float ShadowapBlending; // Blends each cascade backwards by X units. Seams are visible at 0

    private static ShadowManager* _instance = null;

    public static ShadowManager* Instance() {
        if (_instance == null) {
            IntPtr addr = Service.SigScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 49 8B 0C 00");
            if (addr != IntPtr.Zero) {
                _instance = *((ShadowManager**)addr);
            }
        }

        return _instance;
    }
}

