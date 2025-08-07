using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System;
using DrahsidLib;
using Dalamud.Game.ClientState.Conditions;

namespace BetterShadows;

public unsafe struct ShadowmapPointer
{
    public Texture* ShadowMapTexture;
    public Texture* ShadowMapTexture_Combat;
    public TextureFormat TextureFormat;
    public TextureFlags Flags;
}

public unsafe class ShadowmapGroup
{
    public enum MapGroup
    {
        Global = 0,
        Near,
        Far,
        Distance
    }

    public MapGroup Group = MapGroup.Global;
    public bool Allocated = false;
    public bool EnableCombat = false;
    public bool UsingBaseTextures = false;
    public int Count = 0;
    public ShadowmapPointer[]? Shadowmaps = null;
    public SizeParam WidthHeight = new SizeParam();
    public SizeParam WidthHeight_Combat = new SizeParam();
    public string Tag = "";
    public Texture** TexturePointer;

    private const int SAFETY_FRAMES = 4;

    public ShadowmapGroup(int count, string tag, MapGroup group)
    {
        Allocated = false;
        Group = group;
        Count = count;
        Shadowmaps = new ShadowmapPointer[count];
        WidthHeight = new SizeParam();
        WidthHeight_Combat = new SizeParam();

        WidthHeight.Width = 2048;
        WidthHeight.Height = 2048;
        WidthHeight_Combat.Width = 2048;
        WidthHeight_Combat.Height = 2048;

        for (int index = 0; index < Count; index++)
        {
            Shadowmaps[index].ShadowMapTexture = (Texture*)IntPtr.Zero;
            Shadowmaps[index].ShadowMapTexture_Combat = (Texture*)IntPtr.Zero;
            Shadowmaps[index].TextureFormat = (TextureFormat)0x5100;
            Shadowmaps[index].Flags = TextureFlags.TextureRenderTarget;
        }

        Tag = tag;
    }

    public void SetTexturePointer(Texture** texturePointer)
    {
        TexturePointer = texturePointer;
    }

    public bool AllocateTextures()
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            Service.Logger.Error($"AllocateTextures: rtm null");
            return false;
        }

        if (Shadowmaps == null || Count == 0)
        {
            Service.Logger.Warning($"AllocateTextures: Shadowmap group {Tag} has no textures!");
            return false;
        }

        if (TexturePointer == null)
        {
            Service.Logger.Warning($"AllocateTextures: Shadowmap group {Tag} does not point to a texture table!");
            return false;
        }

        bool error = false;
        Texture*[] textures = new Texture*[Count];
        Texture*[] textures_combat = new Texture*[Count];

        SizeParam _width_height = new SizeParam();
        int* width_height = (int*)(&_width_height);
        _width_height.Width = WidthHeight.Width;
        _width_height.Height = WidthHeight.Height;

        for (int index = 0; index < Count; index++)
        {
            Service.Logger.Verbose($"[{index}] CreateTexture2D: {width_height[0]}, {width_height[1]}, 1, {Shadowmaps[index].TextureFormat:X}, {Shadowmaps[index].Flags:X}, 3");
            textures[index] = Device.Instance()->CreateTexture2D(width_height, 1, Shadowmaps[index].TextureFormat, Shadowmaps[index].Flags, 3);
            if (textures[index] == null)
            {
                Service.Logger.Error($"AllocateTextures: Failed to allocate base texture for {Tag} @ index {index}");
                error = true;
                break;
            }
        }

        if (EnableCombat)
        {
            width_height[0] = WidthHeight_Combat.Width;
            width_height[1] = WidthHeight_Combat.Height;
            for (int index = 0; index < Count; index++)
            {
                if (error)
                {
                    break;
                }

                textures_combat[index] = Device.Instance()->CreateTexture2D(width_height, 1, Shadowmaps[index].TextureFormat, Shadowmaps[index].Flags, 3);
                if (textures_combat[index] == null)
                {
                    Service.Logger.Error($"AllocateTextures: Failed to allocate combat texture for {Tag} @ index {index}");
                    error = true;
                    break;
                }
            }
        }
        else
        {
            // combat support was toggled, erase existing combat textures
            for (int index = 0; index < Count; index++)
            {
                if (Shadowmaps[index].ShadowMapTexture_Combat != null)
                {
                    Shadowmaps[index].ShadowMapTexture_Combat->DecRef();
                    Shadowmaps[index].ShadowMapTexture_Combat = null;
                }
            }
        }

        if (error) {
            Service.Logger.Verbose("There was an error...");
            for (int index = 0; index < Count; index++)
            {
                Service.Logger.Verbose($"[${index}] Attempting to erase texture...");
                if (textures[index] != null)
                {
                    textures[index]->DecRef();
                }

                Service.Logger.Verbose($"[${index}, combat] Attempting to erase texture...");
                if (textures_combat[index] != null)
                {
                    textures_combat[index]->DecRef();
                }
            }

            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            Service.Logger.Verbose($"[${index}] Attempting to erase texture...");
            if (TexturePointer[index] != null)
            {
                TexturePointer[index]->DecRef();
            }

            Service.Logger.Verbose("Setting texture pointer");
            TexturePointer[index] = textures[index];
            Service.Logger.Verbose("Setting ShadowMapTexture");
            Shadowmaps[index].ShadowMapTexture = textures[index];

            if (EnableCombat)
            {
                if (Service.Condition[ConditionFlag.InCombat])
                {
                    TexturePointer[index] = textures_combat[index];
                }
                Shadowmaps[index].ShadowMapTexture_Combat = textures_combat[index];
            }
        }

        SizeParam rez = WidthHeight;
        if (EnableCombat && Service.Condition[ConditionFlag.InCombat])
        {
            rez = WidthHeight_Combat;
        }

        Service.Logger.Verbose("Setting rez");
        switch (Group)
        {
            default:
            case MapGroup.Global:
                rtm->ShadowMap_Width = rez.Width;
                rtm->ShadowMap_Height = rez.Height;
                break;
            case MapGroup.Near:
                rtm->NearShadowMap_Width = rez.Width;
                rtm->NearShadowMap_Height = rez.Height;
                break;
            case MapGroup.Far:
                rtm->FarShadowMap_Width = rez.Width;
                rtm->FarShadowMap_Height = rez.Height;
                break;
            case MapGroup.Distance:
                rtm->DistanceShadowMap_Width = rez.Width;
                rtm->DistanceShadowMap_Height = rez.Height;
                break;
        }

        ShadowmapOverlord.InitializedFrames = SAFETY_FRAMES;
        UsingBaseTextures = true;
        Allocated = true;

        return true;
    }

    public void UnallocateTextures(bool allocnew)
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            Service.Logger.Error($"UnallocateTextures: rtm null");
            return;
        }

        var shadows = ShadowManager.Instance();
        if (shadows == null)
        {
            Service.Logger.Error($"UnallocateTextures: ShadowManager null");
            return;
        }

        if (Allocated == false || Shadowmaps == null || Count == 0)
        {
            Service.Logger.Error($"UnallocateTextures: Shadowmap group {Tag} has no textures!");
            return;
        }

        for (int index = 0; index < Count; index++)
        {
            if (Shadowmaps[index].ShadowMapTexture != null)
            {
                Shadowmaps[index].ShadowMapTexture->DecRef();
                Shadowmaps[index].ShadowMapTexture = null;
            }

            if (Shadowmaps[index].ShadowMapTexture_Combat != null)
            {
                Shadowmaps[index].ShadowMapTexture_Combat->DecRef();
                Shadowmaps[index].ShadowMapTexture_Combat = null;
            }

            if (TexturePointer != null)
            {
                if (TexturePointer[index] != null)
                {
                    TexturePointer[index]->DecRef();
                    TexturePointer[index] = null;
                }
            }
        }

        if (allocnew)
        {
            var option = shadows->ShadowmapOption;
            int* width_height = stackalloc int[2];
            if (Group == MapGroup.Global)
            {
                switch (option)
                {
                    default:
                    case 2:
                        width_height[0] = 2048;
                        width_height[1] = 10240;
                        break;
                    case 1:
                        width_height[0] = 2048;
                        width_height[1] = 5120;
                        break;
                    case 0:
                        width_height[0] = 512;
                        width_height[1] = 2560;
                        break;
                }

                if (CodeManager.InitializeShadowMapHook != null && !CodeManager.InitializeShadowMapHook.IsDisposed)
                {
                    CodeManager.InitializeShadowMapHook?.Original(rtm, (SizeParam*)width_height);
                }
            }
            else
            {
                switch (option)
                {
                    default:
                    case 2:
                        width_height[0] = 2048;
                        width_height[1] = 2048;
                        break;
                    case 1:
                    case 0:
                        width_height[0] = 1024;
                        width_height[1] = 1024;
                        break;
                }

                CodeManager.InitializeShadowMapNearFarHook?.Original(rtm, (SizeParam*)width_height);
            }
        }

        ShadowmapOverlord.InitializedFrames = SAFETY_FRAMES;
        Allocated = false;
    }

    public bool EnableCombatTextures()
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            Service.Logger.Error($"EnableCombatTextures: rtm null");
            return false;
        }

        if (EnableCombat == false || Allocated == false)
        {
            return false;
        }

        if (Shadowmaps == null || Count == 0)
        {
            Service.Logger.Error($"EnableCombatTextures: no textures");
            return false;
        }

        if (!UsingBaseTextures)
        {
            return true;
        }

        for (int index = 0; index < Count; index++)
        {
            if (Shadowmaps[index].ShadowMapTexture_Combat != null)
            {
                TexturePointer[index] = Shadowmaps[index].ShadowMapTexture_Combat;
            }
            else
            {
                Service.Logger.Warning($"Combat Texture {index} for {Tag} null?");
            }
        }

        switch (Group)
        {
            default:
            case MapGroup.Global:
                rtm->ShadowMap_Width = WidthHeight_Combat.Width;
                rtm->ShadowMap_Height = WidthHeight_Combat.Height;
                break;
            case MapGroup.Near:
                rtm->NearShadowMap_Width = WidthHeight_Combat.Width;
                rtm->NearShadowMap_Height = WidthHeight_Combat.Height;
                break;
            case MapGroup.Far:
                rtm->FarShadowMap_Width = WidthHeight_Combat.Width;
                rtm->FarShadowMap_Height = WidthHeight_Combat.Height;
                break;
            case MapGroup.Distance:
                rtm->DistanceShadowMap_Width = WidthHeight_Combat.Width;
                rtm->DistanceShadowMap_Height = WidthHeight_Combat.Height;
                break;
        }

        ShadowmapOverlord.InitializedFrames = SAFETY_FRAMES;
        UsingBaseTextures = false;

        return true;
    }

    public bool EnableBaseTextures()
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            Service.Logger.Error($"EnableBaseTextures: rtm null");
            return false;
        }

        if (Allocated == false)
        {
            Service.Logger.Error($"EnableBaseTextures: textures not allocated");
            return false;
        }

        if (Shadowmaps == null || Count == 0)
        {
            Service.Logger.Error($"EnableBaseTextures: no textures");
            return false;
        }

        if (UsingBaseTextures)
        {
            return true;
        }

        for (int index = 0; index < Count; index++)
        {
            if (Shadowmaps[index].ShadowMapTexture != null)
            {
                TexturePointer[index] = Shadowmaps[index].ShadowMapTexture;
            }
            else
            {
                Service.Logger.Warning($"Base Texture {index} for {Tag} null?");
            }
        }

        switch (Group)
        {
            default:
            case MapGroup.Global:
                rtm->ShadowMap_Width = WidthHeight.Width;
                rtm->ShadowMap_Height = WidthHeight.Height;
                break;
            case MapGroup.Near:
                rtm->NearShadowMap_Width = WidthHeight.Width;
                rtm->NearShadowMap_Height = WidthHeight.Height;
                break;
            case MapGroup.Far:
                rtm->FarShadowMap_Width = WidthHeight.Width;
                rtm->FarShadowMap_Height = WidthHeight.Height;
                break;
            case MapGroup.Distance:
                rtm->DistanceShadowMap_Width = WidthHeight.Width;
                rtm->DistanceShadowMap_Height = WidthHeight.Height;
                break;
        }

        ShadowmapOverlord.InitializedFrames = SAFETY_FRAMES;
        UsingBaseTextures = true;

        return true;
    }
}

public static class ShadowmapOverlord
{
    public static bool WasInCombat = false;
    private static ShadowmapGroup GlobalShadowmaps = new ShadowmapGroup(4, "Global", ShadowmapGroup.MapGroup.Global);
    private static ShadowmapGroup NearShadowmaps = new ShadowmapGroup(5, "Near", ShadowmapGroup.MapGroup.Near);
    private static ShadowmapGroup FarShadowmaps = new ShadowmapGroup(20, "Far", ShadowmapGroup.MapGroup.Far);
    private static ShadowmapGroup DistanceShadowmaps = new ShadowmapGroup(5, "Distance", ShadowmapGroup.MapGroup.Distance);

    public static bool OverlordInitialized0 = false;
    public static bool OverlordInitialized1 = false;
    public static bool OverlordInitialized { get { return OverlordInitialized0 && OverlordInitialized1; } }
    public static bool AllowCombat = false;
    public static int InitializedFrames = 0;
    

    public static void OverlordInitializeGlobal(int globalWidth, int globalHeight)
    {
        if (OverlordInitialized0)
        {
            return;
        }

        GlobalShadowmaps.WidthHeight.Width = globalWidth;
        GlobalShadowmaps.WidthHeight.Height = globalHeight;
        GlobalShadowmaps.WidthHeight_Combat.Width = globalWidth;
        GlobalShadowmaps.WidthHeight_Combat.Height = globalHeight;

        if (GlobalShadowmaps.Shadowmaps != null)
        {
            GlobalShadowmaps.Shadowmaps[1].TextureFormat = (TextureFormat)0x5140;
            GlobalShadowmaps.Shadowmaps[1].Flags = TextureFlags.TextureDepthStencil;
            GlobalShadowmaps.Shadowmaps[3].TextureFormat = (TextureFormat)0x5140;
            GlobalShadowmaps.Shadowmaps[3].Flags = TextureFlags.TextureDepthStencil;
        }

        unsafe
        {
            var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
            if (rtm == null)
            {
                return;
            }

            GlobalShadowmaps.SetTexturePointer(&rtm->ShadowMapTexture0);
        }

        OverlordInitialized0 = true;
    }

    public static void OverlordInitializeNFD(int nearWidth, int nearHeight, int farWidth, int farHeight, int distWidth, int distHeight)
    {
        if (OverlordInitialized1)
        {
            return;
        }

        NearShadowmaps.WidthHeight.Width = nearWidth;
        NearShadowmaps.WidthHeight.Height = nearHeight;
        NearShadowmaps.WidthHeight_Combat.Width = nearWidth;
        NearShadowmaps.WidthHeight_Combat.Height = nearHeight;
        if (NearShadowmaps.Shadowmaps != null)
        {
            for (int index = 1; index < NearShadowmaps.Shadowmaps.Length; index++)
            {
                NearShadowmaps.Shadowmaps[index].TextureFormat = (TextureFormat)0x5140;
                NearShadowmaps.Shadowmaps[index].Flags = TextureFlags.TextureDepthStencil;
            }
        }

        FarShadowmaps.WidthHeight.Width = farWidth;
        FarShadowmaps.WidthHeight.Height = farHeight;
        FarShadowmaps.WidthHeight_Combat.Width = farWidth;
        FarShadowmaps.WidthHeight_Combat.Height = farHeight;
        if (FarShadowmaps.Shadowmaps != null)
        {
            for (int index = 1; index < FarShadowmaps.Shadowmaps.Length; index++)
            {
                FarShadowmaps.Shadowmaps[index].TextureFormat = (TextureFormat)0x5140;
                FarShadowmaps.Shadowmaps[index].Flags = TextureFlags.TextureDepthStencil;
            }
        }

        DistanceShadowmaps.WidthHeight.Width = distWidth;
        DistanceShadowmaps.WidthHeight.Height = distHeight;
        DistanceShadowmaps.WidthHeight_Combat.Width = distWidth;
        DistanceShadowmaps.WidthHeight_Combat.Height = distHeight;
        if (DistanceShadowmaps.Shadowmaps != null)
        {
            for (int index = 1; index < DistanceShadowmaps.Shadowmaps.Length; index++)
            {
                DistanceShadowmaps.Shadowmaps[index].TextureFormat = (TextureFormat)0x5140;
                DistanceShadowmaps.Shadowmaps[index].Flags = TextureFlags.TextureDepthStencil;
            }
        }

        unsafe
        {
            var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
            if (rtm == null)
            {
                return;
            }

            NearShadowmaps.SetTexturePointer(&rtm->ShadowMapTextureNear);
            FarShadowmaps.SetTexturePointer(&rtm->ShadowMapTextureFar);
            DistanceShadowmaps.SetTexturePointer(&rtm->ShadowMapTextureDistance);
        }

        OverlordInitialized1 = true;
    }

    public static bool SetGlobalTextureSize(int width, int height, int widthc, int heightc, bool combat)
    {
        if (OverlordInitialized == false) { return false; }

        bool changed = false;
        if (GlobalShadowmaps.WidthHeight.Width != width
            || GlobalShadowmaps.WidthHeight.Height != height
            || GlobalShadowmaps.WidthHeight_Combat.Width != widthc
            || GlobalShadowmaps.WidthHeight_Combat.Height != heightc)
        {
            changed = true;
        }

        GlobalShadowmaps.WidthHeight.Width = width;
        GlobalShadowmaps.WidthHeight.Height = height;
        GlobalShadowmaps.WidthHeight_Combat.Width = widthc;
        GlobalShadowmaps.WidthHeight_Combat.Height = heightc;

        SetAllowCombat(combat);

        if (changed)
        {
            if (GlobalShadowmaps.Allocated)
            {
                GlobalShadowmaps.UnallocateTextures(false);
            }
            return GlobalShadowmaps.AllocateTextures();
        }

        return true;
    }

    public static bool SetNearTextureSize(int width, int height, int widthc, int heightc)
    {
        if (OverlordInitialized == false) { return false; }
        Service.Logger.Verbose("Overlord is Initialized");
        
        bool changed = false;
        if (NearShadowmaps.WidthHeight.Width != width
            || NearShadowmaps.WidthHeight.Height != height
            || NearShadowmaps.WidthHeight_Combat.Width != widthc
            || NearShadowmaps.WidthHeight_Combat.Height != heightc)
        {
            changed = true;
            Service.Logger.Verbose("Change detected");
        }

        NearShadowmaps.WidthHeight.Width = width;
        NearShadowmaps.WidthHeight.Height = height;
        NearShadowmaps.WidthHeight_Combat.Width = widthc;
        NearShadowmaps.WidthHeight_Combat.Height = heightc;

        if (changed)
        {
            if (NearShadowmaps.Allocated)
            {
                Service.Logger.Verbose("Unallocating textures...");
                NearShadowmaps.UnallocateTextures(false);
                Service.Logger.Verbose("Done");
            }
            Service.Logger.Verbose("Allocating new textures...");
            return NearShadowmaps.AllocateTextures();
        }

        return true;
    }

    public static bool SetFarTextureSize(int width, int height, int widthc, int heightc)
    {
        if (OverlordInitialized == false) { return false; }

        bool changed = false;
        if (FarShadowmaps.WidthHeight.Width != width
            || FarShadowmaps.WidthHeight.Height != height
            || FarShadowmaps.WidthHeight_Combat.Width != widthc
            || FarShadowmaps.WidthHeight_Combat.Height != heightc)
        {
            changed = true;
        }

        FarShadowmaps.WidthHeight.Width = width;
        FarShadowmaps.WidthHeight.Height = height;
        FarShadowmaps.WidthHeight_Combat.Width = widthc;
        FarShadowmaps.WidthHeight_Combat.Height = heightc;

        if (changed)
        {
            if (FarShadowmaps.Allocated)
            {
                FarShadowmaps.UnallocateTextures(false);
            }
            return FarShadowmaps.AllocateTextures();
        }

        return true;
    }

    public static bool SetDistanceTextureSize(int width, int height, int widthc, int heightc)
    {
        if (OverlordInitialized == false) { return false; }

        bool changed = false;
        if (DistanceShadowmaps.WidthHeight.Width != width
            || DistanceShadowmaps.WidthHeight.Height != height
            || DistanceShadowmaps.WidthHeight_Combat.Width != widthc
            || DistanceShadowmaps.WidthHeight_Combat.Height != heightc)
        {
            changed = true;
        }

        DistanceShadowmaps.WidthHeight.Width = width;
        DistanceShadowmaps.WidthHeight.Height = height;
        DistanceShadowmaps.WidthHeight_Combat.Width = widthc;
        DistanceShadowmaps.WidthHeight_Combat.Height = heightc;

        if (changed)
        {
            if (DistanceShadowmaps.Allocated)
            {
                DistanceShadowmaps.UnallocateTextures(false);
            }
            return DistanceShadowmaps.AllocateTextures();
        }

        return true;
    }

    public static void SetAllowCombat(bool allowCombat)
    {
        AllowCombat = allowCombat;
        GlobalShadowmaps.EnableCombat = allowCombat;
        NearShadowmaps.EnableCombat = allowCombat;
        FarShadowmaps.EnableCombat = allowCombat;
        DistanceShadowmaps.EnableCombat = allowCombat;
    }

    public static unsafe void EnableCombat()
    {
        if (OverlordInitialized == false || AllowCombat == false) { return; }

        GlobalShadowmaps.EnableCombatTextures();
        NearShadowmaps.EnableCombatTextures();
        FarShadowmaps.EnableCombatTextures();
        DistanceShadowmaps.EnableCombatTextures();
    }

    public static unsafe void DisableCombat()
    {
        if (OverlordInitialized == false) { return; }

        GlobalShadowmaps.EnableBaseTextures();
        NearShadowmaps.EnableBaseTextures();
        FarShadowmaps.EnableBaseTextures();
        DistanceShadowmaps.EnableBaseTextures();
    }

    public static void SetCombat(bool inCombat)
    {
        if (OverlordInitialized == false) { return; }

        if (inCombat != WasInCombat)
        {
            if (InitializedFrames == 0)
            {
                if (inCombat)
                {
                    EnableCombat();
                }
                else
                {
                    DisableCombat();
                }

                WasInCombat = inCombat;
            }
        }

        if (InitializedFrames > 0)
        {
            InitializedFrames--;
        }
    }

    public static void Dispose()
    {
        GlobalShadowmaps.UsingBaseTextures = true;
        GlobalShadowmaps.UnallocateTextures(true);

        NearShadowmaps.UsingBaseTextures = true;
        NearShadowmaps.UnallocateTextures(false);

        FarShadowmaps.UsingBaseTextures = true;
        FarShadowmaps.UnallocateTextures(false);

        DistanceShadowmaps.UsingBaseTextures = true;
        DistanceShadowmaps.UnallocateTextures(true);
    }
}
