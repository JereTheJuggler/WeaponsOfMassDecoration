using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using WeaponsOfMassDecoration.Dusts;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintBoomerang : PaintingProjectile {

		public PaintBoomerang() : base() {
			usesGSShader = true;

			xFrameCount = 1;
			yFrameCount = 3;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Boomerang");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.Flamarang);
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = 3;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.gfxOffY = 19;
			light = .5f;
			Projectile.light = 0;
			AIType = ProjectileID.WoodenBoomerang;
		}

		public override bool PreAI() {
			base.PreAI();
			if(CanPaint()) {
				PaintData data = GetPaintData();
				Paint(new Vector2(Projectile.Center.X + 8, Projectile.Center.Y + 8), data);
				Paint(new Vector2(Projectile.Center.X + 8, Projectile.Center.Y - 8), data);
				Paint(new Vector2(Projectile.Center.X - 8, Projectile.Center.Y + 8), data);
				Paint(new Vector2(Projectile.Center.X - 8, Projectile.Center.Y - 8), data);
				paintedTiles = new List<Point>();
			}
			return true;
		}

		public override void AI() {
			base.AI();
			if(Main.rand.NextFloat() <= .5f) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustType<PaintDust>(), new Vector2(2, 0).RotatedByRandom(Math.PI * 2), 0, GetColor(GetPaintData()), 1);
				if(dust != null) {
					dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
					((float[])dust.customData)[1] = .015f;
					((float[])dust.customData)[0] = 0f;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(CanPaint())
				Explode(Projectile.Center, 45, GetPaintData());
			return true;
		}
	}
}