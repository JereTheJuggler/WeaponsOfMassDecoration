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
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintLaser : PaintingProjectile {
        public PaintLaser() : base() {
            xFrameCount = 1;
            yFrameCount = 1;

            hasGraphics = false;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Beam");
        }
        
        public override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 6;
            projectile.height = 6;
            projectile.aiStyle = 0;
            //projectile.CloneDefaults(ProjectileID.ShadowBeamFriendly);
            //aiType = ProjectileID.ShadowBeamFriendly;
            projectile.friendly = true;
            projectile.timeLeft = 3;
            projectile.alpha = 255;
            projectile.magic = true;
            projectile.light = .5f;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            paintConsumptionChance = .25f;
        }

        public override bool PreAI() {
            return base.PreAI();
        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer) {
                if(projectile.timeLeft <= 2) {
                    Vector2 playerPos = Main.player[Main.myPlayer].position;
                    Vector2 myPos = projectile.Center;
                    paint();
                    if(projectile.ai[1] > 0)
                        explode(projectile.Center, 16, true, false);
                    Vector2 displacement = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;

					createLight();

                    for(int p = 0; p < 12; p++) {
                        //int dustId = Dust.NewDust(projectile.Center, 0, 0, mod.DustType<Dusts.LightDust>(), 0, 0, 200, getLightColor(), .75f);
                        Dust dust = getDust(Dust.NewDust(projectile.TopLeft + displacement * (p / 10f) + new Vector2(-3,-3), 7, 7, DustType<Dusts.LightDust>(), 0, 0, 200, getColor(this), 1f));
                        if(dust != null) {
                            dust.velocity = new Vector2(3, 0).RotatedByRandom(Math.PI * 2);
                            dust.fadeIn = 3f;
                        }
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            explode(projectile.Center, 16, true, false);
            return base.OnTileCollide(oldVelocity);
        }
    }
}
