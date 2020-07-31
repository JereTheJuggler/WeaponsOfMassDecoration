using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics;

namespace WeaponsOfMassDecoration.Projectiles {
    public class Paintball : PaintingProjectile {
        
        public Paintball() {
            trailLength = 5;

            usesGSShader = true;

            xFrameCount = 1;
            yFrameCount = 2;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paintball");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 8;
            projectile.height = 8;
            projectile.aiStyle = 1;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.alpha = 255;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.extraUpdates = 1;
            aiType = ProjectileID.Bullet;
            projectile.light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                paintAlongOldVelocity(projectile.oldVelocity);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(canPaint()) {
                paint();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16f, center.Y / 16f);
                for(int i = 0; i < 64; i++) {
                    Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i))/16f), (int)Math.Floor((center.Y + (oldVelocity.Y * i))/16f));
                    if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
                        paint(coords.X, coords.Y);
                        break;
                    }
                }
            }
            Main.PlaySound(SoundID.Dig,projectile.Center);
            projectile.Kill();
            return false;
        }

        protected override int convertColorFrame() {
            if(colorFrame == PaintID.Negative)
                return 1;
            return 0;
        }
    }
}