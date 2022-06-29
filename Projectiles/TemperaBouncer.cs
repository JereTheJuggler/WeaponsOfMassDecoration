using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Dusts;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class TemperaBouncer : PaintingProjectile {
		public float lastBounceTime = 0;

		public TemperaBouncer() : base() {
			usesShader = true;

			dropsOnDeath = true;
			dropCount = 10;
			dropCone = (float)(3 * Math.PI / 4);
			dropVelocity = 6f;

			yFrameCount = 1;
			xFrameCount = 1;

			trailLength = 3;

			manualRotation = true;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Tempera Bouncer");
		}

		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 3600;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.Opacity = 1f;
			Projectile.scale = 1.3f;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			if(canPaint()) {
				PaintData data = getPaintData();
				paint(new Vector2(Projectile.Center.X + 8, Projectile.Center.Y + 8), data);
				paint(new Vector2(Projectile.Center.X + 8, Projectile.Center.Y - 8), data);
				paint(new Vector2(Projectile.Center.X - 8, Projectile.Center.Y + 8), data);
				paint(new Vector2(Projectile.Center.X - 8, Projectile.Center.Y - 8), data);
				paintedTiles = new List<Point>();
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(canPaint())
				explode(Projectile.Center, 45, getPaintData());
			if(Projectile.penetrate < 0) {
				return true;
			}
			if(Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = oldVelocity.X * -1;
			else
				Projectile.velocity.Y = oldVelocity.Y * -.9f;

			if(lastBounceTime == 0 || Main.GlobalTimeWrappedHourly - .05f >= lastBounceTime) {
				Projectile.penetrate--;
				lastBounceTime = Main.GlobalTimeWrappedHourly;
			}
			return false;
		}
		public override bool PreKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			return base.PreKill(timeLeft);
		}
		public override void Kill(int timeLeft) {
			base.Kill(timeLeft);
		}

		public override void AI() {
			Projectile.velocity.Y += .3f;
			rotation += (float)Math.PI / 10f * Projectile.direction;
			PaintData data = getPaintData();
			for(int i = 0; i < 1; i++) {
				Dust dust = getDust(Dust.NewDust(Projectile.Center - new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.width, Projectile.height, DustType<PaintDust>(), 0, 0, 0, data.RenderColor, 1));
				if(dust != null) {
					dust.noGravity = true;
					dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
					dust.fadeIn = 3f;
					dust.scale = 1.5f;
				}
			}
		}
	}
}