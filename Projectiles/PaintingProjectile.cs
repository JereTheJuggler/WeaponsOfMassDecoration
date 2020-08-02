using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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
using System.IO;

namespace WeaponsOfMassDecoration.Projectiles {
    public abstract class PaintingProjectile : ModProjectile {
		protected bool explodesOnDeath = false;
		protected float explosionRadius = 32f;

		protected bool dropsOnDeath = false;
		protected int dropCount = 5;
		protected float dropCone = (float)(Math.PI / 2f);
        protected float dropVelocity = 5f;

        protected int trailLength = 0;

        protected Vector2 drawOriginOffset = new Vector2(0,0);

		protected bool usesShader = false;
        public bool usesGSShader = false;

        public List<Point> paintedTiles = new List<Point>();
        public double lastUpdateCheck = 0;
        public Vector2 startPosition = new Vector2(0,0);
        public float paintConsumptionChance = 1f;

        public bool manualRotation = false;
        public float rotation = 0;

        public float oldRotation = 0;

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
		public static void sendProjNPCOwnerPacket(PaintingProjectile p,int toClient = -1,int ignoreClient=-1) {
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
        public static void sendMultiProjNPCOwnerPacket(IEnumerable<PaintingProjectile> projectiles,int toClient = -1,int ignoreClient = -1) {
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
        /// Determines whether or not the projectile is currently able to paint. Returns false if the active paint method is removePaint
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
            return p.paintMethod;
        }
    #endregion

    #region tile/npc interaction
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			WoMDNPC npc = target.GetGlobalNPC<WoMDNPC>();
            Player p = getOwner();
            if(npc != null && p != null){// && projectile.owner == Main.myPlayer) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                PaintMethods method = player.paintMethod;
                if(method != PaintMethods.None) {
                    if(method == PaintMethods.RemovePaint) {
                        npc.painted = false;
                    } else {
                        applyPaintedToNPC(target, player.paintColor, player.customPaint, new CustomPaintData() { player = p });
                    }
                }
            }
			if(projectile.penetrate == 1)
				onKillOnNPC(target);
        }

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            Player p = getOwner();
            if(p != null && projectile.owner == Main.myPlayer) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                PaintMethods method = player.paintMethod;
                if(method == PaintMethods.None || method == PaintMethods.RemovePaint || player.currentPaintIndex < 0) {
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
            if(dropsOnDeath)
                createDrops(dropCount, projectile.Center, projectile.oldVelocity * -1, dropCone, dropVelocity);
            if(explodesOnDeath)
                explode(projectile.Center, explosionRadius);
            return base.PreKill(timeLeft);
        }

	#region ai
		public override bool PreAI() {
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
                    if(player.paintColor == -1 && player.customPaint == null)
                        colorFrame = 0;
                    else if(player.customPaint == null)
                        colorFrame = (byte)player.paintColor;
                    else
                        colorFrame = player.customPaint.getPaintID(new CustomPaintData(true, paintCyclingTimeScale, 0, getOwner()));
                }
			} else {
                NPC npc = getNPC(npcOwner);
                if(npc == null)
                    return;
                WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
                if(gNpc == null)
                    return;
                gNpc.getPaintVars(out int paintColor, out CustomPaint customPaint);
                if(paintColor == -1 && customPaint == null)
                    colorFrame = 0;
                else if(customPaint == null)
                    colorFrame = (byte)paintColor;
                else
                    colorFrame = customPaint.getPaintID(new CustomPaintData(true, npcCyclingTimeScale, gNpc.paintedTime));
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
            if(!hasGraphics)
                return false;
            if(!usesShader) {
                Texture2D texture = Main.projectileTexture[projectile.type];
                Rectangle sourceRectangle = getSourceRectangle(texture);
                Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
                float scale = projectile.scale;

                MiscShaderData shader = null;
                if(usesGSShader) {
                    spriteBatch.End();
                    //using PointClamp instead of LinearClamp here because it messes with the chroma keying of the shader.
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

                    resetBatchInPost = true;
                    shader = applyShader(this);
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

                    spriteBatch.Draw(texture, drawPos, sourceRectangle, Color.Multiply(lightColor,lightness) , rotation, origin, scale, SpriteEffects.None, 0f);

                    if(shader != null && i > 0) {
                        spriteBatch.End();
                        //using PointClamp instead of LinearClamp here because it messes with the chroma keying of the shader.
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
                    }
                }
			} else {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

                resetBatchInPost = true;

                Texture2D texture = Main.projectileTexture[projectile.type];
                Rectangle sourceRectangle = getSourceRectangle(texture);
                Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
                float scale = projectile.scale;

                MiscShaderData data = applyShader(this);

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

                    spriteBatch.Draw(texture, drawPos, sourceRectangle, Color.Multiply(lightColor,lightness), rotation, origin, scale, SpriteEffects.None, 0f);

                    if(i > 0) {
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

    #region painting
        /// <summary>
        /// Paints tiles along the old velocity of the projectile. Must be used for projectiles controlled by the player to ensure that paint is properly consumed, and colors are properly changed if the player runs out of a color mid operation
        /// </summary>
        /// <param name="oldVelocity">The old velocity of the projectile</param>
        /// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
        /// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>The number of tiles that were updated</returns>
        public int paintAlongOldVelocity(Vector2 oldVelocity, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
            if(!(blocksAllowed || wallsAllowed))
                return 0;
            int count = 0;
            if(oldVelocity.Length() > 0 && !(startPosition.X == 0 && startPosition.Y == 0) && (startPosition - projectile.position).Length() > oldVelocity.Length()) {
                Vector2 unitVector = new Vector2(oldVelocity.X, oldVelocity.Y);
                unitVector.Normalize();
                for(int o = 0; o < Math.Ceiling(oldVelocity.Length()); o += 8) {
                    if(paint(projectile.Center - oldVelocity + (unitVector * o), blocksAllowed, wallsAllowed, useWorldGen))
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Paints tiles between the 2 provided world coordinates. Must be used for projectiles controlled by the player to ensure that paint is properly consumed and colors are properly changed if the player runs out of a color mid operation
        /// </summary>
        /// <param name="start">The starting position of the line to paint. Expects values in world coordinates</param>
		/// <param name="end">The ending position of the line to paint. Expects values in world coordinates</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
        /// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>The number of tiles that were updated</returns>
        public int paintBetweenPoints(Vector2 start, Vector2 end, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
            if(!(blocksAllowed || wallsAllowed))
                return 0;
            int count = 0;
            Vector2 unitVector = end - start;
            float distance = unitVector.Length();
            unitVector.Normalize();
            int iterations = (int)Math.Ceiling(distance / 8f);
            for(int i = 0; i < iterations; i++) {
                if(paint(start + (unitVector * i * 8), blocksAllowed, wallsAllowed, useWorldGen))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Creates a circle of paint. Must be used for projectiles controlled by the player to ensure that paint is properly consumed and colors are properly changed if the player runs out of a color mid operation
        /// </summary>
        /// <param name="pos">The position of the center of the circle. Expects values in world coordinates</param>
		/// <param name="radius">The radiues of the circle. 16 for each tile</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
        /// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>The number of tiles that were updated</returns>
        public int explode(Vector2 pos, float radius, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
            int count = 0;
            for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
                if(currentLevel == 0) {
                    if(paint(pos, blocksAllowed, wallsAllowed, useWorldGen))
                        count++;
                } else {
                    for(int i = 0; i <= currentLevel * 2; i++) {
                        float xOffset;
                        float yOffset;
                        if(i <= currentLevel) {
                            xOffset = currentLevel;
                            yOffset = i;
                        } else {
                            xOffset = (currentLevel * 2 - i + 1);
                            yOffset = (currentLevel + 1);
                        }
                        Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
                        if(offsetVector.Length() <= radius) {
                            for(int dir = 0; dir < 4; dir++) {
                                if(paint(pos + offsetVector.RotatedBy(dir * (Math.PI / 2)), blocksAllowed, wallsAllowed, useWorldGen))
                                    count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Creates a splatter of paint
        /// </summary>
        /// <param name="pos">The position of the center of the splatter. Expects values in world coordinates</param>
        /// <param name="radius">The length of the spokes coming out of the center of the splatter. 1 per tile</param>
        /// <param name="spokes">The number of spokes to create</param>
        /// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
        /// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>The total number of tiles that were updated</returns>
        public int splatter(Vector2 pos,float radius,int spokes = 5,bool blocksAllowed = true,bool wallsAllowed = true, bool useWorldGen = false) {
            int count = explode(pos, 48f, blocksAllowed, wallsAllowed);
            float angle = Main.rand.NextFloat((float)Math.PI);
            float[] angles = new float[spokes];
            float[] radii = new float[spokes];
            for(int s = 0; s < spokes; s++) {
                angles[s] = angle;
                angle += Main.rand.NextFloat((float)Math.PI/6,(float)(Math.PI*2)/3);
                radii[s] = radius - (Main.rand.NextFloat(4) * 8);
            }
            for(int offset = 0;offset < radius; offset+=8) {
                for(int s = 0; s < spokes; s++) {
                    if(offset <= radii[s]) {
                        Point newPos = new Point(
                            (int)Math.Round(pos.X + Math.Cos(angles[s]) * offset),
                            (int)Math.Round(pos.Y + Math.Sin(angles[s]) * offset)
                        );
                        if(paint(convertPositionToTile(newPos), blocksAllowed, wallsAllowed, useWorldGen))
                            count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Used to paint blocks and walls. Must be used for projectiles controlled by the player to ensure that paint is properly consumed
        /// </summary>
        /// <param name="coordinates">The position of the tile to paint. Expects values using world coordinates</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>Returns true if the given tile was updated</returns>
        public bool paint(Vector2 coordinates, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) => paint(convertPositionToTile(coordinates),blocksAllowed,wallsAllowed, useWorldGen);

        /// <summary>
        /// Used to paint blocks and walls. Paints the tile at projectile.Center. Must be used for projectiles controlled by the player to ensure that paint is properly consumed
        /// </summary>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>Returns true if the given tile was updated</returns>
        public bool paint(bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) => paint(convertPositionToTile(projectile.Center),blocksAllowed,wallsAllowed, useWorldGen);

        /// <summary>
        /// Used to paint blocks and walls. Must be used for projectiles controlled by the player to ensure that paint is properly consumed
        /// </summary>
        /// <param name="coordinates">The position of the tile to paint. Expects values using tile coordinates</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>Returns true if the given tile was updated</returns>
        public bool paint(Point coordinates, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) => paint(coordinates.X, coordinates.Y, blocksAllowed, wallsAllowed, useWorldGen);

        /// <summary>
        /// Used to paint blocks and walls. Must be used for projectiles controlled by the player to ensure that paint is properly consumed
        /// </summary>
        /// <param name="x">Tile x coordinate. Expects values using tile coordinates</param>
        /// <param name="y">Tile y coordinate. Expects values using tile coordinates</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        /// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
        /// <returns>Returns true if the given tile was updated</returns>
        public bool paint(int x,int y,bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
            if(!(blocksAllowed || wallsAllowed))
                return false;
            if(x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return false;
            if(!paintedTiles.Contains(new Point(x, y))) {
                if(npcOwner == -1) {
                    Player p = getOwner();
                    if(projectile.owner == Main.myPlayer && p != null) {
                        WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                        if(player.paint(x, y, blocksAllowed, wallsAllowed, useWorldGen)) {
                            paintedTiles.Add(new Point(x, y));
                            return true;
                        }
                    }
				} else {
                    NPC npc = getNPC(npcOwner);
                    if(npc != null) {
                        WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
                        if(gNpc != null) {
                            gNpc.getPaintVars(out int paintColor, out CustomPaint customPaint);
                            if(paintColor == -1 && customPaint == null)
                                return false;
                            byte targetColor;
                            if(customPaint != null) {
                                targetColor = customPaint.getPaintID(new CustomPaintData(false, npcCyclingTimeScale, gNpc.paintedTime));
							} else {
                                targetColor = (byte)paintColor;
							}
                            return WeaponsOfMassDecoration.paint(x, y, targetColor, PaintMethods.BlocksAndWalls, blocksAllowed, wallsAllowed, useWorldGen);
						}
					}
				}
            }
            return false;
        }
    #endregion

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
        public Color createLight(Vector2 pos,float brightness) {
			Color c = getColor(this);
            Lighting.AddLight(pos, (c.R / 255f) * brightness, (c.G / 255f) * brightness, (c.B / 255f) * brightness);
            return c;
        }
    #endregion
    }
}