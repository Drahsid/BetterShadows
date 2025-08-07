using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using DrahsidLib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace BetterShadows;

[StructLayout(LayoutKind.Explicit, Size = 0x730)]
public unsafe partial struct RenderTargetManagerUpdated {
    public static RenderTargetManagerUpdated* Instance() => (RenderTargetManagerUpdated*)FFXIVClientStructs.FFXIV.Client.Graphics.Render.RenderTargetManager.Instance();
    
    [FieldOffset(0x120)] public Texture* ShadowMapTexture0;
    [FieldOffset(0x128)] public Texture* ShadowMapTexture1;
    [FieldOffset(0x130)] public Texture* ShadowMapTexture2;
    [FieldOffset(0x138)] public Texture* ShadowMapTexture3;

    [FieldOffset(0x158)] public Texture* ShadowMapTextureNear;
    [FieldOffset(0x160)] public Texture* ShadowMapTexture_Near0; // These are initialized in a loop
    [FieldOffset(0x168)] public Texture* ShadowMapTexture_Near1;
    [FieldOffset(0x170)] public Texture* ShadowMapTexture_Near2;
    [FieldOffset(0x178)] public Texture* ShadowMapTexture_Near3;

    [FieldOffset(0x180)] public Texture* ShadowMapTextureFar;
    [FieldOffset(0x188)] public Texture* ShadowMapTexture_Far00; // These are initialized in a loop
    [FieldOffset(0x190)] public Texture* ShadowMapTexture_Far01;
    [FieldOffset(0x198)] public Texture* ShadowMapTexture_Far02;
    [FieldOffset(0x1A0)] public Texture* ShadowMapTexture_Far03;
    [FieldOffset(0x1A8)] public Texture* ShadowMapTexture_Far04;
    [FieldOffset(0x1B0)] public Texture* ShadowMapTexture_Far05;
    [FieldOffset(0x1B8)] public Texture* ShadowMapTexture_Far06;
    [FieldOffset(0x1C0)] public Texture* ShadowMapTexture_Far07;
    [FieldOffset(0x1C8)] public Texture* ShadowMapTexture_Far08;
    [FieldOffset(0x1D0)] public Texture* ShadowMapTexture_Far09;
    [FieldOffset(0x1D8)] public Texture* ShadowMapTexture_Far0A;
    [FieldOffset(0x1E0)] public Texture* ShadowMapTexture_Far0B;
    [FieldOffset(0x1E8)] public Texture* ShadowMapTexture_Far0C;
    [FieldOffset(0x1F0)] public Texture* ShadowMapTexture_Far0D;
    [FieldOffset(0x1F8)] public Texture* ShadowMapTexture_Far0E;
    [FieldOffset(0x200)] public Texture* ShadowMapTexture_Far0F;
    [FieldOffset(0x208)] public Texture* ShadowMapTexture_Far10;
    [FieldOffset(0x210)] public Texture* ShadowMapTexture_Far11;
    [FieldOffset(0x218)] public Texture* ShadowMapTexture_Far12;
    [FieldOffset(0x220)] public Texture* ShadowMapTexture_Far13;

    [FieldOffset(0x228)] public Texture* ShadowMapTextureDistance;
    [FieldOffset(0x230)] public Texture* ShadowMapTexture_Distance0;
    [FieldOffset(0x238)] public Texture* ShadowMapTexture_Distance1;
    [FieldOffset(0x240)] public Texture* ShadowMapTexture_Distance2;
    [FieldOffset(0x248)] public Texture* ShadowMapTexture_Distance3;

    [FieldOffset(0x428)] public int Resolution_Width;
    [FieldOffset(0x42C)] public int Resolution_Height;
    [FieldOffset(0x430)] public int ShadowMap_Width;
    [FieldOffset(0x434)] public int ShadowMap_Height;
    [FieldOffset(0x438)] public int NearShadowMap_Width;
    [FieldOffset(0x43C)] public int NearShadowMap_Height;
    [FieldOffset(0x440)] public int FarShadowMap_Width;
    [FieldOffset(0x444)] public int FarShadowMap_Height;
    [FieldOffset(0x448)] public int DistanceShadowMap_Width;
    [FieldOffset(0x44C)] public int DistanceShadowMap_Height;
}
