using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static WeaponsOfMassDecoration.PaintUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintSplatter : PaintingProjectile {
		public int startingTimeLeft = 25;

		public PaintSplatter() : base() {
			usesShader = true;

			xFrameCount = 1;
			yFrameCount = 1;

			manualRotation = true;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Splatter");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 10;
			projectile.height = 10;
			projectile.gfxOffY = 5;
			projectile.aiStyle = 25;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.penetrate = 1;
			projectile.timeLeft = startingTimeLeft;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;
			aiType = ProjectileID.MagicDagger;
			projectile.damage = 1;
		}

		public override bool PreAI() {
			base.PreAI();
			if(canPaint()) {
				PaintData data = getPaintData();
				Point coords = projectile.Center.ToTileCoordinates();
				paint(coords.X, coords.Y, data);
			}
			return true;
		}

		public override void PostAI() {
			rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
			base.PostAI();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(canPaint()) {
				PaintData data = getPaintData();
				paint(projectile.Center, data);
				oldVelocity.Normalize();
				Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
				center = new Vector2(center.X / 16, center.Y / 16);
				for(int i = 0; i < 64; i++) {
					Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i)) / 16), (int)Math.Floor((center.Y + (oldVelocity.Y * i)) / 16));
					if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
						paint(coords.X, coords.Y, data);
						break;
					}
				}
			}
			projectile.Kill();
			return false;
		}
	}
}