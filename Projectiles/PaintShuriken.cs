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
    class PaintShuriken : PaintingProjectile{

        public float radius = 40f;

        public PaintShuriken() : base() {
            trailLength = 3;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Shuriken");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.CloneDefaults(ProjectileID.Shuriken);
            projectile.friendly = true;
            aiType = ProjectileID.Shuriken;
            projectile.thrown = true;
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint() && (projectile.timeLeft+1) % 3 == 0) {
                Vector2 projCenter = new Vector2((float)Math.Floor(projectile.Center.X / 16f)*16f, (float)Math.Floor(projectile.Center.Y / 16f) * 16f);
                for(float delta = 0; delta < Math.PI * 2; delta += (float)Math.PI / 2) {
                    float startAngle = rotation + delta;
                    Vector2 center = projCenter + new Vector2(0, radius).RotatedBy(startAngle);
                    for(float delta2 = 0; delta2 < Math.PI*2/3; delta2 += (float)Math.PI / 32) {
                        paint(center + new Vector2(0, -1 * radius).RotatedBy(startAngle + projectile.direction * delta2));
                    }
                }
                paintedTiles = new List<Point>();
                rotation += ((float)Math.PI / 6f) * projectile.direction;
            }
            return true;
        }

		public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Dig, projectile.Center);
            for(int i = 0; i < 4; i++) {
                Vector2 vel = oldVelocity * -1;
                vel.Normalize();
                vel = vel.RotatedBy(Main.rand.NextFloat(-.2f, .2f));
                vel *= .5f;
                Dust.NewDust(projectile.Center, 0, 0, DustID.Stone, vel.X, vel.Y,0,WeaponsOfMassDecoration.getColor(this));
			}
			return base.OnTileCollide(oldVelocity);
		}
    }
}
