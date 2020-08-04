using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Projectiles;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	/// <summary>
	/// The base class for all of the custom projectiles in this mod
	/// </summary>
	public abstract class PaintingProjectile : ModProjectile {
		/// <summary>
		/// Used to specify that the projectile should use the generic functionality for creating a circle of paint when it is killed
		/// </summary>
		protected bool explodesOnDeath = false;
		/// <summary>
		/// The radius of the explosion that will be used if explodesOnDeath is true. Uses world distance
		/// </summary>
		protected float explosionRadius = 32f;

		/// <summary>
		/// Used to specify that the projectile should use the generic functionality for creating drops of paint when it is killed
		/// </summary>
		protected bool dropsOnDeath = false;
		/// <summary>
		/// The number of drops that will be created when the projectile is killed if dropsOnDeath is true
		/// </summary>
		protected int dropCount = 5;
		/// <summary>
		/// The spread of the drops that will be created when the projectile is killed if dropsOnDeath is true
		/// </summary>
		protected float dropCone = (float)(Math.PI / 2f);
		/// <summary>
		/// The speed of the drops that will be created when the projectile is killed if dropsOnDeath is true
		/// </summary>
		protected float dropVelocity = 5f;

		/// <summary>
		/// The length of the projectile's trail for rendering
		/// </summary>
		protected int trailLength = 0;

		/// <summary>
		/// The offset from Projectile.Center to use for the drawing origin
		/// </summary>
		protected Vector2 drawOriginOffset = new Vector2(0, 0);

		/// <summary>
		/// This should be true for projectiles that need to be shaded entirely
		/// </summary>
		protected bool usesShader = false;
		/// <summary>
		/// This should be true for projectiles that only need some of their sprite to be shaded
		/// </summary>
		public bool usesGSShader = false;

		/// <summary>
		/// A list of the tiles that the projectile has painted throughout its lifetime
		/// </summary>
		public List<Point> paintedTiles = new List<Point>();
		public Vector2 startPosition = new Vector2(0, 0);

		/// <summary>
		/// If this is true, rotation will be used for storing trail information instead of projectile.rotation
		/// </summary>
		public bool manualRotation = false;
		public float rotation = 0;

		/// <summary>
		/// The projectile's rotation at the end of the last update
		/// </summary>
		public float oldRotation = 0;

		/// <summary>
		/// The brightness of the light that the projectile emits. Can't use Projectile.light because that makes the projectile emit a white light with the same brightness
		/// </summary>
		public float light = 0;

		/// <summary>
		/// Whether or not the projectile has a sprite
		/// </summary>
		public bool hasGraphics = true;

		/// <summary>
		/// The number of frames along the y axis of the projectile's sprite
		/// </summary>
		protected int yFrameCount = 31;
		/// <summary>
		/// The number of frames along the x axis of the projectile's sprite
		/// </summary>
		protected int xFrameCount = 1;

		/// <summary>
		/// The number of updates between advancing the projectile's animation
		/// </summary>
		public int animationFrameDuration = 0;

		/// <summary>
		/// The current frame of animation for the projectile. Animation frames go along the x axis of the projectile's sprite
		/// </summary>
		public int animationFrame = 0;
		/// <summary>
		/// The current frame along the y axis based on the projectile's current color
		/// </summary>
		public int colorFrame = 0;

		/// <summary>
		/// The current frame of the projectile. Automatically calculated by updateFrame
		/// </summary>
		public int frame = 0;

		/// <summary>
		/// Specifies the whoAmI of the npc that owns this projectile
		/// </summary>
		public int npcOwner = -1;

		/// <summary>
		/// Used to specify that the spritebatch should be reset in the postDraw event hook
		/// </summary>
		protected bool resetBatchInPost = false;

		protected PaintData _paintData = null;
		public PaintData paintData {
			get {
				if(_paintData == null) {
					_paintData = getPaintData();
				}
				return _paintData;
			}
		}

		public PaintingProjectile() : base() {
			projectile.light = 0;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = yFrameCount * xFrameCount;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = trailLength;
			if(trailLength > 1) {
				ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			}
		}

		#region packets
		/// <summary>
		/// Sends a ModPacket to sync the npc owner of a PaintingProjectile
		/// </summary>
		/// <param name="p"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void sendProjNPCOwnerPacket(PaintingProjectile p, int toClient = -1, int ignoreClient = -1) {
			if(server() || multiplayer()) {
				ModPacket packet = p.mod.GetPacket();
				packet.Write(WoMDMessageTypes.SetProjNPCOwner);
				packet.Write(p.projectile.whoAmI);
				packet.Write(p.projectile.type);
				packet.Write(p.npcOwner);
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Sets the npc owner of the projectile specified in ModPacket
		/// </summary>
		/// <param name="reader"></param>
		public static void readProjNPCOwnerPacket(BinaryReader reader) {
			int projId = reader.ReadInt32();
			int projType = reader.ReadInt32();
			int owner = reader.ReadInt32();
			Projectile proj = getProjectile(projId);
			if(proj != null && proj.type == projType && proj.active) {
				PaintingProjectile p = proj.modProjectile as PaintingProjectile;
				if(p != null) {
					p.npcOwner = owner;
				}
			}
		}

		/// <summary>
		/// Sends a ModPacket to sync the npc owner of multiple PaintingProjectile objects
		/// </summary>
		/// <param name="p"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void sendMultiProjNPCOwnerPacket(IEnumerable<PaintingProjectile> projectiles, int toClient = -1, int ignoreClient = -1) {
			if(projectiles.Count() == 0)
				return;
			if(server() || multiplayer()) {
				ModPacket packet = ModLoader.GetMod("WeaponsOfMassDecoration").GetPacket();
				packet.Write(projectiles.First().npcOwner);
				packet.Write(WoMDMessageTypes.SetMultiProjNPCOwner);
				packet.Write(projectiles.Count());
				foreach(PaintingProjectile p in projectiles) {
					packet.Write(p.projectile.whoAmI);
					packet.Write(p.projectile.type);
				}
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Sets the npc owner of the multiple projectiles specified in ModPacket
		/// </summary>
		/// <param name="reader"></param>
		public static void readMultiProjNPCOwnerPacket(BinaryReader reader) {
			int owner = reader.ReadInt32();
			int count = reader.ReadInt32();
			for(int i = 0; i < count; i++) {
				int projId = reader.ReadInt32();
				int projType = reader.ReadInt32();
				Projectile p = getProjectile(projId);
				if(p != null && p.type == projType) {
					PaintingProjectile proj = p.modProjectile as PaintingProjectile;
					if(proj != null)
						proj.npcOwner = owner;
				}
			}
		}
		#endregion

		#region getters / value conversion
		/// <summary>
		/// Safely attempts to get the Player of the projectile's owner. Returns null if the Player cannot be obtained
		/// </summary>
		/// <returns></returns>
		public Player getOwner() {
			return getPlayer(projectile.owner);
		}

		/// <summary>
		/// Safely attempts to get the ModPlayer of the projectile's owner. Returns null if the ModPlayer cannot be obtained
		/// </summary>
		/// <returns></returns>
		public WoMDPlayer getModPlayer() {
			Player p = getOwner();
			if(p == null)
				return null;
			return p.GetModPlayer<WoMDPlayer>();
		}

		/// <summary>
		/// Determines whether or not the projectile is currently able to paint.
		/// </summary>
		/// <returns></returns>
		public bool canPaint() {
			if(npcOwner != -1)
				return true;
			if(projectile.owner != Main.myPlayer)
				return false;
			WoMDPlayer player = getModPlayer();
			if(player == null)
				return false;
			return player.canPaint();
		}

		public Point convertPositionToTile(Vector2 position) {
			return new Point((int)Math.Floor(position.X / 16f), (int)Math.Floor(position.Y / 16f));
		}
		public Point convertPositionToTile(Point position) {
			return new Point((int)Math.Floor(position.X / 16f), (int)Math.Floor(position.Y / 16f));
		}

		public PaintMethods getPaintMethod() {
			if(npcOwner != -1)
				return PaintMethods.BlocksAndWalls;
			WoMDPlayer p = getModPlayer();
			if(p == null)
				return PaintMethods.BlocksAndWalls;
			return p.paintData.paintMethod;
		}
		#endregion

		#region tile/npc interaction
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			WoMDNPC npc = target.GetGlobalNPC<WoMDNPC>();
			WoMDPlayer player = getModPlayer();
			if(npc != null && player != null) {
				PaintMethods method = player.paintData.paintMethod;
				if(method != PaintMethods.None) {
					if(method == PaintMethods.RemovePaint) {
						npc.painted = false;
						int index = target.FindBuffIndex(BuffType<Painted>());
						if(index >= 0)
							target.DelBuff(index);
					} else {
						applyPaintedToNPC(target, new PaintData(npcCyclingTimeScale, player.paintData.paintColor, player.paintData.customPaint, player.paintData.customPaint is ISprayPaint, Main.GlobalTime, player: player.player));
					}
				}
			}
			if(projectile.penetrate == 1)
				onKillOnNPC(target);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			WoMDPlayer player = getModPlayer();
			if(player != null) {
				if(player.paintData.paintMethod == PaintMethods.None ||
				   player.paintData.paintMethod == PaintMethods.RemovePaint ||
				   (player.paintData.paintColor == -1 && player.paintData.customPaint == null)) {
					damage = (int)Math.Round(damage * .5f);
				}
			}
		}

		public virtual void onKillOnNPC(NPC target) {

		}

		public override void OnHitPvp(Player target, int damage, bool crit) {
			/*base.OnHitPvp(target, damage, crit);
            target.AddBuff(BuffType<Buffs.Painted>(), 600);
            Player p = getOwner();
            if(currentPaintIndex >= 0 && p != null) {
                if(currentPaintIndex >= 0) {
                    if(p.inventory[currentPaintIndex].modItem is CustomPaint) {
                        //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).paintColor = -1;
                        //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).customPaint = (Items.CustomPaint)p.inventory[currentPaintIndex].modItem.Clone();
                    }
                    for(int i = 0; i < PaintItemID.list.Length; i++) {
                        if(p.inventory[currentPaintIndex].type == PaintItemID.list[i]) {
                            //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).paintColor = (byte)i;
                            //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).customPaint = null;
                            break;
                        }
                    }
                }
            } else {
                //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).customPaint = null;
                //target.GetGlobalNPC<NPCs.WoMDNPC>(mod).paintColor = -1;
            }*/
		}
		#endregion

		/// <summary>
		/// Creates drops of paint
		/// </summary>
		/// <param name="count"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="spreadAngle"></param>
		/// <param name="speed"></param>
		/// <param name="timeLeft"></param>
		public void createDrops(int count, Vector2 position, Vector2 direction, float spreadAngle, float speed = 1f, int timeLeft = 30) {
			direction.Normalize();
			for(int i = 0; i < count; i++) {
				Vector2 vel = direction.RotatedBy((spreadAngle * Main.rand.NextFloat()) - (spreadAngle / 2));
				Projectile p = getProjectile(Projectile.NewProjectile(position + vel * 2f, vel * speed, ProjectileType<PaintSplatter>(), 0, 0, projectile.owner, 1, ProjectileID.IchorSplash));
				if(p != null) {
					((PaintingProjectile)p.modProjectile).light = light;
					p.timeLeft = timeLeft;
					p.alpha = 125;
				}
			}
		}

		public override bool PreKill(int timeLeft) {
			if(server() || singlePlayer() || (multiplayer() && projectile.owner == Main.myPlayer)) {
				if(dropsOnDeath)
					createDrops(dropCount, projectile.Center, projectile.oldVelocity * -1, dropCone, dropVelocity);
				if(explodesOnDeath) {
					PaintData d = getPaintData();
					if(d != null)
						explode(projectile.Center, explosionRadius, d);
				}
			}
			return base.PreKill(timeLeft);
		}

		#region ai
		public override bool PreAI() {
			_paintData = null;
			oldRotation = projectile.rotation;
			if(startPosition.X == 0 && startPosition.Y == 0)
				startPosition = projectile.position;
			return true;
		}

		public override void AI() {
			base.AI();
		}

		public override void PostAI() {
			base.PostAI();
			if(projectile.light != 0) {
				light = projectile.light;
				projectile.light = 0;
			}
			if(manualRotation)
				projectile.rotation = rotation;
			if(trailLength > 1) {
				for(int i = trailLength - 1; i > 0; i--)
					projectile.oldRot[i] = projectile.oldRot[i - 1];
			}
			if(trailLength > 0) {
				projectile.oldRot[0] = projectile.rotation;
			}
			PaintMethods method = getPaintMethod();
			if(xFrameCount > 1 && (animationFrameDuration == 0 || projectile.timeLeft % animationFrameDuration == 0))
				nextFrame(method);
			if(!usesShader) {
				updateColorFrame(method);
			}
			createLight();
		}
		#endregion

		#region rendering
		/// <summary>
		/// Updates the colorFrame property
		/// </summary>
		public void updateColorFrame(PaintMethods method) {
			if(npcOwner == -1) {
				WoMDPlayer player = getModPlayer();
				if(player != null) {
					if(player.paintData.paintColor == -1 && player.paintData.customPaint == null)
						colorFrame = 0;
					else if(player.paintData.customPaint == null)
						colorFrame = (byte)player.paintData.paintColor;
					else
						colorFrame = player.paintData.customPaint.getPaintID(player.paintData);
				}
			} else {
				NPC npc = getNPC(npcOwner);
				if(npc == null)
					return;
				WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
				if(gNpc == null)
					return;
				PaintData data = gNpc.paintData;
				if(data == null) {
					colorFrame = 0;
				} else {
					if(data.paintColor == -1 && data.customPaint == null)
						colorFrame = 0;
					else if(data.customPaint == null)
						colorFrame = (byte)data.paintColor;
					else
						colorFrame = data.customPaint.getPaintID(data);
				}
			}
			updateFrame(method);
		}

		/// <summary>
		/// Advances the animation frame, wrapping back to frame 0 if necessary
		/// </summary>
		public void nextFrame(PaintMethods method) {
			if(xFrameCount == 1)
				return;
			animationFrame++;
			if(animationFrame >= xFrameCount)
				animationFrame = 0;
			updateFrame(method);
		}

		/// <summary>
		/// Updates the frame property based on the current animationFrame and colorFrame
		/// </summary>
		/// <param name="method"></param>
		public void updateFrame(PaintMethods method) {
			if(usesShader)
				frame = animationFrame;
			else
				frame = convertColorFrame(method) * xFrameCount + animationFrame;
		}

		/// <summary>
		/// Can be used to convert the current colorFrame into a different index. This is useful for projectiles that use the GSShader.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		protected virtual int convertColorFrame(PaintMethods method) {
			switch(yFrameCount) {
				case 2:
					if(method == PaintMethods.RemovePaint || colorFrame == 0)
						return 0;
					return 1;
				case 3:
					if(method == PaintMethods.RemovePaint)
						return 1;
					if(colorFrame == 0)
						return 0;
					return 2;
			}
			if(method == PaintMethods.RemovePaint)
				return 0;
			return colorFrame;
		}

		/// <summary>
		/// Gets a source rectangle for the texture provided
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public Rectangle getSourceRectangle(Texture2D texture) {
			int frameHeight = (texture.Height - (2 * (yFrameCount - 1))) / yFrameCount;
			int yFrame = (int)Math.Floor((float)frame / xFrameCount);
			if(yFrame > yFrameCount)
				yFrame = yFrameCount - 1;
			int startY = yFrame * (frameHeight + 2);
			int frameWidth = (texture.Width - (2 * (xFrameCount - 1))) / xFrameCount;
			int xFrame = frame % xFrameCount;
			int startX = xFrame * (frameWidth + 2);
			return new Rectangle(startX, startY, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if(server())
				return false;
			//TODO: make this function a little more organized
			if(!hasGraphics)
				return false;
			if(!usesShader) {
				Texture2D texture = Main.projectileTexture[projectile.type];
				Rectangle sourceRectangle = getSourceRectangle(texture);
				Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
				float scale = projectile.scale;

				MiscShaderData shader = null;
				if(usesGSShader) {
					shader = getShader(this, getPaintData());
					if(shader != null) {
						spriteBatch.End();
						//using PointClamp instead of LinearClamp here because it messes with the chroma keying of the shader.
						spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

						resetBatchInPost = true;
					}
				}

				for(int i = trailLength; i >= 0; i--) {
					if(i == 1)
						continue;

					Vector2 projectilePos = (i == 0 ? projectile.Center : projectile.oldPos[i - 1] + (new Vector2(projectile.width / 2f, projectile.height / 2f) * projectile.scale));
					Vector2 drawPos = projectilePos - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);

					float rotation = (i == 0 ? projectile.rotation : projectile.oldRot[i - 1]);

					float opacity = projectile.Opacity - (projectile.Opacity / (trailLength + 1)) * i;
					float lightness = 1f - (.75f / (trailLength + 1)) * i;

					if(shader != null) {
						shader.UseOpacity(opacity).Apply();
					}

					spriteBatch.Draw(texture, drawPos, sourceRectangle, Color.Multiply(lightColor, lightness), rotation, origin, scale, SpriteEffects.None, 0f);

					if(shader != null && i > 0) {
						spriteBatch.End();
						//using PointClamp instead of LinearClamp here because it messes with the chroma keying of the shader.
						spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
					}
				}
			} else {
				MiscShaderData data = getShader(this, getPaintData());
				if(data != null) {
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

					resetBatchInPost = true;
				}

				Texture2D texture = Main.projectileTexture[projectile.type];
				Rectangle sourceRectangle = getSourceRectangle(texture);
				Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
				float scale = projectile.scale;

				for(int i = trailLength; i >= 0; i--) {
					if(i == 1)
						continue;

					Vector2 projectilePos = (i == 0 ? projectile.Center : projectile.oldPos[i - 1] + (new Vector2(projectile.width / 2f, projectile.height / 2f)));
					Vector2 drawPos = projectilePos - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);

					float rotation = (i == 0 ? projectile.rotation : projectile.oldRot[i - 1]);

					float op = projectile.Opacity - (projectile.Opacity / (trailLength + 1)) * i;
					float lightness = 1f - (.75f / (trailLength + 1)) * i;

					if(data != null)
						data.UseOpacity(op).Apply();

					spriteBatch.Draw(texture, drawPos, sourceRectangle, Color.Multiply(lightColor, lightness), rotation, origin, scale, SpriteEffects.None, 0f);

					if(i > 0 && data != null) {
						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
					}
				}
			}
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
		}
		#endregion

		/// <summary>
		/// Paints tiles along the old velocity of the projectile. Must be used for projectiles controlled by the player to ensure that paint is properly consumed, and colors are properly changed if the player runs out of a color mid operation
		/// </summary>
		/// <param name="oldVelocity">The old velocity of the projectile</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>The number of tiles that were updated</returns>
		public void paintAlongOldVelocity(Vector2 oldVelocity, PaintData paintData) {
			paintBetweenPoints(projectile.Center - oldVelocity, projectile.Center, paintData);
		}

		#region lights
		/// <summary>
		/// Creates a light with the projectile's color. Uses the center of the projectile for position and projectile.light for brightness
		/// </summary>
		public Color createLight() {
			return createLight(projectile.Center, light);
		}

		/// <summary>
		/// Creates a light with the projectile's color
		/// </summary>
		/// <param name="pos">The position for the light. Expects values using world coordinates</param>
		/// <param name="brightness">The brightness of the light. Expects 0 to 1f</param>
		public Color createLight(Vector2 pos, float brightness) {
			Color c = paintData.renderColor;
			Lighting.AddLight(pos, (c.R / 255f) * brightness, (c.G / 255f) * brightness, (c.B / 255f) * brightness);
			return c;
		}
		#endregion

		protected PaintData getPaintData() {
			PaintData data = new PaintData(paintCyclingTimeScale, -1, null, false, 0);
			if(npcOwner != -1) {
				NPC npc = getNPC(npcOwner);
				if(npc == null)
					return data;
				WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
				return gNpc.paintData;
			}
			WoMDPlayer player = getModPlayer();
			if(player == null)
				return data;
			return player.paintData;
		}
	}
}