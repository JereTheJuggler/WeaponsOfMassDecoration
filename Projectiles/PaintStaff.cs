using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintStaff : PaintingProjectile {

		public PaintStaff() : base() {
			trailLength = 4;

			dropsOnDeath = true;
			dropCount = 5;
			dropCone = (int)(3f * Math.PI / 4f);
			dropVelocity = 6f;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Bolt");
		}

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 3600;
			Projectile.alpha = 200;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			AIType = ProjectileID.DiamondBolt;
			light = 1f;
			Projectile.alpha = 150;
		}

		public override bool PreAI() {
			base.PreAI();
			if(CanPaint()) {
				Point coords = new Point((int)Math.Floor(Projectile.Center.X / 16), (int)Math.Floor(Projectile.Center.Y / 16));
				PaintData data = GetPaintData();
				Paint(coords.X, coords.Y, data);
				for(int d = 0; d < 2; d++) {
					Dust dust = GetDust(Dust.NewDust(Projectile.position, 0, 0, DustType<Dusts.PaintDust>(), 0, 0, 200, GetColor(data), .75f));
					if(dust != null) {
						dust.noGravity = true;
						dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
						dust.fadeIn = 1.5f;
						dust.alpha = 20;
						dust.scale = 1.5f;
					}
				}
			}
			Projectile.ai[0] += .2f;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			return base.OnTileCollide(oldVelocity);
		}
	}
}