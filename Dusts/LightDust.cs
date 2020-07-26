using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using WeaponsOfMassDecoration.Buffs;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Dusts {
    public class LightDust : ModDust{

        public Color trueColor;

        public override void SetDefaults() {
            //base.SetDefaults();
        }
        public override void OnSpawn(Dust dust) {
			if(Main.netMode != NetmodeID.Server) {
				SetDefaults();
				dust.alpha = 0;
				dust.noGravity = true;
                trueColor = dust.color;
                dust.noLight = true;
				//dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.SilverDye).UseColor(Color.White).UseSaturation(1f);
				//dust.scale = 1f;
				//dust.position += new Vector2(Main.rand.Nextint(-4f, 4f), Main.rand.Nextint(-4f, 4f));
				dust.customData = new float[] { Main.rand.NextFloat(-.2f, .2f) };
			}
        }
        public override bool MidUpdate(Dust dust) {
            dust.rotation += ((float[])dust.customData)[0];
            return true;
        }

		public override Color? GetAlpha(Dust dust, Color lightColor) {
            dust.color = trueColor.MultiplyRGB(lightColor);
            return Color.White;
		}

		public override bool Update(Dust dust) {
            float rotSpeed = ((float[])dust.customData)[0];
            dust.rotation += rotSpeed;
            dust.alpha += 1;
            dust.scale -= .03f;
            if(dust.alpha >= 255 || dust.scale <= 0)
                dust.active = false;
            return false;
        }
    }
}
