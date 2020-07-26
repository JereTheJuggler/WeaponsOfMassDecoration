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
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintStaff : PaintingProjectile{

        public PaintStaff() : base() {
            trailLength = 4;
            trailMode = 0;

            dropsOnDeath = true;
            dropCount = 5;
            dropCone = (int)(3f * Math.PI / 4f);
            dropVelocity = 6f;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Bolt");     //The English name of the projectile
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
            projectile.light = 1f;
            projectile.alpha = 150;
        }

        public override bool PreAI() {
            base.PreAI();
            if(canPaint()) {
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                paint(coords.X, coords.Y);
                for(int d = 0; d < 2; d++) {
                    Dust dust = getDust(Dust.NewDust(projectile.position, 0, 0, DustType<Dusts.LightDust>(), 0, 0, 200, getColor(this), .75f));
                    if(dust != null) {
                        dust.noGravity = true;
                        dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                        dust.fadeIn = 1.5f;
                        dust.alpha = 20;
                        dust.scale = 1.5f;
                    }
				}
            }
			projectile.ai[0] += .2f;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Main.PlaySound(SoundID.Item10, projectile.Center);
            return base.OnTileCollide(oldVelocity);
        }
    }
}
