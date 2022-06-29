using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintBomb : PaintingProjectile {
		public const int bounceRadius = 30;

		public PaintBomb() : base() {
			//explodesOnDeath = true;
			explosionRadius = 80;

			manualRotation = true;

			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Bomb");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
			AIType = ProjectileID.BouncyBomb;
			Projectile.gfxOffY = 19;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			rotation += (Projectile.velocity.X < 0 ? -.1f : .1f);
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
			if(Projectile.soundDelay == 0) {
				SoundEngine.PlaySound(SoundID.Dig);
			}
			Projectile.soundDelay = 10;
			if(Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
				Projectile.velocity.X = oldVelocity.X * -.75f;
			if(Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
				Projectile.velocity.Y = oldVelocity.Y * -.75f;
			if(Projectile.penetrate == 1) {
				return true;
			}

			Projectile.penetrate--;

			if(Projectile.penetrate == 1) {
				Projectile.aiStyle = 0;
				Projectile.tileCollide = false;
				Projectile.timeLeft = 3;
				Projectile.penetrate = -1;
				Projectile.position = Projectile.Center - new Vector2(explosionRadius, explosionRadius);
				Projectile.Size = new Vector2(explosionRadius * 2, explosionRadius * 2);
				Projectile.velocity = new Vector2(0, 0);
				Projectile.friendly = true;
				Projectile.damage = 100;

				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
				//smoke dust
				for(int i = 0; i < 15; i++) {
					Dust smoke = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f));
					if(smoke != null)
						smoke.velocity *= 1.4f;
				}
				//fire dust
				for(int i = 0; i < 30; i++) {
					Dust fire = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GrassBlades, 0f, 0f, 100, default, 1f));
					if(fire != null) {
						fire.noGravity = true;
						fire.velocity *= 2f;
					}
					fire = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f));
					if(fire != null)
						fire.velocity *= 2f;
				}

				if(CanPaint())
					Explode(Projectile.Center, explosionRadius, GetPaintData());
			} else if(CanPaint()){
				Explode(Projectile.Center, bounceRadius, GetPaintData());
			}
			return false;
		}

		public override void AI() {
			if(Main.rand.NextBool(2)) {
				Dust d = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f));
				if(d != null) {
					d.scale = .1f + Main.rand.Next(5) * .1f;
					d.fadeIn = 1.5f + Main.rand.Next(5) * .1f;
					d.noGravity = true;
					d.position = Projectile.Center + new Vector2(0f, (-(float)Projectile.height / 2)).RotatedBy(rotation, default) * 1.1f;
				}
				d = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f));
				if(d != null) {
					d.scale = 1f + Main.rand.Next(5) * .1f;
					d.noGravity = true;
					d.position = Projectile.Center + new Vector2(0f, (-(float)Projectile.height / 2 - 6)).RotatedBy(rotation, default) * 1.1f;
				}
			}
			base.AI();
		}
	}
}