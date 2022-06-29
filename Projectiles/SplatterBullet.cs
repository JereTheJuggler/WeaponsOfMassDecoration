using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using WeaponsOfMassDecoration.Dusts;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	public class SplatterBullet : PaintingProjectile {

		public SplatterBullet() : base() {
			explodesOnDeath = true;
			explosionRadius = 48;

			trailLength = 5;

			yFrameCount = 1;

			manualRotation = true;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Splatter Bullet");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			AIType = ProjectileID.Bullet;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			rotation += .2f;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(CanPaint()) {
				PaintData data = GetPaintData();
				Paint(Projectile.Center, data);
				oldVelocity.Normalize();
				Vector2 center = Projectile.Center.ToWorldCoordinates(0, 0);
				center = new Vector2(center.X / 16, center.Y / 16);
				for(int i = 0; i < 64; i++) {
					Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i)) / 16), (int)Math.Floor((center.Y + (oldVelocity.Y * i)) / 16));
					if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
						Paint(coords.X, coords.Y, data);
						break;
					}
				}
			}
			BlowUp();
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
			BlowUp();
		}

		public override void OnHitPvp(Player target, int damage, bool crit) {
			base.OnHitPvp(target, damage, crit);
			BlowUp();
		}

		public void BlowUp() {
			Projectile.position = Projectile.Center - new Vector2(32, 32);
			Projectile.Size = new Vector2(64, 64);
			Projectile.timeLeft = 3;
			Projectile.aiStyle = 0;
			Projectile.velocity = new Vector2(0, 0);

			Vector2 backwards = Projectile.oldVelocity * -1;
			double splatterAngle = (Math.PI / 5) * 3;
			int spokes = 7;
			backwards = backwards.RotatedBy(splatterAngle / -2f);
			PaintData data = GetPaintData();
			bool paint = CanPaint();
			for(byte i = 0; i < spokes; i++) {
				Vector2 vel = backwards.RotatedBy((splatterAngle / (spokes - 1)) * i);
				int length = Main.rand.Next(7, 12);
				if(paint)
					PaintBetweenPoints(Projectile.Center, Projectile.Center + vel * length, data);
				for(int j = 0; j < length; j++) {
					Dust dust = GetDust(Dust.NewDust((Projectile.Center + (vel * j)) - new Vector2(5, 5), 10, 10, DustType<PaintDust>(), 0, 0, 0, data.RenderColor, 1f));
					if(dust != null) {
						dust.noGravity = true;
						dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
						dust.fadeIn = 3f;
						dust.scale = 1.5f;
					}
				}
			}
			for(int i = 0; i < 15; i++) {
				Dust dust = GetDust(Dust.NewDust(Projectile.Center - new Vector2(16, 16), 32, 32, DustID.Smoke, 0f, 0f, 100, data.RenderColor, 1f));
				if(dust != null) {
					dust.velocity *= .8f;
				}
			}
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
		}
	}
}