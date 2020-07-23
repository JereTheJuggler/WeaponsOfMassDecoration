﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintDynamite : PaintingProjectile {
        public float rot = 0f;

		public PaintDynamite() : base() {
			explosionRadius = 10;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Dynamite");     //The English name of the projectile
            //ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
        }

        public override void SetDefaults() {
            projectile.width = 8;               //The width of projectile hitbox
            projectile.height = 8;              //The height of projectile hitbox
            projectile.aiStyle = 1;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.penetrate = -1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 420;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.Dynamite;
            paintConsumptionChance = .5f;      
            Main.projFrames[projectile.type] = 31;
            rot = 0f;
            light = .5f;
            //drawOffsetX = 9;
            //drawOriginOffsetY = 9;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                //Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                //if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY)
                //    WorldGen.paintWall(coords.X, coords.Y, (byte)color);
            }
            if(projectile.velocity.Y == 0 && Math.Abs(projectile.velocity.X) > 0)
                projectile.velocity.X *= .98f;
            rot += projectile.velocity.X * .05f;
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Main.expertMode) {
                if(target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                    damage /= 5;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.ai[1] != 0)
                return true;
            //if(projectile.soundDelay == 0) {
            //    Main.PlaySound(SoundID.Dig);
            //}
            projectile.soundDelay = 20;
            if(projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
                projectile.velocity.X = oldVelocity.X * -.4f;
            if(projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
                projectile.velocity.Y = oldVelocity.Y * -.35f;
            projectile.velocity.X *= .9f;
            return false;
        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer && projectile.timeLeft <= 3) {
                projectile.tileCollide = false;
                projectile.alpha = 255;
                projectile.position.X = projectile.position.X + (projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (projectile.height / 2);
                projectile.width = (int)Math.Round(32 * explosionRadius);
				projectile.height = (int)Math.Round(32 * explosionRadius);
				projectile.position.X = projectile.position.X - (projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (projectile.height / 2);
                projectile.damage = 120;
            } else {
                if(Main.rand.Next(2) == 0) {
                    int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1f);
                    Main.dust[dustIndex].scale = .1f + Main.rand.Next(5) * .1f;
                    Main.dust[dustIndex].fadeIn = 1.5f + Main.rand.Next(5) * .1f;
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2 - 12)).RotatedBy(rot, default(Vector2)) * 1.1f;
                    dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1f);
                    Main.dust[dustIndex].scale = 1f + Main.rand.Next(5) * .1f;
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2 - 12)).RotatedBy(rot, default(Vector2)) * 1.1f;
                }
            }
            base.AI();
        }
        //    projectile.ai[0] += 1f;
        //    if(projectile.ai[0] > 5f) {
        //        projectile.ai[0] = 10f;
        //        if(projectile.velocity.Y == 0f && projectile.velocity.X != 0f) {
        //            projectile.velocity.X = projectile.velocity.X * .97f;
        //            if((double)projectile.velocity.X > -.01 && (double)projectile.velocity.X < .01) {
        //                projectile.velocity.X = 0f;
        //                projectile.netUpdate = true;
        //            }
        //        }
        //        projectile.velocity.Y = projectile.velocity.Y + .2f;
        //    }
        //    projectile.rotation += projectile.velocity.X * .1f;
        //}

        public override void Kill(int timeLeft) {
            if(projectile.ai[1] == 0) {
                //for(int i = 0; i < 5; i++) {
                //    Vector2 vel = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-10, -8));
                //    Projectile.NewProjectile(projectile.Center, vel, projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0, 1);
                //}
            }
            Main.PlaySound(SoundID.Item14, projectile.position);
            //smoke dust
            for(int i = 0; i < 15; i++) {
                int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            //fire dust
            for(int i = 0; i < 30; i++) {
                int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 3, 0f, 0f, 100, default(Color), 1f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 2f;
                dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity *= 2f;
            }
            //large smoke gore spawn
            //for(int g = 0; g < 2; g++) {
            //    for(int i = 0; i < 4; i++) {
            //        int goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f),
            //                                     default(Vector2), Main.rand.Next(61, 64), 1f);
            //        Main.gore[goreIndex].scale = 1.5f;
            //        Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
            //        Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
            //    }
            //}
            //reset size
            projectile.position.X = projectile.position.X + (projectile.width / 2);
            projectile.position.Y = projectile.position.Y + (projectile.height / 2);
            projectile.width = 18;
            projectile.height = 18;
            projectile.position.X = projectile.position.X - (projectile.width / 2);
            projectile.position.Y = projectile.position.Y - (projectile.height / 2);

            if(color >= 0) {
                explode(projectile.Center, explosionRadius*16, true, true);
            }
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
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, rot, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
