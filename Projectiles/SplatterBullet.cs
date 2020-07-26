using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;
using WeaponsOfMassDecoration.Dusts;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    public class SplatterBullet : PaintingProjectile {

        public SplatterBullet() : base() {
            explodesOnDeath = true;
            explosionRadius = 48;

            trailLength = 5;
            trailMode = 1;

            yFrameCount = 1;

            manualRotation = true;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Splatter Bullet");     //The English name of the projectile
        }

        public override void SetDefaults() {
            base.SetDefaults();
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
            aiType = ProjectileID.Bullet;           //Act exactly like default Bullet
            projectile.light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            rotation += .2f;
            return true;
        }

		public override bool OnTileCollide(Vector2 oldVelocity) {
            if(canPaint()) {
                paint();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16, center.Y / 16);
                for(int i = 0; i < 64; i++) {
                    Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i))/16), (int)Math.Floor((center.Y + (oldVelocity.Y * i))/16));
                    if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
                        paint(coords.X, coords.Y);
                        break;
                    }
                }
            }
            blowUp();
            return false;
        }

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
            blowUp();
		}

		public override void OnHitPvp(Player target, int damage, bool crit) {
			base.OnHitPvp(target, damage, crit);
            blowUp();
		}

        public void blowUp() {
            projectile.position = projectile.Center - new Vector2(32, 32);
            projectile.Size = new Vector2(64, 64);
            projectile.timeLeft = 3;
            projectile.aiStyle = 0;
            projectile.velocity = new Vector2(0, 0); 
            
            Vector2 backwards = projectile.oldVelocity * -1;
            double splatterAngle = (Math.PI / 5) * 3;
            int spokes = 7;
            backwards = backwards.RotatedBy(splatterAngle / -2f);
            Color c = getColor(this);
            for(byte i = 0; i < spokes; i++) {
                Vector2 vel = backwards.RotatedBy((splatterAngle / (spokes - 1)) * i);
                int length = Main.rand.Next(7, 12);

                paintBetweenPoints(projectile.Center, projectile.Center + vel * length);
                for(int j = 0; j < length; j++) {
                    Dust dust = getDust(Dust.NewDust((projectile.Center + (vel * j)) - new Vector2(5, 5), 10, 10, DustType<LightDust>(), 0, 0, 0, c, 1f));
                    if(dust != null) {
                        dust.noGravity = true;
                        dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                        dust.fadeIn = 3f;
                        dust.scale = 1.5f;
                    }
                }
            }
            for(int i = 0; i < 15; i++) {
                Dust dust = getDust(Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, DustID.Smoke, 0f, 0f, 100, c, 1f));
                if(dust != null) {
                    dust.velocity *= .8f;
                }
            }
            Main.PlaySound(SoundID.Item14, projectile.Center);
        }

		public override bool PreKill(int timeLeft) {
            return base.PreKill(timeLeft);
		}
    }
}