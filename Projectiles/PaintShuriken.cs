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
        public float rot = 0;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Shuriken");
            base.SetStaticDefaults();
        }

        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Shuriken);
            //projectile.width = 18;
            //projectile.height = 36;
            //projectile.aiStyle = 3;
            projectile.friendly = true;
            //projectile.melee = true;    
            //projectile.penetrate = -1;  
            //projectile.light = .5f;
            aiType = ProjectileID.Shuriken;
            projectile.thrown = true;
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0 && (projectile.timeLeft+1) % 3 == 0) {
                //projectile.frame = color;
                Vector2 projCenter = new Vector2((float)Math.Floor(projectile.Center.X / 16f)*16f, (float)Math.Floor(projectile.Center.Y / 16f) * 16f);
                for(float delta = 0; delta < Math.PI * 2; delta += (float)Math.PI / 2) {
                    float startAngle = rot + delta;
                    Vector2 center = projCenter + new Vector2(0, radius).RotatedBy(startAngle);
                    for(float delta2 = 0; delta2 < Math.PI*2/3; delta2 += (float)Math.PI / 32) {
                        paintTileAndWall(center + new Vector2(0, -1 * radius).RotatedBy(startAngle + projectile.direction * delta2));
                    }
                }
                paintedTiles = new List<Point>();
                rot += ((float)Math.PI / 6f) * projectile.direction;
            }
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            projectile.alpha = 0;
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            projectile.gfxOffY = 0;
            //float xOffset = (projectile.velocity.X < 0 ? 2f : -2f);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            //Vector2 origin = new Vector2((texture.Width / 2f) * (float)Math.Cos(projectile.rotation + (float)Math.PI / xOffset), (texture.Width / 2f) * (float)Math.Sin(projectile.rotation + (float)Math.PI / xOffset));//new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) * (frameHeight/-4f);
            Vector2 origin = sourceRectangle.Size() / 2f;//new Vector2(texture.Width / 2 - 1, projectile.gfxOffY);
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
