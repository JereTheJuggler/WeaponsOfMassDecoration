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
    class PaintSplatter : PaintingProjectile{
        public int startingTimeLeft = 25;

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Splatter");     //The English name of the projectile
        }

        public override void SetDefaults() {
            base.SetDefaults();
            //projectile.velocity.Normalize();
            //projectile.velocity *= 5;
            //projectile.CloneDefaults(ProjectileID.DiamondBolt);
            projectile.width = 10;               //The width of projectile hitbox
            projectile.height = 10;              //The height of projectile hitbox
            projectile.gfxOffY = 5;
            projectile.aiStyle = 25;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.penetrate = 1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = startingTimeLeft;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            //projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            //projectile.light = 1f;            //How much light emit around the projectile
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.MagicDagger;           //Act exactly like default Bullet
            projectile.damage = 1;
            //projectile.Opacity = 0f;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paint(coords.X, coords.Y);
            }
            return true;
        }

		public override void PostAI() {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI/2f;
			base.PostAI();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
            if(canPaint()) {
                paint();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16, center.Y / 16);
                for(int i = 0; i < 64; i++) {
                    Point coords = new Point((int)Math.Floor((center.X + (oldVelocity.X * i)) / 16), (int)Math.Floor((center.Y + (oldVelocity.Y * i)) / 16));
                    if(coords.X > 0 && coords.Y > 0 && coords.X < Main.maxTilesX && coords.Y < Main.maxTilesY && WorldGen.SolidOrSlopedTile(coords.X, coords.Y)) {
                        paint(coords.X, coords.Y);
                        break;
                    }
                }
            }
            projectile.Kill();
            return false;
        }
    }
}
