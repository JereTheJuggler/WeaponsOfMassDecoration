using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static WeaponsOfMassDecoration.PaintUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	public class Paintball : PaintingProjectile {

		public Paintball() {
			trailLength = 5;

			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paintball");
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
			Projectile.extraUpdates = 1;
			AIType = ProjectileID.Bullet;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			if(CanPaint()) {
				PaintAlongOldVelocity(Projectile.oldVelocity, GetPaintData());
			}
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(CanPaint()) {
				PaintData data = GetPaintData();
				Paint(Projectile.Center, data);
				oldVelocity.Normalize();
				Vector2 center = Projectile.Center.ToWorldCoordinates(0, 0);
				center = new Vector2(center.X / 16f, center.Y / 16f);
				for(int i = 0; i < 64; i++) {
					Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i)) / 16f), (int)Math.Floor((center.Y + (oldVelocity.Y * i)) / 16f));
					if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
						Paint(coords.X, coords.Y, data);
						break;
					}
				}
			}
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			Projectile.Kill();
			return false;
		}
	}
}