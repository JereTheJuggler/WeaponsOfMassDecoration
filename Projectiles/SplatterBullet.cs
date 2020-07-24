using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;
using WeaponsOfMassDecoration.Dusts;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Projectiles {
    public class SplatterBullet : PaintingProjectile {

        public float rot;

        public SplatterBullet() : base() {
            explodesOnDeath = true;
            explosionRadius = 16;
            rot = 0;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Splatter Bullet");     //The English name of the projectile
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
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
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.Bullet;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
            light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            rot += .2f;
            if(color >= 0) {
                paintAlongOldVelocity(projectile.oldVelocity);
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
            projectile.Kill();
            return false;
        }

		public override bool PreKill(int timeLeft) {
            Vector2 backwards = projectile.oldVelocity * -1;
            double splatterAngle = (Math.PI / 5) * 3;
            int spokes = 7;
            backwards = backwards.RotatedBy(splatterAngle / -2f);
            Color c = WeaponsOfMassDecoration.getColor(this);
			for(byte i = 0; i < spokes; i++) {
                Vector2 vel = backwards.RotatedBy((splatterAngle / (spokes - 1)) * i);
                int length = Main.rand.Next(7, 12);
                
                paintBetweenPoints(projectile.Center, projectile.Center + vel * length);
                for(int j = 0; j < length; j++) {
                    for(int x = 0; x < 2; x++) {
                        int dustId = Dust.NewDust((projectile.Center + (vel * j)) - new Vector2(5, 5), 10, 10, DustType<LightDust>(), 0, 0, 0, c, 1f);
                        Dust dust = Main.dust[dustId];
                        dust.noGravity = true;
                        dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                        dust.fadeIn = 3f;
                        //dust.alpha = 125;
                        dust.scale = 1.5f;
                    }
                }
			}
            for(int i = 0; i < 15; i++) {
                int dustIndex = Dust.NewDust(projectile.Center - new Vector2(16,16), 32, 32, DustID.Smoke, 0f, 0f, 100, c, 1f);
				Main.dust[dustIndex].velocity *= .8f;
            }
            int projectileId = Projectile.NewProjectile(projectile.Center - new Vector2(32, 32), new Vector2(0, 0), ProjectileType<DamageProjectile>(), projectile.damage, projectile.knockBack, projectile.owner);
            Projectile p = Main.projectile[projectileId];
            p.position = projectile.Center - new Vector2(32, 32);
            p.Size = new Vector2(64, 64);
            p.ranged = true;
            p.active = true;
            Main.PlaySound(SoundID.Item14, projectile.Center);
            return base.PreKill(timeLeft);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            createLight(projectile.Center, light);
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = texture.Height;
            int startY = 0;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width/2, texture.Width / 2);
            for(int k = 0; k < projectile.oldPos.Length; k++) {
                Vector2 drawPos = projectile.oldPos[k]-Main.screenPosition+(origin/2);
                Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, sourceRectangle, color, rot, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}