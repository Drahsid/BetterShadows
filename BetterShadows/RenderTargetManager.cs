using System;
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
    [FieldOffset(0x160)] public Texture* ShadowMapTexture_Near0; // These are initialized in a loop
    [FieldOffset(0x168)] public Texture* ShadowMapTexture_Near1;
    [FieldOffset(0x170)] public Texture* ShadowMapTexture_Near2;
    [FieldOffset(0x178)] public Texture* ShadowMapTexture_Near3;

    [FieldOffset(0x180)] public Texture* ShadowMapTexture5; // far 1
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

    [FieldOffset(0x228)] public Texture* ShadowMapTexture7;
    [FieldOffset(0x230)] public Texture* ShadowMapTexture8;

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

    public static unsafe byte InitializeShadowmap(RenderTargetManagerUpdated* thisx, int sizeX, int sizeY)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = sizeX;
        width_height[1] = sizeY;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture2 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture3 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null && texture2 != null && texture3 != null)
        {
            thisx->ShadowMap_Width = sizeX;
            thisx->ShadowMap_Height = sizeY;

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

    public static unsafe byte InitializeNearShadowmap(RenderTargetManagerUpdated* thisx, int sizeX, int sizeY)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = sizeX;
        width_height[1] = sizeY;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture2 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture3 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture4 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null && texture2 != null && texture3 != null && texture4 != null)
        {
            thisx->NearShadowMap_Width = sizeX;
            thisx->NearShadowMap_Height = sizeY;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture4 != null)
            {
                thisx->ShadowMapTexture4->DecRef();
                thisx->ShadowMapTexture4 = null;
            }

            if (thisx->ShadowMapTexture_Near0 != null)
            {
                thisx->ShadowMapTexture_Near0->DecRef();
                thisx->ShadowMapTexture_Near0 = null;
            }

            if (thisx->ShadowMapTexture_Near1 != null)
            {
                thisx->ShadowMapTexture_Near1->DecRef();
                thisx->ShadowMapTexture_Near1 = null;
            }

            if (thisx->ShadowMapTexture_Near2 != null)
            {
                thisx->ShadowMapTexture_Near2->DecRef();
                thisx->ShadowMapTexture_Near2 = null;
            }

            if (thisx->ShadowMapTexture_Near3 != null)
            {
                thisx->ShadowMapTexture_Near3->DecRef();
                thisx->ShadowMapTexture_Near3 = null;
            }

            thisx->ShadowMapTexture4 = texture0;
            thisx->ShadowMapTexture_Near0 = texture1;
            thisx->ShadowMapTexture_Near1 = texture2;
            thisx->ShadowMapTexture_Near2 = texture3;
            thisx->ShadowMapTexture_Near3 = texture4;

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

            if (texture4 != null)
            {
                texture4->DecRef();
            }

            return 0;
        }
    }

    public static unsafe byte InitializeFarShadowmap(RenderTargetManagerUpdated* thisx, int sizeX, int sizeY)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = sizeX;
        width_height[1] = sizeY;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture2 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture3 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture4 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture5 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture6 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture7 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture8 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture9 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureA = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureB = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureC = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureD = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureE = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* textureF = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture10 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture11 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture12 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture13 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);
        Texture* texture14 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null)
        {
            thisx->FarShadowMap_Width = sizeX;
            thisx->FarShadowMap_Height = sizeY;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture5 != null)
            {
                thisx->ShadowMapTexture5->DecRef();
                thisx->ShadowMapTexture5 = null;
            }

            if (thisx->ShadowMapTexture_Far00 != null)
            {
                thisx->ShadowMapTexture_Far00->DecRef();
                thisx->ShadowMapTexture_Far00 = null;
            }

            if (thisx->ShadowMapTexture_Far01 != null)
            {
                thisx->ShadowMapTexture_Far01->DecRef();
                thisx->ShadowMapTexture_Far01 = null;
            }

            if (thisx->ShadowMapTexture_Far02 != null)
            {
                thisx->ShadowMapTexture_Far02->DecRef();
                thisx->ShadowMapTexture_Far02 = null;
            }

            if (thisx->ShadowMapTexture_Far03 != null)
            {
                thisx->ShadowMapTexture_Far03->DecRef();
                thisx->ShadowMapTexture_Far03 = null;
            }

            if (thisx->ShadowMapTexture_Far04 != null)
            {
                thisx->ShadowMapTexture_Far04->DecRef();
                thisx->ShadowMapTexture_Far04 = null;
            }

            if (thisx->ShadowMapTexture_Far05 != null)
            {
                thisx->ShadowMapTexture_Far05->DecRef();
                thisx->ShadowMapTexture_Far05 = null;
            }

            if (thisx->ShadowMapTexture_Far06 != null)
            {
                thisx->ShadowMapTexture_Far06->DecRef();
                thisx->ShadowMapTexture_Far06 = null;
            }

            if (thisx->ShadowMapTexture_Far07 != null)
            {
                thisx->ShadowMapTexture_Far07->DecRef();
                thisx->ShadowMapTexture_Far07 = null;
            }

            if (thisx->ShadowMapTexture_Far08 != null)
            {
                thisx->ShadowMapTexture_Far08->DecRef();
                thisx->ShadowMapTexture_Far08 = null;
            }

            if (thisx->ShadowMapTexture_Far09 != null)
            {
                thisx->ShadowMapTexture_Far09->DecRef();
                thisx->ShadowMapTexture_Far09 = null;
            }

            if (thisx->ShadowMapTexture_Far0A != null)
            {
                thisx->ShadowMapTexture_Far0A->DecRef();
                thisx->ShadowMapTexture_Far0A = null;
            }

            if (thisx->ShadowMapTexture_Far0B != null)
            {
                thisx->ShadowMapTexture_Far0B->DecRef();
                thisx->ShadowMapTexture_Far0B = null;
            }

            if (thisx->ShadowMapTexture_Far0C != null)
            {
                thisx->ShadowMapTexture_Far0C->DecRef();
                thisx->ShadowMapTexture_Far0C = null;
            }

            if (thisx->ShadowMapTexture_Far0D != null)
            {
                thisx->ShadowMapTexture_Far0D->DecRef();
                thisx->ShadowMapTexture_Far0D = null;
            }

            if (thisx->ShadowMapTexture_Far0E != null)
            {
                thisx->ShadowMapTexture_Far0E->DecRef();
                thisx->ShadowMapTexture_Far0E = null;
            }

            if (thisx->ShadowMapTexture_Far0F != null)
            {
                thisx->ShadowMapTexture_Far0F->DecRef();
                thisx->ShadowMapTexture_Far0F = null;
            }

            if (thisx->ShadowMapTexture_Far10 != null)
            {
                thisx->ShadowMapTexture_Far10->DecRef();
                thisx->ShadowMapTexture_Far10 = null;
            }

            if (thisx->ShadowMapTexture_Far11 != null)
            {
                thisx->ShadowMapTexture_Far11->DecRef();
                thisx->ShadowMapTexture_Far11 = null;
            }

            if (thisx->ShadowMapTexture_Far12 != null)
            {
                thisx->ShadowMapTexture_Far12->DecRef();
                thisx->ShadowMapTexture_Far12 = null;
            }

            if (thisx->ShadowMapTexture_Far13 != null)
            {
                thisx->ShadowMapTexture_Far13->DecRef();
                thisx->ShadowMapTexture_Far13 = null;
            }

            thisx->ShadowMapTexture5 = texture0;
            thisx->ShadowMapTexture_Far00 = texture1;
            thisx->ShadowMapTexture_Far01 = texture2;
            thisx->ShadowMapTexture_Far02 = texture3;
            thisx->ShadowMapTexture_Far03 = texture4;
            thisx->ShadowMapTexture_Far04 = texture5;
            thisx->ShadowMapTexture_Far05 = texture6;
            thisx->ShadowMapTexture_Far06 = texture7;
            thisx->ShadowMapTexture_Far07 = texture8;
            thisx->ShadowMapTexture_Far08 = texture9;
            thisx->ShadowMapTexture_Far09 = textureA;
            thisx->ShadowMapTexture_Far0A = textureB;
            thisx->ShadowMapTexture_Far0B = textureC;
            thisx->ShadowMapTexture_Far0C = textureD;
            thisx->ShadowMapTexture_Far0D = textureE;
            thisx->ShadowMapTexture_Far0E = textureF;
            thisx->ShadowMapTexture_Far0F = texture10;
            thisx->ShadowMapTexture_Far10 = texture11;
            thisx->ShadowMapTexture_Far11 = texture12;
            thisx->ShadowMapTexture_Far12 = texture13;
            thisx->ShadowMapTexture_Far13 = texture14;

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

            if (texture4 != null)
            {
                texture4->DecRef();
            }

            if (texture5 != null)
            {
                texture5->DecRef();
            }

            if (texture6 != null)
            {
                texture6->DecRef();
            }

            if (texture7 != null)
            {
                texture7->DecRef();
            }

            if (texture8 != null)
            {
                texture8->DecRef();
            }

            if (texture9 != null)
            {
                texture9->DecRef();
            }

            if (textureA != null)
            {
                textureA->DecRef();
            }

            if (textureB != null)
            {
                textureB->DecRef();
            }

            if (textureC != null)
            {
                textureC->DecRef();
            }

            if (textureD != null)
            {
                textureD->DecRef();
            }

            if (textureE != null)
            {
                textureE->DecRef();
            }

            if (textureF != null)
            {
                textureF->DecRef();
            }

            if (texture10 != null)
            {
                texture10->DecRef();
            }

            if (texture11 != null)
            {
                texture11->DecRef();
            }

            if (texture12 != null)
            {
                texture12->DecRef();
            }

            if (texture13 != null)
            {
                texture13->DecRef();
            }

            if (texture14 != null)
            {
                texture14->DecRef();
            }

            return 0;
        }
    }

    public static unsafe byte InitializeUnkShadowmap(RenderTargetManagerUpdated* thisx, int sizeX, int sizeY)
    {
        int* width_height = stackalloc int[2];

        width_height[0] = sizeX;
        width_height[1] = sizeY;

        Texture* texture0 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5100, 0x100000, 3);
        Texture* texture1 = Device.Instance()->CreateTexture2D(&width_height[0], 1, 0x5140, 0x200000, 3);

        if (texture0 != null && texture1 != null)
        {
            thisx->UnkShadowMap_Width = sizeX;
            thisx->UnkShadowMap_Height = sizeY;

            // decref existing textures before we assign the new ones
            if (thisx->ShadowMapTexture7 != null)
            {
                thisx->ShadowMapTexture7->DecRef();
                thisx->ShadowMapTexture7 = null;
            }

            if (thisx->ShadowMapTexture8 != null)
            {
                thisx->ShadowMapTexture8->DecRef();
                thisx->ShadowMapTexture8 = null;
            }

            thisx->ShadowMapTexture7 = texture0;
            thisx->ShadowMapTexture8 = texture1;

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
