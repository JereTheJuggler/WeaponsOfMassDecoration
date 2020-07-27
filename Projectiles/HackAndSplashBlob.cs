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
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class HackAndSplashBlob : PaintingProjectile{
		public HackAndSplashBlob() : base() {
			dropsOnDeath = true;
			dropCount = 5;
			dropCone = (float)Math.PI / 2f;
			usesShader = true;

			explodesOnDeath = true;
			explosionRadius = 64f;

			yFrameCount = 1;
			xFrameCount = 6;

			trailLength = 3;

            animationFrameDuration = 4;

			drawOriginOffset = new Vector2(0, -5);
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Splatter");
        }

        public override void SetDefaults() {
			base.SetDefaults();
            projectile.width = 20;
            projectile.height = 20;
            projectile.gfxOffY = 12;
			projectile.aiStyle = 0;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.penetrate = 2;
            projectile.timeLeft = 1000;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            aiType = ProjectileID.MagicDagger;
            projectile.damage = 20;
            projectile.Opacity = .75f;
            projectile.light = .5f;
            projectile.melee = true;
        }

        public override bool PreAI() {
			if(canPaint()) {
                int armLength = 48;
				Vector2 arm1 = new Vector2((float)Math.Cos(projectile.rotation + (Math.PI / 6f) + (Math.PI / 2f)), (float)Math.Sin(projectile.rotation + (Math.PI / 6f) + (Math.PI / 2f)));
                Vector2 arm2 = new Vector2((float)Math.Cos(projectile.rotation - (Math.PI / 6f) + (Math.PI / 2f)), (float)Math.Sin(projectile.rotation - (Math.PI / 6f) + (Math.PI / 2f)));
                for(int offset = 8; offset <= armLength; offset += 8) {
                    Point tile1 = new Point(
                        (int)Math.Round((projectile.Center.X + (arm1.X * offset)) / 16f),
                        (int)Math.Round((projectile.Center.Y + (arm1.Y * offset)) / 16f)
                    );
                    Point tile2 = new Point(
                        (int)Math.Round((projectile.Center.X + (arm2.X * offset)) / 16f),
                        (int)Math.Round((projectile.Center.Y + (arm2.Y * offset)) / 16f)
                    );
                    paint(tile1);
                    paint(tile2);
                }
			}
			base.PreAI();
			return true;
        }

		public override void AI() {
			projectile.velocity.Y += .4f;
			projectile.rotation = projectile.velocity.ToRotation() + (float)(Math.PI / 2f);
		}

        public override bool OnTileCollide(Vector2 oldVelocity) {
			if(canPaint()) {
                paint();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16f, center.Y / 16f);
                explode(center, 32f, true, true);
            }
			Main.PlaySound(SoundID.Item21, projectile.Center);
            projectile.Kill();
            return false;
        }
    }
}
