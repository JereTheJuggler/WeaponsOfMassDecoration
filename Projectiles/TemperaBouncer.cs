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
    class TemperaBouncer : PaintingProjectile{
        public float rot = 0;
        const double coneAngle = 3 * Math.PI / 4;
        const int numSplatters = 10;

		public float lastBounceTime = 0;

		public TemperaBouncer() : base() {
			usesShader = true;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tempera Bouncer");     //The English name of the projectile
            //ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;    //The length of old position to be recorded
            //ProjectileID.Sets.TrailingMode[projectile.type] = 0;        //The recording mode
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
            //projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            //aiType = ProjectileID.BallofFire;           //Act exactly like default Bullet
            Main.projFrames[projectile.type] = 1;
			projectile.Opacity = 1f;
			projectile.scale = 1.3f;
            //projectile.Opacity = 0f;
        }

        public override bool PreAI() {
            base.PreAI();
            if(color >= 0) {
                //projectile.frame = color;
                paintTileAndWall(new Vector2(projectile.Center.X + 8, projectile.Center.Y + 8));
                paintTileAndWall(new Vector2(projectile.Center.X + 8, projectile.Center.Y - 8));
                paintTileAndWall(new Vector2(projectile.Center.X - 8, projectile.Center.Y + 8));
                paintTileAndWall(new Vector2(projectile.Center.X - 8, projectile.Center.Y - 8));
                paintedTiles = new List<Point>();
            }
			return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(color >= 0) {
                explode(projectile.Center, 45, true, true);
            }
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
            Vector2 backwards = projectile.oldVelocity.RotatedBy(Math.PI);
            backwards.Normalize();
            backwards *= 6f;
            for(int i = 0; i < numSplatters; i++) {
                Vector2 vel = backwards.RotatedBy(((coneAngle) * Main.rand.NextFloat()) - (coneAngle / 2));
                int projId = Projectile.NewProjectile(projectile.Center + vel * 2f, vel, ModContent.ProjectileType<PaintSplatter>(), 0, 0, projectile.owner, 1, ProjectileID.IchorSplash);
				Main.projectile[projId].timeLeft = 30;
                Main.projectile[projId].alpha = 125;
            }
			Main.PlaySound(SoundID.Item10,projectile.position);
            return base.PreKill(timeLeft);
        }
        public override void Kill(int timeLeft) {
            base.Kill(timeLeft);
        }

        public override void AI() {
			//projectile.velocity *= 1.01f;
			//if(projectile.velocity.Y < 8)
			projectile.velocity.Y += .3f;
			/*if(projectile.velocity.Length() >= 15) {
				projectile.velocity *= .98f;
			}*/
			rot += (float)Math.PI / 10f * projectile.direction;
			//if(Math.Abs(projectile.velocity.X) >= 8)
			//    projectile.velocity.X *= .97f;

			//if(projectile.velocity.Y < 0) {
			//    projectile.velocity *= .99f;
			//}else {
			//    projectile.velocity *= 1.01f;
			//}
			for(int i = 0; i < 2; i++) { 
                //int dustId = Dust.NewDust(projectile.Center, 0, 0, 74, 0, 0, 0, getLightColor(), 1);
                int dustId = Dust.NewDust(projectile.Center-new Vector2(projectile.width/2,projectile.height/2), projectile.width, projectile.height, mod.DustType("LightDust"), 0, 0, 0, WeaponsOfMassDecoration.getColor(this),1);
                Dust dust = Main.dust[dustId];
                dust.noGravity = true;
				dust.velocity = new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                dust.fadeIn = 3f;
                //dust.alpha = 125;
				dust.scale = 1.5f;
            }
        }

		public override void PostAI() {
			projectile.rotation = rot;
			for(byte i = 2; i > 0; i--) {
				projectile.oldPos[i] = projectile.oldPos[i - 1];
				projectile.oldRot[i] = projectile.oldRot[i - 1];
			}
			projectile.oldPos[0] = projectile.position;
			projectile.oldRot[0] = projectile.rotation;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if(Main.netMode != NetmodeID.Server) {

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
				
				createLight(projectile.Center, .5f);

				projectile.scale = 1.3f;

				resetBatchInPost = true;
				projectile.frame = 0;

				Texture2D texture = Main.projectileTexture[projectile.type];
				int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
				int startY = (frameHeight + 2) * projectile.frame;
				Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
				Vector2 drawPos = projectile.Center - Main.screenPosition;
				
				MiscShaderData data = (MiscShaderData) WeaponsOfMassDecoration.applyShader(this);
				
				Vector2 origin = new Vector2(frameHeight / 2, frameHeight / 2);

				for(int i = 2; i >= 0; i--) {
					Vector2 oldPosition = projectile.oldPos[i] + (projectile.Size / 2) - Main.screenPosition;
					float oldRotation = projectile.oldRot[i];
					float opacity = projectile.Opacity - (projectile.Opacity / 4) * (i + 1); //projectile.Opacity - ((projectile.Opacity / 3) * (float)(i + 1f));
					float scale = projectile.scale * (1f - (.1f * (i + 1f)));
					float lightness = (1f - (.2f * (i + 1f)));
					Color c = new Color(lightness, lightness, lightness, opacity).MultiplyRGB(lightColor);
					if(data != null)
						data.UseOpacity(opacity).Apply();

					spriteBatch.Draw(texture, oldPosition, sourceRectangle, c, projectile.rotation, origin, scale, SpriteEffects.None, 0);
					
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
				}

				if(data != null)
					data.UseOpacity(projectile.Opacity).Apply();

				spriteBatch.Draw(
					texture,
					drawPos,
					sourceRectangle,
					new Color(1f, 1f, 1f, projectile.Opacity).MultiplyRGB(lightColor),
					rot,
					origin,
					projectile.scale,
					SpriteEffects.None,
					0
				);

				//WeaponsOfMassDecoration.applyShader(projectile, shader, new DrawData(texture, drawPos, sourceRectangle, Color.White, rot, origin, 1f, projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0));

				//getTrueColor(true);

				//GameShaders.Armor.GetShaderFromItemId(ItemID.RedDye).UseColor(color).UseOpacity(.5f).Apply(projectile);
				//GameShaders.Armor.GetShaderFromItemId(ItemID.RedDye).UseColor(color).Apply(projectile,);
				//spriteBatch.Draw(texture, drawPos, sourceRectangle, color, rot, origin, projectile.scale, projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
				//GameShaders.Misc["TemperaBouncer"].UseColor(color).Apply();
			}
			return false;
        }

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			base.PostDraw(spriteBatch, lightColor);
		}
	}
}
