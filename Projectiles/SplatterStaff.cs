using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class SplatterStaff : PaintingProjectile {
		public Point mousePosition;
		public float distance = -1;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Bolt");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			mousePosition = Main.MouseWorld.ToTileCoordinates();
			projectile.width = 80;
			projectile.height = 80;
			projectile.aiStyle = 0;
			projectile.friendly = false;
			projectile.hostile = false;
			projectile.magic = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 3600;
			projectile.alpha = 200;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
			aiType = ProjectileID.DiamondBolt;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			double xPos = projectile.Center.X / 16;
			double yPos = projectile.Center.Y / 16;
			double displacement = Math.Sqrt(Math.Pow(xPos - mousePosition.X, 2) + Math.Pow(yPos - mousePosition.Y, 2));
			if(displacement <= 1) {
				projectile.timeLeft = 1;
			} else if(distance == -1) {
				if(startPosition.X != 0 && startPosition.Y != 0) {
					distance = (float)Math.Sqrt(Math.Pow(startPosition.X / 16 - mousePosition.X, 2) + Math.Pow(startPosition.Y / 16 - mousePosition.Y, 2));
				}
			} else {
				float projDistance = (float)Math.Sqrt(Math.Pow(xPos - startPosition.X / 16, 2) + Math.Pow(yPos - startPosition.Y / 16, 2));
				if(projDistance >= distance) {
					projectile.timeLeft = 1;
				}
			}
			if(projectile.timeLeft == 1) {
				projectile.friendly = true;
				return false;
			}
			return true;
		}

		public override void AI() {
			for(int i = 0; i < 2; i++) {
				Vector2 speed = new Vector2(2, 0).RotatedByRandom(Math.PI * 2);
				Dust dust = getDust(Dust.NewDust(projectile.Center, 0, 0, mod.DustType("PaintDust"), speed.X, speed.Y, 0, getColor(getPaintData()), 1));
				if(dust != null) {
					dust.noGravity = true;
					dust.fadeIn = 1.5f;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}

		public override void Kill(int timeLeft) {
			splatter(projectile.Center, 128, 8, getPaintData());
			createLight(projectile.Center, 1f);
			Main.PlaySound(SoundID.Item14, projectile.position);
			//smoke dust
			for(int i = 0; i < 15; i++) {
				Dust d = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, getColor(getPaintData()), 2f));
				if(d != null)
					d.velocity *= 1.4f;
			}
		}
	}
}