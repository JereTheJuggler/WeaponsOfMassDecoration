using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintSolution : PaintingProjectile {

		public PaintSolution() : base() {
			hasGraphics = false;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spray Paint");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = 41;
			AIType = ProjectileID.PureSpray;
			Projectile.friendly = true;
			Projectile.timeLeft = 50;
			Projectile.alpha = 255;
			//projectile.light = .5f;
			light = .5f;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			if(canPaint()) {
				Vector2 playerPos = Main.player[Main.myPlayer].position;
				Vector2 myPos = Projectile.Center;
				if(Math.Sqrt(Math.Pow(myPos.X - playerPos.X, 2) + Math.Pow(myPos.Y - playerPos.Y, 2)) > 32) {
					Convert((int)(Projectile.position.X + Projectile.width / 2) / 16, (int)(Projectile.position.Y + Projectile.height / 2) / 16, 2);
				}
			}
			int dustType = DustID.Smoke;
			if(Projectile.timeLeft > 100) {
				Projectile.timeLeft = 100;
			}
			if(Projectile.ai[0] > 7f) {
				float dustScale = 1f;
				if(Projectile.ai[0] == 8f) {
					dustScale = 0.2f;
				} else if(Projectile.ai[0] == 9f) {
					dustScale = 0.4f;
				} else if(Projectile.ai[0] == 10f) {
					dustScale = 0.6f;
				} else if(Projectile.ai[0] == 11f) {
					dustScale = 0.8f;
				}
				Projectile.ai[0] += 1f;
				PaintData data = getPaintData();
				for(int i = 0; i < 1; i++) {
					Dust dust = getDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1f));
					if(dust != null) {
						dust.noGravity = true;
						dust.scale *= 1.75f;
						dust.velocity.X = dust.velocity.X * 2f;
						dust.velocity.Y = dust.velocity.Y * 2f;
						dust.scale *= dustScale;
						dust.color = getColor(data);
					}
				}
			} else {
				Projectile.ai[0] += 1f;
			}
			Projectile.rotation += 0.3f * Projectile.direction;
		}

		public void Convert(int i, int j, int size = 4) {
			PaintData data = getPaintData();
			for(int k = i - size; k <= i + size; k++) {
				for(int l = j - size; l <= j + size; l++) {
					if(WorldGen.InWorld(k, l, 1) && new Vector2(k - i, l - j).Length() < new Vector2(size * 2, size * 2).Length()) {
						paint(k, l, data);
					}
				}
			}
		}
	}
}