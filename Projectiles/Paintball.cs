using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;

namespace WeaponsOfMassDecoration.Projectiles {
    public class Paintball : PaintingProjectile {
        
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paintball");     //The English name of the projectile
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
            projectile.alpha = 255;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.Bullet;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
            light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                //Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paintAlongOldVelocity(projectile.oldVelocity);
                //paintTileAndWall(coords.X, coords.Y);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(color >= 0) {
                paintTileAndWall();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16, center.Y / 16);
                for(int i = 0; i < 64; i++) {
                    Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i))/16), (int)Math.Floor((center.Y + (oldVelocity.Y * i))/16));
                    if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
                        paintTileAndWall(coords.X, coords.Y);
                        break;
                    }
                }
            }
            Main.PlaySound(SoundID.Dig,projectile.Center);
            projectile.Kill();
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (texture.Height - (2 * (Main.projFrames[projectile.type]-1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight+2) * projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width/2, texture.Width / 2);
            for(int k = 0; k < projectile.oldPos.Length; k++) {
                //Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + origin + new Vector2(0f, projectile.gfxOffY);
                Vector2 drawPos = projectile.oldPos[k]-Main.screenPosition+(origin/2);
                Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}