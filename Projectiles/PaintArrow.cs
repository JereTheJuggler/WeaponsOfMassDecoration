using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static WeaponsOfMassDecoration.PaintUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	public class PaintArrow : PaintingProjectile {

		public PaintArrow() : base() {
			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 3;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Arrow");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
		}

		public override bool PreAI() {
			base.PreAI();
			if(canPaint()) {
				PaintData data = getPaintData();
				if(data != null) {
					Point coords = new Point((int)Math.Floor(Projectile.Center.X / 16f), (int)Math.Floor(Projectile.Center.Y / 16f));
					paint(coords.X, coords.Y, data);
				}
			}
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(canPaint()) {
				PaintData data = getPaintData();
				bool madeContact = false;
				for(float i = 1f; i < 8f; i += .5f) {
					Vector2 coords = Projectile.position + oldVelocity * i;
					Point tile = new Point((int)Math.Floor(coords.X / 16f), (int)Math.Floor(coords.Y / 16f));
					if(tile.X > 0 && tile.Y > 0 && tile.X < Main.maxTilesX && tile.Y < Main.maxTilesY) {
						if(WorldGen.SolidOrSlopedTile(tile.X, tile.Y) || madeContact) {
							madeContact = true;
							paint(tile.X, tile.Y, data);
						}
					}
				}
			}
			SoundEngine.PlaySound(SoundID.Dig);
			Projectile.Kill();
			return false;
		}
	}
}