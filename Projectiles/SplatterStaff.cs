using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
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
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 3600;
			Projectile.alpha = 200;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			AIType = ProjectileID.DiamondBolt;
			light = .5f;
		}

		public override bool PreAI() {
			base.PreAI();
			double xPos = Projectile.Center.X / 16;
			double yPos = Projectile.Center.Y / 16;
			double displacement = Math.Sqrt(Math.Pow(xPos - mousePosition.X, 2) + Math.Pow(yPos - mousePosition.Y, 2));
			if(displacement <= 1) {
				Projectile.timeLeft = 1;
			} else if(distance == -1) {
				if(startPosition.X != 0 && startPosition.Y != 0) {
					distance = (float)Math.Sqrt(Math.Pow(startPosition.X / 16 - mousePosition.X, 2) + Math.Pow(startPosition.Y / 16 - mousePosition.Y, 2));
				}
			} else {
				float projDistance = (float)Math.Sqrt(Math.Pow(xPos - startPosition.X / 16, 2) + Math.Pow(yPos - startPosition.Y / 16, 2));
				if(projDistance >= distance) {
					Projectile.timeLeft = 1;
				}
			}
			if(Projectile.timeLeft == 1) {
				Projectile.friendly = true;
				return false;
			}
			return true;
		}

		public override void AI() {
			for(int i = 0; i < 2; i++) {
				Vector2 speed = new Vector2(2, 0).RotatedByRandom(Math.PI * 2);
				Dust dust = GetDust(Dust.NewDust(Projectile.Center, 0, 0, Mod.Find<ModDust>("PaintDust").Type, speed.X, speed.Y, 0, GetColor(GetPaintData()), 1));
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
			PaintData data = GetPaintData();
			if(CanPaint())
				Splatter(Projectile.Center, 128, 8, data);
			CreateLight(Projectile.Center, 1f);
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			//smoke dust
			for(int i = 0; i < 15; i++) {
				Dust d = GetDust(Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, data.RenderColor, 2f));
				if(d != null)
					d.velocity *= 1.4f;
			}
		}
	}
}