using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintDynamite : PaintingProjectile {
		public PaintDynamite() : base() {
			explosionRadius = 10;

			manualRotation = true;

			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Dynamite");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 420;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
			AIType = ProjectileID.Dynamite;
			Projectile.gfxOffY = 19;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			if(Projectile.velocity.Y == 0 && Math.Abs(Projectile.velocity.X) > 0)
				Projectile.velocity.X *= .98f;
			rotation += Projectile.velocity.X * .05f;
			return true;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if(Main.expertMode) {
				if(target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
					damage /= 5;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(Projectile.ai[1] != 0)
				return true;
			Projectile.soundDelay = 20;
			if(Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
				Projectile.velocity.X = oldVelocity.X * -.4f;
			if(Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
				Projectile.velocity.Y = oldVelocity.Y * -.35f;
			Projectile.velocity.X *= .9f;
			return false;
		}

		public override void AI() {
			if(Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				Projectile.tileCollide = false;
				Projectile.alpha = 255;
				Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
				Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
				Projectile.width = (int)Math.Round(32 * explosionRadius);
				Projectile.height = (int)Math.Round(32 * explosionRadius);
				Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
				Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);
				Projectile.damage = 120;
			} else {
				if(Main.rand.NextBool(2)) {
					Dust smoke = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f));
					if(smoke != null) {
						smoke.scale = .1f + Main.rand.Next(5) * .1f;
						smoke.fadeIn = 1.5f + Main.rand.Next(5) * .1f;
						smoke.noGravity = true;
						smoke.position = Projectile.Center + new Vector2(0f, (-(float)Projectile.height / 2 - 12)).RotatedBy(rotation, default) * 1.1f;
					}
					Dust fire = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f));
					if(fire != null) {
						fire.scale = 1f + Main.rand.Next(5) * .1f;
						fire.noGravity = true;
						fire.position = Projectile.Center + new Vector2(0f, (-(float)Projectile.height / 2 - 12)).RotatedBy(rotation, default) * 1.1f;
					}
				}
			}
			base.AI();
		}

		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			//smoke dust
			for(int i = 0; i < 15; i++) {
				Dust smoke = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f));
				if(smoke != null)
					smoke.velocity *= 1.4f;
			}
			//fire dust
			for(int i = 0; i < 30; i++) {
				Dust dust = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Grass, 0f, 0f, 100, default, 1f));
				if(dust != null) {
					dust.noGravity = true;
					dust.velocity *= 2f;
				}
				Dust fire = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f));
				if(fire != null)
					fire.velocity *= 2f;
			}
			//reset size
			Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
			Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
			Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);

			if(CanPaint())
				Explode(Projectile.Center, explosionRadius * 16, GetPaintData());
		}
	}
}