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

			trailLength = 3;

			manualRotation = true;
		}

        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
            DisplayName.SetDefault("Tempera Bouncer");
        }

        public override void SetDefaults() {
            projectile.width = 18;
            projectile.height = 18;
			projectile.friendly = true;
            projectile.hostile = false;
            projectile.magic = true;
			projectile.penetrate = 4;
            projectile.timeLeft = 3600;
            projectile.alpha = 0;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.Opacity = 1f;
			projectile.scale = 1.3f;
            projectile.light = .5f;
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
                Dust dust = getDust(Dust.NewDust(projectile.Center-new Vector2(projectile.width/2,projectile.height/2), projectile.width, projectile.height, DustType<PaintDust>(), 0, 0, 0, getColor(this),1));
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
