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
    class ThrowingPaintbrush : PaintingProjectile{
        public double coneAngle = 3 * Math.PI / 4;
        public int numSplatters = 6;
        public int splattersFlung = 0;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Throwing Paintbrush");
            base.SetStaticDefaults();

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ThrowingKnife);
            projectile.thrown = true;
            light = 0;

            aiType = ProjectileID.ThrowingKnife;
            Main.projFrames[projectile.type] = 31;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paintTileAndWall(projectile.Center+new Vector2(0,24).RotatedBy(projectile.rotation+Math.PI));
                if(Math.Abs(projectile.rotation - projectile.oldRot[0]) > Math.PI / 9) {
                    paintTileAndWall(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI + (Math.PI / 9)));
                    paintTileAndWall(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI - (Math.PI / 9)));
                }
            }
            return true;
        }

        public override void AI() {
            base.AI();
            //if((projectile.timeLeft + 1) % 5 == 0 && Math.Abs(projectile.oldRot[0] - projectile.rotation) >= Math.PI/15 && projectile.velocity.Length() >= 4.5) {// && splattersFlung < 3) {
            //    int projId = Projectile.NewProjectile(projectile.Center + new Vector2(0, 16).RotatedBy(projectile.rotation+Math.PI), new Vector2(0, 5).RotatedBy(projectile.rotation + Math.PI)*Math.Abs(projectile.rotation-projectile.oldRot[0]), mod.ProjectileType<PaintSplatter>(), 0, 0, projectile.owner, 0, 0);
            //    Main.projectile[projId].timeLeft = 30;
            //    Main.projectile[projId].alpha = 125;
            //    splattersFlung++;
            //}
            projectile.oldRot[0] = projectile.rotation;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Dig, projectile.Center);
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
            projectile.alpha = 0;
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            projectile.gfxOffY = frameHeight / 2;
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
