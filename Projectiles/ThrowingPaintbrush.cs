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
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Projectiles {
    class ThrowingPaintbrush : PaintingProjectile{
        public int splattersFlung = 0;

        public Vector2? lastPaintCoord = null;

        public ThrowingPaintbrush() : base() {
            dropsOnDeath = true;
            dropCount = 6;
            dropVelocity = 6f;

            explodesOnDeath = true;
            explosionRadius = 45;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Throwing Paintbrush");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.CloneDefaults(ProjectileID.ThrowingKnife);
            projectile.thrown = true;
            projectile.light = 0;

            aiType = ProjectileID.ThrowingKnife;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                Vector2 coords = projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI);
                paint(coords);
                if(Math.Abs(projectile.rotation - oldRotation) > Math.PI / 9) {
                    paint(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI + (Math.PI / 9)));
                    paint(projectile.Center + new Vector2(0, 24).RotatedBy(projectile.rotation + Math.PI - (Math.PI / 9)));
                }
                if(lastPaintCoord != null) {
                    paintBetweenPoints((Vector2)lastPaintCoord, coords);
				}
                lastPaintCoord = coords;
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Dig, projectile.Center);
            return true;
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return base.PreDraw(spriteBatch, lightColor);
            //Redraw the projectile with the color not influenced by light
            /*projectile.alpha = 0;
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            projectile.gfxOffY = frameHeight / 2;
            //int xOffset = (projectile.velocity.X < 0 ? 2f : -2f);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color color = projectile.GetAlpha(lightColor);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;*/
        }
    }
}
