using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WeaponsOfMassDecoration.NPCs {
    class WoMDPlayer : ModPlayer{
        public bool painted = false;
        public int paintColor = -1;
        public Items.CustomPaint customPaint = null;

        public override void ResetEffects() {
            painted = false;
        }

        public override void UpdateDead() {
            painted = false;
        }

        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
            /*if(painted && (paintColor > 0 || customPaint != null)) {
                byte trueColor = 0;
                if(customPaint != null)
                    trueColor = customPaint.getTrueColor();
                else
                    trueColor = (byte)paintColor;
                Color drawColor = PaintColors.colors[trueColor];
                r = drawColor.R;
                g = drawColor.G;
                b = drawColor.B;
            }*/
        }
    }
}
