using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintStaff : PaintingProjectile{

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
            projectile.width = 10;
            projectile.height = 10;
            projectile.aiStyle = 0;
			projectile.friendly = true;
            projectile.hostile = false;
            projectile.magic = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 3600;
            projectile.alpha = 200;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            aiType = ProjectileID.DiamondBolt;
            light = 1f;
            projectile.alpha = 150;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paint(coords.X, coords.Y);
                for(int d = 0; d < 2; d++) {
                    Dust dust = getDust(Dust.NewDust(projectile.position, 0, 0, DustType<Dusts.PaintDust>(), 0, 0, 200, getColor(this), .75f));
                    if(dust != null) {
                        dust.noGravity = true;
                        dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                        dust.fadeIn = 1.5f;
                        dust.alpha = 20;
                        dust.scale = 1.5f;
                    }
				}
            }
			projectile.ai[0] += .2f;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Item10, projectile.Center);
            return base.OnTileCollide(oldVelocity);
        }
    }
}
