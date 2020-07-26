using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Dusts;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Projectiles {
    class SplatterStaff : PaintingProjectile{
        public Point mousePosition;
        public float distance = -1;

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Bolt");     //The English name of the projectile
        }

        public override void SetDefaults() {
            base.SetDefaults();
            mousePosition = Main.MouseWorld.ToTileCoordinates();
            //projectile.velocity.Normalize();
            //projectile.velocity *= 5;
            projectile.width = 80;               //The width of projectile hitbox
            projectile.height = 80;              //The height of projectile hitbox
            projectile.aiStyle = 0;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = false;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.magic = true;           //Is the projectile shoot by a ranged weapon?
            projectile.penetrate = 1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 3600;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 200;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = false;          //Can the projectile collide with tiles?
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.DiamondBolt;           //Act exactly like default Bullet
            projectile.light = 1f;
            //projectile.Opacity = 0f;
        }

        public override bool PreAI() {
            base.PreAI();
            double xPos = projectile.Center.X / 16;
            double yPos = projectile.Center.Y / 16;
            double displacement = Math.Sqrt(Math.Pow(xPos - mousePosition.X, 2) + Math.Pow(yPos - mousePosition.Y, 2));
            if(displacement <= 1) {
                projectile.timeLeft = 1;
            } else if(distance == -1) {
                if(startPosition.X != 0 && startPosition.Y != 0) {
                    distance = (float)Math.Sqrt(Math.Pow(startPosition.X/16 - mousePosition.X, 2) + Math.Pow(startPosition.Y/16 - mousePosition.Y, 2));
                }
            } else {
                float projDistance = (float)Math.Sqrt(Math.Pow(xPos - startPosition.X/16, 2) + Math.Pow(yPos - startPosition.Y/16, 2));
                if(projDistance >= distance) {
                    projectile.timeLeft = 1;
                }
            }
            if(projectile.timeLeft == 1) {
                projectile.friendly = true;
                return false;
            }
            return true;
        }

        public override void AI() {
            for(int i = 0; i < 2; i++) {
				Vector2 speed = new Vector2(2, 0).RotatedByRandom(Math.PI * 2);
                Dust dust = getDust(Dust.NewDust(projectile.Center, 0, 0, mod.DustType("LightDust"), speed.X, speed.Y , 0, getColor(this), 1));
                if(dust != null) {
                    dust.noGravity = true;
                    dust.fadeIn = 1.5f;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            return true;
        }

        public override void Kill(int timeLeft) {
            splatter(projectile.Center, 128, 8, true, true);
            createLight(projectile.Center, 1f);
            Main.PlaySound(SoundID.Item14, projectile.position);
            //smoke dust
            for(int i = 0; i < 15; i++) {
                Dust d = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, getColor(this), 2f));
                if(d != null)
                    d.velocity *= 1.4f;
            }
        }
    }
}
