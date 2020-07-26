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
            trailMode = 0;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paintball");     //The English name of the projectile
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
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.Bullet;           //Act exactly like default Bullet
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
    }
}