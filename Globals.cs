using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterShadows
{
    internal class Globals
    {
        public static Configuration Config;
        public static DtrDisplay DtrDisplay;
        public static bool ReapplyPreset = false;

        public static void ToggleHacks() {
            if (Globals.Config.Enabled && Globals.Config.EnabledOverall) {
                CodeManager.DoEnableHacks();
            }
            else {
                CodeManager.DoDisableHacks();
            }
        }

        public static void ToggleShadowmap() {
            if (Globals.Config.HigherResShadowmap && Globals.Config.EnabledOverall) {
                CodeManager.DoEnableShadowmap();
            }
            else {
                CodeManager.DoDisableShadowmap();
            }
        }
    }
}
