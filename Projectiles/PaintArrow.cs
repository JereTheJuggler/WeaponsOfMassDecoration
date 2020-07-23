using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;

namespace WeaponsOfMassDecoration.Projectiles {
    public class PaintArrow : PaintingProjectile {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Arrow");     //The English name of the projectile
            //ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
        }

        public override void SetDefaults() {
            projectile.width = 10;               //The width of projectile hitbox
            projectile.height = 10;              //The height of projectile hitbox
            projectile.aiStyle = 1;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.ranged = true;           //Is the projectile shoot by a ranged weapon?
            projectile.penetrate = 1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 600;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.light = 0f;            //How much light emit around the projectile
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.WoodenArrowFriendly;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paintTileAndWall(coords.X, coords.Y);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(color >= 0) {
                bool madeContact = false;
                for(float i = 1f; i < 8f; i += .5f) {
                    Vector2 coords = projectile.position + oldVelocity * i;
                    Point tile = new Point((int)Math.Floor(coords.X / 16), (int)Math.Floor(coords.Y / 16));
                    if(tile.X > 0 && tile.Y > 0 && tile.X < Main.maxTilesX && tile.Y < Main.maxTilesY) {
                        if(WorldGen.SolidOrSlopedTile(tile.X, tile.Y) || madeContact) {
                            madeContact = true;
                            paintTileAndWall(tile.X, tile.Y);
                        }
                    }
                }
            }
            Main.PlaySound(SoundID.Dig);
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
            //Vector2 origin = new Vector2((texture.Width/2f) * (float)Math.Cos(projectile.rotation+(float)Math.PI/xOffset), (texture.Width/2f) * (float)Math.Sin(projectile.rotation + (float)Math.PI / xOffset));//new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) * (frameHeight/-4f);
            //origin += (texture.Width/2)*projectile.velocity.SafeNormalize(new Vector2(0, 1));
            Vector2 origin = new Vector2(texture.Width / 2, texture.Width/2);
            //Vector2 origin = new Vector2(0, 0);
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}