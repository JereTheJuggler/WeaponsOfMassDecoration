using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
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
			Projectile.CloneDefaults(ProjectileID.ThrowingKnife);
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.light = 0;

			AIType = ProjectileID.ThrowingKnife;
		}

		public override bool PreAI() {
			base.PreAI();
			if(CanPaint()) {
				PaintData data = GetPaintData();
				Vector2 coords = Projectile.Center + new Vector2(0, 24).RotatedBy(Projectile.rotation + Math.PI);
				Paint(coords, data);
				if(Math.Abs(Projectile.rotation - oldRotation) > Math.PI / 9) {
					Paint(Projectile.Center + new Vector2(0, 24).RotatedBy(Projectile.rotation + Math.PI + (Math.PI / 9)), data);
					Paint(Projectile.Center + new Vector2(0, 24).RotatedBy(Projectile.rotation + Math.PI - (Math.PI / 9)), data);
				}
				if(lastPaintCoord != null) {
					PaintBetweenPoints((Vector2)lastPaintCoord, coords, data);
				}
				lastPaintCoord = coords;
			}
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			return true;
		}

		public override void OnSpawn(IEntitySource source) {
			//TODO assign random paint data when thrown by the artist npc
			base.OnSpawn(source);
		}
	}
}