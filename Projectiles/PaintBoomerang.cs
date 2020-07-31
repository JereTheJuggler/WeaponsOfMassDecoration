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
            projectile.light = .5f;
            projectile.gfxOffY = 19;

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
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(canPaint()) {
                explode(projectile.Center, 45, true, true);
            }
            return true;
        }

        protected override int convertColorFrame() {
            if(colorFrame == 0)
                return 0;
            if(colorFrame == PaintID.Negative)
                return 2;
            return 1;
        }
    }
}
