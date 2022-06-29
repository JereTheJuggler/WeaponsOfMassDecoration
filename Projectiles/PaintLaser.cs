using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintLaser : PaintingProjectile {
		public PaintLaser() : base() {
			xFrameCount = 1;
			yFrameCount = 1;

			hasGraphics = false;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Beam");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.timeLeft = 3;
			Projectile.alpha = 255;
			Projectile.DamageType = DamageClass.Magic;
			light = .5f;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override bool PreAI() {
			return base.PreAI();
		}

		public override void AI() {
			if(Projectile.owner == Main.myPlayer) {
				if(Projectile.timeLeft <= 2) {
					PaintData data = getPaintData();
					Vector2 playerPos = Main.player[Main.myPlayer].position;
					Vector2 myPos = Projectile.Center;
					if(Projectile.ai[1] > 0 && canPaint())
						explode(Projectile.Center, 16, data);
					Vector2 displacement = new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center;

					createLight();

					for(int p = 0; p < 12; p++) {
						Dust dust = getDust(Dust.NewDust(Projectile.TopLeft + displacement * (p / 10f) + new Vector2(-3, -3), 7, 7, DustType<Dusts.PaintDust>(), 0, 0, 200, getColor(data), 1f));
						if(dust != null) {
							dust.velocity = new Vector2(.05f, 0f).RotatedByRandom(Math.PI * 2);
							if(dust.customData != null)
								((float[])dust.customData)[1] = .02f;
							dust.fadeIn = 3f;
						}
					}
					if(canPaint())
						paint(Projectile.Center, data);
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			PaintData data = getPaintData().clone();
			data.wallsAllowed = false;
			explode(Projectile.Center, 16, data);
			return base.OnTileCollide(oldVelocity);
		}
	}
}