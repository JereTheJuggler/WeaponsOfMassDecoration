using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;

namespace WeaponsOfMassDecoration.Projectiles {
    public class SmallPaintRocket : PaintingProjectile {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Rocket Mk I");     //The English name of the projectile
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
        }

        public override void SetDefaults() {
            projectile.width = 8;               //The width of projectile hitbox
            projectile.height = 8;              //The height of projectile hitbox
            projectile.aiStyle = 1;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.ranged = true;           //Is the projectile shoot by a ranged weapon?
            projectile.penetrate = 1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 600;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.RocketI;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                //if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY)
                    //WorldGen.paintWall(coords.X, coords.Y, (byte)color);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(color >= 0) {
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16, center.Y / 16);
                float speed = projectile.velocity.Length();
                for(int i = 0; i < speed * 8; i += 2) {
                    Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i)) / 16), (int)Math.Floor((center.Y + (oldVelocity.Y * i)) / 16));
                    if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {                       
                        WorldGen.paintTile(coords.X, coords.Y, (byte)color);
                        WorldGen.paintWall(coords.X, coords.Y, (byte)color);
                    }
                }
            }
            projectile.Kill();
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            float xOffset = (projectile.velocity.X < 0 ? 2f : -2f);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = new Vector2((texture.Width / 2f) * (float)Math.Cos(projectile.rotation + (float)Math.PI / xOffset), (texture.Width / 2f) * (float)Math.Sin(projectile.rotation + (float)Math.PI / xOffset));//new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) * (frameHeight/-4f);
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}