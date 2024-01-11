using System.Runtime.InteropServices;

namespace BetterShadows;

[StructLayout(LayoutKind.Explicit, Size = 0x4C8)]
public unsafe partial struct RenderTargetManagerUpdated {
    [FieldOffset(0x270)] public uint Resolution_Width;
    [FieldOffset(0x274)] public uint Resolution_Height;
    [FieldOffset(0x278)] public uint ShadowMap_Width;
    [FieldOffset(0x27C)] public uint ShadowMap_Height;
    [FieldOffset(0x280)] public uint NearShadowMap_Width;
    [FieldOffset(0x284)] public uint NearShadowMap_Height;
    [FieldOffset(0x288)] public uint FarShadowMap_Width;
    [FieldOffset(0x28C)] public uint FarShadowMap_Height;
}
