﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
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
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.width = 10;
			projectile.height = 10;
			projectile.aiStyle = 1;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.ranged = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 600;
			projectile.alpha = 0;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;
		}

		public override bool PreAI() {
			base.PreAI();
			if(canPaint()) {
				PaintData data = getPaintData();
				if(data != null) {
					Point coords = new Point((int)Math.Floor(projectile.Center.X / 16f), (int)Math.Floor(projectile.Center.Y / 16f));
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
					Vector2 coords = projectile.position + oldVelocity * i;
					Point tile = new Point((int)Math.Floor(coords.X / 16f), (int)Math.Floor(coords.Y / 16f));
					if(tile.X > 0 && tile.Y > 0 && tile.X < Main.maxTilesX && tile.Y < Main.maxTilesY) {
						if(WorldGen.SolidOrSlopedTile(tile.X, tile.Y) || madeContact) {
							madeContact = true;
							paint(tile.X, tile.Y, data);
						}
					}
				}
			}
			Main.PlaySound(SoundID.Dig);
			projectile.Kill();
			return false;
		}
	}
}