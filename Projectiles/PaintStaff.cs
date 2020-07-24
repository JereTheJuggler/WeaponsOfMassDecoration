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

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintStaff : PaintingProjectile{
        const double coneAngle = 3 * Math.PI / 4;
        const int numSplatters = 5;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Bolt");     //The English name of the projectile
            //ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
        }

        public override void SetDefaults() {
            //projectile.velocity.Normalize();
            //projectile.velocity *= 5;
            //projectile.CloneDefaults(ProjectileID.DiamondBolt);
            projectile.width = 10;               //The width of projectile hitbox
            projectile.height = 10;              //The height of projectile hitbox
            //projectile.aiStyle = 29;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.aiStyle = 0;
			projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.magic = true;           //Is the projectile shoot by a ranged weapon?
            projectile.penetrate = 1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 3600;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 200;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.DiamondBolt;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
            light = 1f;
            projectile.alpha = 150;
        }

        public override bool PreAI() {
            base.PreAI();
            projectile.rotation = 0;
			
            if(color >= 0) {
                //projectile.frame = color;
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paintTileAndWall(coords.X, coords.Y);
                for(int d = 0; d < 2; d++) {
					//if(Main.rand.NextFloat() <= .75f) {
					int dustId = Dust.NewDust(projectile.Center - new Vector2(7,7) + (projectile.velocity*.5f), 14, 14, ModContent.DustType<Dusts.LightDust>(), 0, 0, 200, WeaponsOfMassDecoration.getColor(this), .75f);

					Dust dust = Main.dust[dustId];
					dust.noGravity = true;
					dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
					dust.fadeIn = 1.5f;
					dust.alpha = 20;
					dust.scale = 1.5f;
					/*int maxTimeLeft = 0;

					float amplitude = 20f;
					Vector2 normVelocity = projectile.velocity.RotatedBy(Math.PI / 2);
					normVelocity.Normalize();
					Vector2 dustPosition = projectile.Center + (normVelocity * (float)(amplitude * Math.Sin(maxTimeLeft - projectile.timeLeft)));
					*/
					//projectile.Hitbox = new Rectangle((int)Math.Round(dustPosition.X - projectile.Hitbox.Width), (int)Math.Round(dustPosition.Y - projectile.Hitbox.Height), projectile.Hitbox.Width, projectile.Hitbox.Height);

					//int dustId = Dust.NewDust(projectile.Center + (normVelocity * (float)(20f * Math.Sin(projectile.ai[0]))), 0, 0, ModContent.DustType<Dusts.LightDust>(), 0, 0, 200, getLightColor(), .75f);
					//}
				}
            }
			projectile.ai[0] += .2f;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Item10, projectile.Center);
			splat(oldVelocity);
            return true;
        }

		public override void onKillOnNPC(NPC target) {
			splat(projectile.oldVelocity);
		}

		public void splat(Vector2 velo) {
			if(color >= 0) {
				explode(projectile.Center, 45, true, true);
			}
			Vector2 backwards = velo.RotatedBy(Math.PI);
			backwards.Normalize();
			backwards *= 6f;
			for(int i = 0; i < numSplatters; i++) {
				Vector2 vel = backwards.RotatedBy(((coneAngle) * Main.rand.NextFloat()) - (coneAngle / 2));
				int projId = Projectile.NewProjectile(projectile.Center + vel * 2f, vel, ModContent.ProjectileType<PaintSplatter>(), 0, 0, projectile.owner, 1, ProjectileID.IchorSplash);
				Main.projectile[projId].timeLeft = 30;
				Main.projectile[projId].alpha = 125;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (texture.Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            createLight(projectile.Center, 1f);
            projectile.gfxOffY = texture.Width /2;
            //float xOffset = (projectile.velocity.X < 0 ? 2f : -2f);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            //Vector2 origin = new Vector2((texture.Width / 2f) * (float)Math.Cos(projectile.rotation + (float)Math.PI / xOffset), (texture.Width / 2f) * (float)Math.Sin(projectile.rotation + (float)Math.PI / xOffset));//new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) * (frameHeight/-4f);
            Vector2 origin = new Vector2(texture.Width/2+1,texture.Width/2+1);
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
