using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace BetterShadows;

    [StructLayout(LayoutKind.Explicit, Size = 0x4C8)]
public unsafe partial struct RenderTargetManagerUpdated {
    [FieldOffset(0x120)] public Texture* ShadowMapTexture0;
    [FieldOffset(0x128)] public Texture* ShadowMapTexture1;
    [FieldOffset(0x130)] public Texture* ShadowMapTexture2;
    [FieldOffset(0x138)] public Texture* ShadowMapTexture3;

    [FieldOffset(0x158)] public Texture* ShadowMapTexture4; // near 1
    [FieldOffset(0x160)] public Texture* ShadowMapTexture5; // near 2

    [FieldOffset(0x180)] public Texture* ShadowMapTexture6; // far 1
    [FieldOffset(0x188)] public Texture* ShadowMapTexture7; // far 2

    [FieldOffset(0x228)] public Texture* ShadowMapTexture8;
    [FieldOffset(0x230)] public Texture* ShadowMapTexture9;

    [FieldOffset(0x430)] public int Resolution_Width;
    [FieldOffset(0x434)] public int Resolution_Height;
    [FieldOffset(0x438)] public int ShadowMap_Width;
    [FieldOffset(0x43C)] public int ShadowMap_Height;
    [FieldOffset(0x440)] public int NearShadowMap_Width;
    [FieldOffset(0x444)] public int NearShadowMap_Height;
    [FieldOffset(0x448)] public int FarShadowMap_Width;
    [FieldOffset(0x44C)] public int FarShadowMap_Height;
    [FieldOffset(0x450)] public int UnkShadowMap_Width;
    [FieldOffset(0x454)] public int UnkShadowMap_Height;

    public static unsafe byte InitializeShadowmap(RenderTargetManagerUpdated* thisx, int size)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = size;
        width_height[1] = size;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture2 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture3 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null && texture2 != null && texture3 != null)
        {
            thisx->ShadowMap_Width = size;
            thisx->ShadowMap_Height = size;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture0 != null)
            {
                thisx->ShadowMapTexture0->DecRef();
                thisx->ShadowMapTexture0 = null;
            }

            if (thisx->ShadowMapTexture1 != null)
            {
                thisx->ShadowMapTexture1->DecRef();
                thisx->ShadowMapTexture1 = null;
            }

            if (thisx->ShadowMapTexture2 != null)
            {
                thisx->ShadowMapTexture2->DecRef();
                thisx->ShadowMapTexture2 = null;
            }

            if (thisx->ShadowMapTexture3 != null)
            {
                thisx->ShadowMapTexture3->DecRef();
                thisx->ShadowMapTexture3 = null;
            }

            thisx->ShadowMapTexture0 = texture0;
            thisx->ShadowMapTexture1 = texture1;
            thisx->ShadowMapTexture2 = texture2;
            thisx->ShadowMapTexture3 = texture3;

            return 1;
        }
        else
        {
            // Texture allocation failed, cleanup
            if (texture0 != null)
            {
                texture0->DecRef();
            }

            if (texture1 != null)
            {
                texture1->DecRef();
            }

            if (texture2 != null)
            {
                texture2->DecRef();
            }

            if (texture3 != null)
            {
                texture3->DecRef();
            }

            return 0;
        }
    }

    public static unsafe byte InitializeNearShadowmap(RenderTargetManagerUpdated* thisx, int size)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = size;
        width_height[1] = size;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null)
        {
            thisx->NearShadowMap_Width = size;
            thisx->NearShadowMap_Height = size;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture4 != null)
            {
                thisx->ShadowMapTexture4->DecRef();
                thisx->ShadowMapTexture4 = null;
            }

            if (thisx->ShadowMapTexture5 != null)
            {
                thisx->ShadowMapTexture5->DecRef();
                thisx->ShadowMapTexture5 = null;
            }

            thisx->ShadowMapTexture4 = texture0;
            thisx->ShadowMapTexture5 = texture1;

            return 1;
        }
        else
        {
            // Texture allocation failed, cleanup
            if (texture0 != null)
            {
                texture0->DecRef();
            }

            if (texture1 != null)
            {
                texture1->DecRef();
            }

            return 0;
        }
    }

    public static unsafe byte InitializeFarShadowmap(RenderTargetManagerUpdated* thisx, int size)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = size;
        width_height[1] = size;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null)
        {
            thisx->FarShadowMap_Width = size;
            thisx->FarShadowMap_Height = size;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture6 != null)
            {
                thisx->ShadowMapTexture6->DecRef();
                thisx->ShadowMapTexture6 = null;
            }

            if (thisx->ShadowMapTexture7 != null)
            {
                thisx->ShadowMapTexture7->DecRef();
                thisx->ShadowMapTexture7 = null;
            }

            thisx->ShadowMapTexture6 = texture0;
            thisx->ShadowMapTexture7 = texture1;

            return 1;
        }
        else
        {
            // Texture allocation failed, cleanup
            if (texture0 != null)
            {
                texture0->DecRef();
            }

            if (texture1 != null)
            {
                texture1->DecRef();
            }

            return 0;
        }
    }

    public static unsafe byte InitializeUnkShadowmap(RenderTargetManagerUpdated* thisx, int size)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = size;
        width_height[1] = size;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null)
        {
            thisx->UnkShadowMap_Width = size;
            thisx->UnkShadowMap_Height = size;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture8 != null)
            {
                thisx->ShadowMapTexture8->DecRef();
                thisx->ShadowMapTexture8 = null;
            }

            if (thisx->ShadowMapTexture9 != null)
            {
                thisx->ShadowMapTexture9->DecRef();
                thisx->ShadowMapTexture9 = null;
            }

            thisx->ShadowMapTexture8 = texture0;
            thisx->ShadowMapTexture9 = texture1;

            return 1;
        }
        else
        {
            // Texture allocation failed, cleanup
            if (texture0 != null)
            {
                texture0->DecRef();
            }

            if (texture1 != null)
            {
                texture1->DecRef();
            }

            return 0;
        }
    }
}
