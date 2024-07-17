using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System;
using DrahsidLib;

namespace BetterShadows;

public static unsafe class CombatShadowmap
{
    public static bool CombatMode = false;
    public static int InitializedFrames = 0;
    public static Texture* ShadowMapTexture0_Combat = null;
    public static Texture* ShadowMapTexture1_Combat = null;
    public static Texture* ShadowMapTexture2_Combat = null;
    public static Texture* ShadowMapTexture3_Combat = null;
    public static Texture* ShadowMapTexture0 = null;
    public static Texture* ShadowMapTexture1 = null;
    public static Texture* ShadowMapTexture2 = null;
    public static Texture* ShadowMapTexture3 = null;
    public static SizeParam WidthHeight_Combat;
    public static SizeParam WidthHeight;

    public static void SetCombatTextureSize(ShadowmapResolution setting)
    {
        int sizeX = 512;
        int sizeY = 512 * 5;

        switch (setting)
        {
            default:
            case ShadowmapResolution.RES_NONE:
                break;
            case ShadowmapResolution.RES_64:
                sizeX = 64;
                break;
            case ShadowmapResolution.RES_128:
                sizeX = 128;
                break;
            case ShadowmapResolution.RES_256:
                sizeX = 256;
                break;
            case ShadowmapResolution.RES_512:
                sizeX = 512;
                break;
            case ShadowmapResolution.RES_1024:
                sizeX = 1024;
                break;
            case ShadowmapResolution.RES_2048:
                sizeX = 2048;
                break;
            case ShadowmapResolution.RES_4096:
                sizeX = 4096;
                break;
            case ShadowmapResolution.RES_8192:
                sizeX = 8192;
                break;
            case ShadowmapResolution.RES_16384:
                sizeX = 16384;
                break;
        }

        if (Globals.Config.MaintainGameAspect)
        {
            sizeY = Math.Min(16384, sizeX * 5);
        }

        // Fix strange behavior with strongest shadow softening by forcing the 1:5 shadowmap ratio
        var shadows = ShadowManager.Instance();
        if (shadows != null && shadows->ShadowSofteningSetting == 3)
        {
            sizeX = Math.Min(512 * 6, sizeX);
            sizeY = Math.Min(512 * 6 * 5, sizeY);
        }

        if (sizeX != WidthHeight_Combat.Width || sizeY != WidthHeight_Combat.Height)
        {
            int* width_height_combat = stackalloc int[2];
            width_height_combat[0] = sizeX;
            width_height_combat[1] = sizeY;

            var texture0 = Device.Instance()->CreateTexture2D(width_height_combat, 1, 0x5100, 0x100000, 3);
            var texture1 = Device.Instance()->CreateTexture2D(width_height_combat, 1, 0x5140, 0x200000, 3);
            var texture2 = Device.Instance()->CreateTexture2D(width_height_combat, 1, 0x5100, 0x100000, 3);
            var texture3 = Device.Instance()->CreateTexture2D(width_height_combat, 1, 0x5140, 0x200000, 3);

            if (texture0 != null && texture1 != null && texture2 != null && texture3 != null && sizeX != 0 && sizeY != 0)
            {
                if (ShadowMapTexture0_Combat != null)
                {
                    ShadowMapTexture0_Combat->DecRef();
                    ShadowMapTexture0_Combat = null;
                }

                if (ShadowMapTexture1_Combat != null)
                {
                    ShadowMapTexture1_Combat->DecRef();
                    ShadowMapTexture1_Combat = null;
                }

                if (ShadowMapTexture2_Combat != null)
                {
                    ShadowMapTexture2_Combat->DecRef();
                    ShadowMapTexture2_Combat = null;
                }

                if (ShadowMapTexture3_Combat != null)
                {
                    ShadowMapTexture3_Combat->DecRef();
                    ShadowMapTexture3_Combat = null;
                }

                ShadowMapTexture0_Combat = texture0;
                ShadowMapTexture1_Combat = texture1;
                ShadowMapTexture2_Combat = texture2;
                ShadowMapTexture3_Combat = texture3;
                WidthHeight_Combat.Width = sizeX;
                WidthHeight_Combat.Height = sizeY;
                InitializedFrames = 2;
            }
            else
            {
                // Texture allocation failed, cleanup
                Service.Logger.Error("Texture allocation failed! (Combat)");

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
            }
        }
    }

    public static void SetOriginalTextureSize(int sizeX, int sizeY) {
        WidthHeight.Width = sizeX;
        WidthHeight.Height = sizeY;
    }

    public static unsafe void SetCombat(bool toggle)
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            return;
        }

        if (toggle && InitializedFrames == 0)
        {
            if (ShadowMapTexture0_Combat != null && ShadowMapTexture1_Combat != null && ShadowMapTexture2_Combat != null && ShadowMapTexture3_Combat != null && WidthHeight_Combat.Width != 0 && WidthHeight_Combat.Height != 0)
            {
                // reset this if we reinitialized the shadowmap
                if (rtm->ShadowMapTexture0 == ShadowMapTexture0 && rtm->ShadowMapTexture1 == ShadowMapTexture1 && rtm->ShadowMapTexture2 == ShadowMapTexture2 && rtm->ShadowMapTexture3 == ShadowMapTexture3)
                {
                    rtm->ShadowMapTexture0 = ShadowMapTexture0_Combat;
                    rtm->ShadowMapTexture1 = ShadowMapTexture1_Combat;
                    rtm->ShadowMapTexture2 = ShadowMapTexture2_Combat;
                    rtm->ShadowMapTexture3 = ShadowMapTexture3_Combat;
                    rtm->ShadowMap_Width = WidthHeight_Combat.Width;
                    rtm->ShadowMap_Height = WidthHeight_Combat.Height;
                }
                else if (rtm->ShadowMapTexture0 != ShadowMapTexture0_Combat || rtm->ShadowMapTexture1 != ShadowMapTexture1_Combat || rtm->ShadowMapTexture2 != ShadowMapTexture2_Combat || rtm->ShadowMapTexture3 != ShadowMapTexture3_Combat)
                {
                    Service.Logger.Warning("Something is wrong!!! (toggle)");
                }
            }
            else
            {
                Service.Logger.Warning("Something is wrong!!! (true)");
            }
        }

        if ((CombatMode != toggle && toggle == false) || InitializedFrames > 0)
        {
            if (ShadowMapTexture0 != null && ShadowMapTexture1 != null && ShadowMapTexture2 != null && ShadowMapTexture3 != null && WidthHeight.Width != 0 && WidthHeight.Height != 0)
            {
                rtm->ShadowMapTexture0 = ShadowMapTexture0;
                rtm->ShadowMapTexture1 = ShadowMapTexture1;
                rtm->ShadowMapTexture2 = ShadowMapTexture2;
                rtm->ShadowMapTexture3 = ShadowMapTexture3;
                rtm->ShadowMap_Width = WidthHeight.Width;
                rtm->ShadowMap_Height = WidthHeight.Height;
            }
            else
            {
                Service.Logger.Warning("Something is wrong!!! (CombatMode != toggle && toggle == false)");
            }
        }

        if (InitializedFrames > 0)
        {
            InitializedFrames--;
        }

        CombatMode = toggle;
    }

    public static unsafe void Dispose()
    {
        var rtm = (RenderTargetManagerUpdated*)RenderTargetManager.Instance();
        if (rtm == null)
        {
            return;
        }

        if (CombatMode)
        {
            rtm->ShadowMapTexture0 = ShadowMapTexture0;
            rtm->ShadowMapTexture1 = ShadowMapTexture1;
            rtm->ShadowMapTexture2 = ShadowMapTexture2;
            rtm->ShadowMapTexture3 = ShadowMapTexture3;
        }

        if (ShadowMapTexture0_Combat != null)
        {
            ShadowMapTexture0_Combat->DecRef();
            ShadowMapTexture0_Combat = null;
        }

        if (ShadowMapTexture1_Combat != null)
        {
            ShadowMapTexture1_Combat->DecRef();
            ShadowMapTexture1_Combat = null;
        }

        if (ShadowMapTexture2_Combat != null)
        {
            ShadowMapTexture2_Combat->DecRef();
            ShadowMapTexture2_Combat = null;
        }

        if (ShadowMapTexture3_Combat != null)
        {
            ShadowMapTexture3_Combat->DecRef();
            ShadowMapTexture3_Combat = null;
        }

        CombatMode = false;
    }
}
