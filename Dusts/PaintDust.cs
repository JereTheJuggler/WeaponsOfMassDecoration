using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Dusts {
	public class PaintDust : ModDust {

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
				dust.customData = new float[] { Main.rand.NextFloat(-.2f, .2f), .03f };
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
			dust.position += dust.velocity;
			dust.scale -= ((float[])dust.customData)[1];
			if(dust.alpha >= 255 || dust.scale <= 0)
				dust.active = false;
			return false;
		}
	}
}