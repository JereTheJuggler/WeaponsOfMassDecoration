using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static WeaponsOfMassDecoration.PaintUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	class HackAndSplashBlob : PaintingProjectile {
		public HackAndSplashBlob() : base() {
			dropsOnDeath = true;
			dropCount = 5;
			dropCone = (float)Math.PI / 2f;
			fullyShaded = true;

			explodesOnDeath = true;
			explosionRadius = 64f;

			yFrameCount = 1;
			xFrameCount = 6;

			trailLength = 3;

			animationFrameDuration = 4;

			drawOriginOffset = new Vector2(0, -5);
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Splatter");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.gfxOffY = 12;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 1000;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			AIType = ProjectileID.MagicDagger;
			Projectile.damage = 20;
			Projectile.Opacity = .75f;
			light = .5f;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override bool PreAI() {
			if(CanPaint()) {
				PaintData data = GetPaintData();
				int armLength = 48;
				Vector2 arm1 = new Vector2((float)Math.Cos(Projectile.rotation + (Math.PI / 6f) + (Math.PI / 2f)), (float)Math.Sin(Projectile.rotation + (Math.PI / 6f) + (Math.PI / 2f)));
				Vector2 arm2 = new Vector2((float)Math.Cos(Projectile.rotation - (Math.PI / 6f) + (Math.PI / 2f)), (float)Math.Sin(Projectile.rotation - (Math.PI / 6f) + (Math.PI / 2f)));
				for(int offset = 8; offset <= armLength; offset += 8) {
					Point tile1 = new Point(
						(int)Math.Round((Projectile.Center.X + (arm1.X * offset)) / 16f),
						(int)Math.Round((Projectile.Center.Y + (arm1.Y * offset)) / 16f)
					);
					Point tile2 = new Point(
						(int)Math.Round((Projectile.Center.X + (arm2.X * offset)) / 16f),
						(int)Math.Round((Projectile.Center.Y + (arm2.Y * offset)) / 16f)
					);
					Paint(tile1.X, tile1.Y, data);
					Paint(tile2.X, tile2.Y, data);
				}
			}
			base.PreAI();
			return true;
		}

		public override void AI() {
			Projectile.velocity.Y += .4f;
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(Math.PI / 2f);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(CanPaint()) {
				PaintData data = GetPaintData();
				Paint(Projectile.Center, data);
				oldVelocity.Normalize();
				Explode(Projectile.Center, 32f, data);
			}
			SoundEngine.PlaySound(SoundID.Item21, Projectile.Center);
			Projectile.Kill();
			return false;
		}
	}
}