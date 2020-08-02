using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net.DateFormatter;
using WeaponsOfMassDecoration.Dusts;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintBoomerang : PaintingProjectile{

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
            projectile.CloneDefaults(ProjectileID.Flamarang);
            projectile.width = 20;
            projectile.height = 20;
            projectile.aiStyle = 3;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.gfxOffY = 19;
            light = .5f;
            projectile.light = 0;
            aiType = ProjectileID.WoodenBoomerang;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                paint(new Vector2(projectile.Center.X + 8,projectile.Center.Y + 8));
                paint(new Vector2(projectile.Center.X + 8, projectile.Center.Y - 8));
                paint(new Vector2(projectile.Center.X - 8, projectile.Center.Y + 8));
                paint(new Vector2(projectile.Center.X - 8, projectile.Center.Y - 8));
                paintedTiles = new List<Point>();
            }
            return true;
        }

		public override void AI() {
            //projectile.VanillaAI();
			base.AI();
            if(Main.rand.NextFloat() <= .5f) {
                Dust dust = Dust.NewDustPerfect(projectile.Center, DustType<PaintDust>(), new Vector2(2, 0).RotatedByRandom(Math.PI * 2), 0, getColor(this), 1);
                if(dust != null) {
                    dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                    ((float[])dust.customData)[1] = .015f;
                    ((float[])dust.customData)[0] = 0f;
                }
            }
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
            if(canPaint()) {
                explode(projectile.Center, 45, true, true);
            }
            return true;
        }
    }
}
