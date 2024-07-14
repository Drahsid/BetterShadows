using DrahsidLib;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct GraphicsConfig {
    [FieldOffset(0x0C)] public byte GammaCorrection;
    [FieldOffset(0x0F)] public byte ModelLOD0;
    [FieldOffset(0x10)] public byte ModelLOD1;
    [FieldOffset(0x12)] public byte ShadowModelLOD;
    [FieldOffset(0x17)] public byte ParallaxOcclusion;
    [FieldOffset(0x18)] public byte Tesselation;
    [FieldOffset(0x19)] public byte Jittering0;
    [FieldOffset(0x1A)] public byte Jittering1;
    [FieldOffset(0x1B)] public byte Glare1;
    [FieldOffset(0x20)] public byte Reflections;
    [FieldOffset(0x21)] public byte GlareWater; // water surface glare
    [FieldOffset(0x22)] public byte Grass;
    [FieldOffset(0x23)] public byte AmbientOcclusion;
    [FieldOffset(0x26)] public byte DepthOfField;
    [FieldOffset(0x28)] public byte MotionBlur;
    [FieldOffset(0x29)] public byte LimbDarkening;
    [FieldOffset(0x2C)] public byte AntiAliasing;
    [FieldOffset(0x13)] public byte ShadowDistantObjectLOD;
    [FieldOffset(0x31)] public byte AnisotropicFiltering; // 0 = 4x, 1 = 8x, 2 = 16x
    [FieldOffset(0x33)] public byte TransparentLightQuality;
    [FieldOffset(0x34)] public byte ShadowFilterFlags; // self/partymembers/other/enemies
    [FieldOffset(0x35)] public byte ShadowSofteningSetting; // 0 = Weak, 1 = Strong, 2 = Strongest
    [FieldOffset(0x36)] public byte ShadowmapOption;
    [FieldOffset(0x37)] public byte ShadowCascadeCount;
    [FieldOffset(0x40)] public byte CharacterLighting;
    [FieldOffset(0x41)] public byte MaxDynamicLightShadowCount; // "Cast Shadows" setting; 8 = Minimum, 14 = Normal, 20 = Maximum
    [FieldOffset(0x44)] public byte DynamicResolution;
    [FieldOffset(0x45)] public byte Unk_0x45; // setting to 0 with DLSS results in DLAA
    [FieldOffset(0x54)] public byte UpscaleMethod;
    [FieldOffset(0x56)] public byte GrassInteraction;

    private static GraphicsConfig* _instance = null;

    public static GraphicsConfig* Instance() {
        if (_instance == null) {
            IntPtr addr = Service.SigScanner.GetStaticAddressFromSig("48 8b ?? ?? ?? ?? ?? 80 79 ?? 02 0f ?? ?? ?? ?? ?? 8b");
            if (addr != IntPtr.Zero) {
                _instance = *((GraphicsConfig**)addr);
            }
        }

        return _instance;
    }
}

