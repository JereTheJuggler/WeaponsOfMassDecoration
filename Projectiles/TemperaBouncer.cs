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
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using WeaponsOfMassDecoration.Dusts;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class TemperaBouncer : PaintingProjectile{
		public float lastBounceTime = 0;

		public TemperaBouncer() : base() {
			usesShader = true;

			dropsOnDeath = true;
			dropCount = 10;
			dropCone = (float)(3 * Math.PI / 4);
			dropVelocity = 6f;

			yFrameCount = 1;
			xFrameCount = 1;

			trailLength = 5;
			trailMode = 0;

			manualRotation = true;
		}

        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
            DisplayName.SetDefault("Tempera Bouncer");     //The English name of the projectile
        }

        public override void SetDefaults() {
            //projectile.velocity.Normalize();
            //projectile.velocity *= 5;
            projectile.width = 18;               //The width of projectile hitbox
            projectile.height = 18;              //The height of projectile hitbox
			//projectile.aiStyle = 15;
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.magic = true;           //Is the projectile shoot by a ranged weapon?
			projectile.penetrate = 4;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 3600;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.Opacity = 1f;
			projectile.scale = 1.3f;
        }

        public override bool PreAI() {
            base.PreAI();
			if(canPaint()) {
				paint(new Vector2(projectile.Center.X + 8, projectile.Center.Y + 8));
				paint(new Vector2(projectile.Center.X + 8, projectile.Center.Y - 8));
				paint(new Vector2(projectile.Center.X - 8, projectile.Center.Y + 8));
				paint(new Vector2(projectile.Center.X - 8, projectile.Center.Y - 8));
				paintedTiles = new List<Point>();
			}
			return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            explode(projectile.Center, 45, true, true);
            if(projectile.penetrate < 0) {
                return true;
            }
            if(projectile.velocity.X != oldVelocity.X)
                projectile.velocity.X = oldVelocity.X * -1;
            else
                projectile.velocity.Y = oldVelocity.Y * -.9f;

			if(lastBounceTime == 0 || Main.GlobalTime - .05f >= lastBounceTime) {
				projectile.penetrate--;
				lastBounceTime = Main.GlobalTime;
			}
            return false;
        }
        public override bool PreKill(int timeLeft) {
			Main.PlaySound(SoundID.Item10,projectile.position);
            return base.PreKill(timeLeft);
        }
        public override void Kill(int timeLeft) {
            base.Kill(timeLeft);
        }

        public override void AI() {
			projectile.velocity.Y += .3f;
			rotation += (float)Math.PI / 10f * projectile.direction;
			for(int i = 0; i < 1; i++) { 
                Dust dust = getDust(Dust.NewDust(projectile.Center-new Vector2(projectile.width/2,projectile.height/2), projectile.width, projectile.height, DustType<LightDust>(), 0, 0, 0, getColor(this),1));
				if(dust != null) {
					dust.noGravity = true;
					dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
					dust.fadeIn = 3f;
					dust.scale = 1.5f;
				}
            }
        }
	}
}
