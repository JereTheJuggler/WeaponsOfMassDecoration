using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using WeaponsOfMassDecoration.Dusts;
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
			projectile.width = 8;
			projectile.height = 8;
			projectile.aiStyle = 1;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.ranged = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 600;
			projectile.alpha = 255;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;
			aiType = ProjectileID.Bullet;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			rotation += .2f;
			return true;
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
			blowUp();
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
			blowUp();
		}

		public override void OnHitPvp(Player target, int damage, bool crit) {
			base.OnHitPvp(target, damage, crit);
			blowUp();
		}

		public void blowUp() {
			projectile.position = projectile.Center - new Vector2(32, 32);
			projectile.Size = new Vector2(64, 64);
			projectile.timeLeft = 3;
			projectile.aiStyle = 0;
			projectile.velocity = new Vector2(0, 0);

			Vector2 backwards = projectile.oldVelocity * -1;
			double splatterAngle = (Math.PI / 5) * 3;
			int spokes = 7;
			backwards = backwards.RotatedBy(splatterAngle / -2f);
			Color c = getColor(getPaintData());
			for(byte i = 0; i < spokes; i++) {
				Vector2 vel = backwards.RotatedBy((splatterAngle / (spokes - 1)) * i);
				int length = Main.rand.Next(7, 12);

				paintBetweenPoints(projectile.Center, projectile.Center + vel * length, getPaintData());
				for(int j = 0; j < length; j++) {
					Dust dust = getDust(Dust.NewDust((projectile.Center + (vel * j)) - new Vector2(5, 5), 10, 10, DustType<PaintDust>(), 0, 0, 0, c, 1f));
					if(dust != null) {
						dust.noGravity = true;
						dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
						dust.fadeIn = 3f;
						dust.scale = 1.5f;
					}
				}
			}
			for(int i = 0; i < 15; i++) {
				Dust dust = getDust(Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, DustID.Smoke, 0f, 0f, 100, c, 1f));
				if(dust != null) {
					dust.velocity *= .8f;
				}
			}
			Main.PlaySound(SoundID.Item14, projectile.Center);
		}
	}
}