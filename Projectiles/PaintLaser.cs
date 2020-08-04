using Microsoft.Xna.Framework;
using System;
using Terraria;
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
			projectile.width = 6;
			projectile.height = 6;
			projectile.aiStyle = 0;
			projectile.friendly = true;
			projectile.timeLeft = 3;
			projectile.alpha = 255;
			projectile.magic = true;
			light = .5f;
			projectile.penetrate = -1;
			projectile.extraUpdates = 2;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override bool PreAI() {
			return base.PreAI();
		}

		public override void AI() {
			if(projectile.owner == Main.myPlayer) {
				if(projectile.timeLeft <= 2) {
					PaintData data = getPaintData();
					Vector2 playerPos = Main.player[Main.myPlayer].position;
					Vector2 myPos = projectile.Center;
					if(projectile.ai[1] > 0 && canPaint())
						explode(projectile.Center, 16, data);
					Vector2 displacement = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;

					createLight();

					for(int p = 0; p < 12; p++) {
						Dust dust = getDust(Dust.NewDust(projectile.TopLeft + displacement * (p / 10f) + new Vector2(-3, -3), 7, 7, DustType<Dusts.PaintDust>(), 0, 0, 200, getColor(data), 1f));
						if(dust != null) {
							dust.velocity = new Vector2(.05f, 0f).RotatedByRandom(Math.PI * 2);
							if(dust.customData != null)
								((float[])dust.customData)[1] = .02f;
							dust.fadeIn = 3f;
						}
					}
					if(canPaint())
						paint(projectile.Center, data);
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			PaintData data = getPaintData().clone();
			data.wallsAllowed = false;
			explode(projectile.Center, 16, data);
			return base.OnTileCollide(oldVelocity);
		}
	}
}