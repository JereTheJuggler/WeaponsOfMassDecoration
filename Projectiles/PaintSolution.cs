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
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintSolution : PaintingProjectile{

        public PaintSolution() : base() {
            hasGraphics = false;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Spray Paint");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 6;
            projectile.height = 6;
            projectile.aiStyle = 41;
            aiType = ProjectileID.PureSpray;
            projectile.friendly = true;
            projectile.timeLeft = 50;
            projectile.alpha = 255;
            //projectile.light = .5f;
            light = .5f;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            paintConsumptionChance = .25f;
        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer) {
                Vector2 playerPos = Main.player[Main.myPlayer].position;
                Vector2 myPos = projectile.Center;
                if(Math.Sqrt(Math.Pow(myPos.X-playerPos.X,2)+Math.Pow(myPos.Y-playerPos.Y,2)) > 32){
                    Convert((int)(projectile.position.X + (int)(projectile.width / 2)) / 16, (int)(projectile.position.Y + (int)(projectile.height / 2)) / 16, 2);
                }
            }
            int dustType = DustID.Smoke;
            if(projectile.timeLeft > 100) {
                projectile.timeLeft = 100;
            }
            if(projectile.ai[0] > 7f) {
                float dustScale = 1f;
                if(projectile.ai[0] == 8f) {
                    dustScale = 0.2f;
                } else if(projectile.ai[0] == 9f) {
                    dustScale = 0.4f;
                } else if(projectile.ai[0] == 10f) {
                    dustScale = 0.6f;
                } else if(projectile.ai[0] == 11f) {
                    dustScale = 0.8f;
                }
                projectile.ai[0] += 1f;
                for(int i = 0; i < 1; i++) {
                    Dust dust = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default, 1f));
                    if(dust != null) {
                        dust.noGravity = true;
                        dust.scale *= 1.75f;
                        dust.velocity.X = dust.velocity.X * 2f;
                        dust.velocity.Y = dust.velocity.Y * 2f;
                        dust.scale *= dustScale;
                        dust.color = getColor(this);
                    }
                }
            } else {
                projectile.ai[0] += 1f;
            }
            projectile.rotation += 0.3f * (int)projectile.direction;
        }

        public void Convert(int i, int j, int size = 4) {
            for(int k = i - size; k <= i + size; k++) {
                for(int l = j - size; l <= j + size; l++) {
                    if(WorldGen.InWorld(k, l, 1) && new Vector2(k-i,l-j).Length() < new Vector2(size*2, size * 2).Length()) {
                        paint(k, l);
                    }
                }
            }
        }
    }
}
