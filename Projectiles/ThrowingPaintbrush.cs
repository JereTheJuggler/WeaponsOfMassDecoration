using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static WeaponsOfMassDecoration.PaintUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	class ThrowingPaintbrush : PaintingProjectile {
		public Vector2? lastPaintCoord = null;

		public ThrowingPaintbrush() : base() {
			dropsOnDeath = true;
			dropCount = 6;
			dropVelocity = 6f;

			explodesOnDeath = true;
			explosionRadius = 45;

			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 3;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Throwing Paintbrush");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.ThrowingKnife);
			projectile.thrown = true;
			projectile.light = 0;

			aiType = ProjectileID.ThrowingKnife;
		}

		public override bool PreAI() {
			base.PreAI();
			if(canPaint()) {
				PaintData data = getPaintData();
				Vector2 coords = projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI);
				paint(coords, data);
				if(Math.Abs(projectile.rotation - oldRotation) > Math.PI / 9) {
					paint(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI + (Math.PI / 9)), data);
					paint(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI - (Math.PI / 9)), data);
				}
				if(lastPaintCoord != null) {
					paintBetweenPoints((Vector2)lastPaintCoord, coords, data);
				}
				lastPaintCoord = coords;
			}
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Main.PlaySound(SoundID.Dig, projectile.Center);
			return true;
		}
	}
}