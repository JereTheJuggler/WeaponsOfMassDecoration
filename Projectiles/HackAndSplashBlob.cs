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

namespace WeaponsOfMassDecoration.Projectiles {
    class HackAndSplashBlob : PaintingProjectile{
		const double coneAngle = 3 * Math.PI / 4;

		public int startingTimeLeft = 1000;
        public int startTime;

		public HackAndSplashBlob() : base() {
			dropsOnDeath = true;
			dropCount = 5;
			dropCone = (float)Math.PI / 2f;
			usesShader = true;

			explodesOnDeath = true;
			explosionRadius = 64f;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Splatter");     //The English name of the projectile
            //ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
        }

        public override void SetDefaults() {
            //projectile.velocity.Normalize();
            //projectile.velocity *= 5;
            //projectile.CloneDefaults(ProjectileID.DiamondBolt);
            projectile.width = 20;               //The width of projectile hitbox
            projectile.height = 20;              //The height of projectile hitbox
            projectile.gfxOffY = 12;
            startTime = (int)Math.Floor(Main.GlobalTime*3);
			projectile.aiStyle = 0;// 25;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.penetrate = 2;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = startingTimeLeft;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            //projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            //projectile.light = 1f;            //How much light emit around the projectile
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.MagicDagger;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 31;
            projectile.damage = 20;
            projectile.Opacity = .75f;
            animationFrames = 6;
            light = .5f;
            projectile.melee = true;
        }

        public override bool PreAI() {
			if(color >= 0) {
                //projectile.frame = color;
                Point coords = new Point((int)Math.Floor(projectile.Center.X / 16), (int)Math.Floor(projectile.Center.Y / 16));
                //paintTileAndWall(coords.X, coords.Y);
                int armLength = 48;
                Vector2 arm1 = new Vector2((float)Math.Cos(projectile.rotation + (Math.PI / 6) + (Math.PI/2)), (float)Math.Sin(projectile.rotation + (Math.PI / 6) + (Math.PI / 2)));
                Vector2 arm2 = new Vector2((float)Math.Cos(projectile.rotation - (Math.PI / 6) + (Math.PI / 2)), (float)Math.Sin(projectile.rotation - (Math.PI / 6) + (Math.PI / 2)));
                for(int offset = 8; offset <= armLength; offset += 8) {
                    Point tile1 = new Point(
                        (int)Math.Round((projectile.Center.X + (arm1.X * offset)) / 16),
                        (int)Math.Round((projectile.Center.Y + (arm1.Y * offset)) / 16)
                    );
                    Point tile2 = new Point(
                        (int)Math.Round((projectile.Center.X + (arm2.X * offset)) / 16),
                        (int)Math.Round((projectile.Center.Y + (arm2.Y * offset)) / 16)
                    );
                    paintTileAndWall(tile1);
                    paintTileAndWall(tile2);
                }
			}
			base.PreAI();
			return true;
        }

		public override void AI() {
			projectile.velocity.Y += .4f;
			projectile.rotation = projectile.velocity.ToRotation() + (float)(Math.PI / 2f);
		}

		public override void PostAI() {
			for(byte i = 2; i > 0; i--) {
				projectile.oldPos[i] = projectile.oldPos[i - 1];
				projectile.oldRot[i] = projectile.oldRot[i - 1];
			}
			projectile.oldPos[0] = projectile.position;
			projectile.oldRot[0] = projectile.rotation;
			//base.PostAI();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            //explode(new Vector2(projectile.position.X / 16f, projectile.position.Y / 16f), 32f, true, true);
            base.OnHitNPC(target, damage, knockback, crit);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
			if(color >= 0) {
                paintTileAndWall();
                oldVelocity.Normalize();
                Vector2 center = projectile.Center.ToWorldCoordinates(0, 0);
                center = new Vector2(center.X / 16, center.Y / 16);
                explode(center, 32f, true, true);
            }
			Main.PlaySound(SoundID.Item21, projectile.Center);
            projectile.Kill();
            return false;
        }

		public override bool PreKill(int timeLeft) {
			return base.PreKill(timeLeft);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if(Main.netMode != NetmodeID.Server) {

				//createLight(projectile.Center, 1f);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				resetBatchInPost = true;

				projectile.frame++;
				if(projectile.frame >= animationFrames)
					projectile.frame = 0;

				Texture2D texture = Main.projectileTexture[projectile.type];
				int frameHeight = texture.Height;
				int frameY = 0;
				int frameWidth = (texture.Width + 2) / animationFrames - 2;
				int frameX = (frameWidth + 2) * projectile.frame;
				Rectangle sourceRectangle = new Rectangle(frameX, frameY, frameWidth, frameHeight);

				MiscShaderData data = WeaponsOfMassDecoration.applyShader(this) as MiscShaderData;
				
				Vector2 origin = new Vector2(frameWidth / 2, frameWidth / 2);

				Vector2 drawPos = projectile.Center - Main.screenPosition;
				SpriteEffects effect = (projectile.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

				for(int i = 2; i >= 0; i--) {
					Vector2 pos = drawPos - (projectile.velocity * (.5f * (i + 1f)));
					float opacity = projectile.Opacity - ((projectile.Opacity / 4) * (i + 1));
					float scale = projectile.scale * (1f - (.1f * (i + 1f)));
					float lightness = 1f - (.25f * (i + 1f));

					if(data != null)
						data.UseOpacity(opacity).Apply();

					spriteBatch.Draw(texture, pos, sourceRectangle, new Color(lightness, lightness, lightness, opacity).MultiplyRGB(lightColor), projectile.rotation, origin, scale, effect, 0);

					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
				}

				if(data != null)
					data.UseOpacity(projectile.Opacity).Apply();

				spriteBatch.Draw(texture, drawPos, sourceRectangle, new Color(1f,1f,1f,projectile.Opacity).MultiplyRGB(lightColor), projectile.rotation, origin, projectile.scale, effect, 0f);
			}
			return false;
        }
    }
}
