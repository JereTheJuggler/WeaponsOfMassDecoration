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
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Boomerang");
            base.SetStaticDefaults();
        }

        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Flamarang);
            projectile.width = 20;
            projectile.height = 20;
            projectile.aiStyle = 3;
            projectile.friendly = true; // 8 for paint flower
            projectile.melee = true;    // 13 for harPAINT
            projectile.penetrate = -1;  // 21 for water bolt
            light = .5f;

            aiType = ProjectileID.WoodenBoomerang;
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                paintTileAndWall(new Vector2(projectile.Center.X + 8,projectile.Center.Y + 8));
                paintTileAndWall(new Vector2(projectile.Center.X + 8, projectile.Center.Y - 8));
                paintTileAndWall(new Vector2(projectile.Center.X - 8, projectile.Center.Y + 8));
                paintTileAndWall(new Vector2(projectile.Center.X - 8, projectile.Center.Y - 8));
                paintedTiles = new List<Point>();
            }
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(color >= 0) {
                explode(projectile.Center, 45, true, true);
            }
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            projectile.gfxOffY = 19;
            //float xOffset = (projectile.velocity.X < 0 ? 2f : -2f);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            //Vector2 origin = new Vector2((texture.Width / 2f) * (float)Math.Cos(projectile.rotation + (float)Math.PI / xOffset), (texture.Width / 2f) * (float)Math.Sin(projectile.rotation + (float)Math.PI / xOffset));//new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) * (frameHeight/-4f);
            Vector2 origin = new Vector2(texture.Width / 2 - 1, projectile.gfxOffY);
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
