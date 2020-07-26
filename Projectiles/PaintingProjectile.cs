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

namespace WeaponsOfMassDecoration.Projectiles {
    public abstract class PaintingProjectile : ModProjectile {

		protected bool explodesOnDeath = false;
		protected float explosionRadius = 32f;

		protected bool dropsOnDeath = false;
		protected int dropCount = 5;
		protected float dropCone = (float)(Math.PI / 2f);
        protected float dropVelocity = 5f;

        protected int trailLength = 0;
        protected int trailMode = 0;

        protected Vector2 drawOriginOffset = new Vector2(0,0);

		protected bool usesShader = false;

        public List<Point> paintedTiles = new List<Point>();
        public double lastUpdateCheck = 0;
        public Vector2 startPosition = new Vector2(0,0);
        public float paintConsumptionChance = 1f;

        public bool manualRotation = false;
        public float rotation = 0;

        public float oldRotation = 0;

        public bool hasGraphics = true;

        protected int yFrameCount = 31;
        protected int xFrameCount = 1;

        public int animationFrameDuration = 0;

        public int animationFrame = 0;
        public int colorFrame = 0;

		protected bool resetBatchInPost = false;

		public PaintingProjectile() : base() { }

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
            Main.projFrames[projectile.type] = yFrameCount * xFrameCount;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = trailLength;
            if(trailLength > 0) {
                ProjectileID.Sets.TrailingMode[projectile.type] = trailMode;
            }
        }

		public override void SetDefaults() {
			base.SetDefaults();
		}

		#region getters / value conversion
		public Player getOwner() {
            return getPlayer(projectile.owner);
        }

        public WoMDPlayer getModPlayer() {
            Player p = getOwner();
            if(p == null)
                return null;
            return p.GetModPlayer<WoMDPlayer>();
		}

        public bool canPaint() {
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
        #endregion

        #region tile/npc interaction
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			WoMDGlobalNPC npc = target.GetGlobalNPC<WoMDGlobalNPC>();
            Player p = getOwner();
            if(npc != null && p != null && projectile.owner == Main.myPlayer) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                PaintMethods method = player.getPaintMethod();
                if(method != PaintMethods.None) {
                    if(method == PaintMethods.RemovePaint) {
                        npc.painted = false;
                    } else {
                        player.getPaintVars(out int paintColor, out CustomPaint customPaint);
                        applyPaintedToNPC(target, paintColor, customPaint);
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
                PaintMethods method = player.getPaintMethod();
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
                        //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = -1;
                        //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = (Items.CustomPaint)p.inventory[currentPaintIndex].modItem.Clone();
                    }
                    for(int i = 0; i < PaintIDs.itemIds.Length; i++) {
                        if(p.inventory[currentPaintIndex].type == PaintIDs.itemIds[i]) {
                            //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = (byte)i;
                            //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = null;
                            break;
                        }
                    }
                }
            } else {
                //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = null;
                //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = -1;
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
            if(manualRotation)
                projectile.rotation = rotation;
            if(trailLength > 1) {
                for(int i = trailLength - 1; i > 0; i--)
                    projectile.oldRot[i] = projectile.oldRot[i - 1];
            }
            if(trailLength > 0) {
                projectile.oldRot[0] = projectile.rotation;
            }
            if(xFrameCount > 1 && (animationFrameDuration == 0 || projectile.timeLeft % animationFrameDuration == 0))
                nextFrame();
        }
        #endregion

        #region rendering
        /// <summary>
        /// Updates the colorFrame property
        /// </summary>
        public void updateColorFrame() {
            WoMDPlayer player = getModPlayer();
            if(Main.myPlayer == projectile.owner && player != null) {
                player.getPaintVars(out int paintColor, out CustomPaint customPaint);
                colorFrame = getCurrentColorID(true, paintColor, customPaint, paintCyclingTimeScale);
			}
		}

        /// <summary>
        /// Advances the animation frame, wrapping back to frame 0 if necessary
        /// </summary>
        public void nextFrame() {
            if(xFrameCount == 1)
                return;
            animationFrame++;
            if(animationFrame >= xFrameCount)
                animationFrame = 0;
		}

        /// <summary>
        /// Gets a source rectangle for the texture provided
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Rectangle getSourceRectangle(Texture2D texture) {
            int frameHeight = (texture.Height - (2 * (yFrameCount - 1))) / yFrameCount;
            int yFrame = (colorFrame > yFrameCount - 1? yFrameCount - 1: colorFrame);
            int startY = yFrame * (frameHeight + 2);
            int frameWidth = (texture.Width - (2 * (xFrameCount - 1))) / xFrameCount;
            int startX = animationFrame * (frameWidth + 2);
            return new Rectangle(startX, startY, frameWidth, frameHeight);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if(!hasGraphics) {
                createLight();
                return false;
            }
            if(!usesShader) {
                updateColorFrame();

                Color projLight = createLight();

                Texture2D texture = Main.projectileTexture[projectile.type];
                Rectangle sourceRectangle = getSourceRectangle(texture);
                Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
                float scale = projectile.scale;
                for(int i = trailLength; i >= 0; i--) {
                    if(i == 1)
                        continue;

                    Vector2 projectilePos = (i == 0 ? projectile.Center : projectile.oldPos[i - 1] + (new Vector2(projectile.width / 2f, projectile.height / 2f) * projectile.scale));
                    Vector2 drawPos = projectilePos - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);

                    float rotation = (i == 0 ? projectile.rotation : projectile.oldRot[i - 1]);

                    float opacity = projectile.Opacity - (projectile.Opacity / (trailLength + 1)) * i;
                    float lightness = 1f - (.75f / (trailLength + 1)) * i;

                    Color color;
                    if(i == 0) {
                        color = new Color(clamp(lightColor.R + projLight.R,0,255), clamp(lightColor.G + projLight.G, 0, 255), clamp(lightColor.B + projLight.B, 0, 255),lightColor.A);
					} else {
                        color = lightColor;
					}
                    color.A = (byte)Math.Round(opacity * 255);
                    color = Color.Multiply(color, lightness);

                    spriteBatch.Draw(texture, drawPos, sourceRectangle, color, rotation, origin, scale, SpriteEffects.None, 0f);
                }
			} else {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

                Color projLight = createLight();

                resetBatchInPost = true;

                Texture2D texture = Main.projectileTexture[projectile.type];
                Rectangle sourceRectangle = getSourceRectangle(texture);
                Vector2 origin = (sourceRectangle.Size() / 2) + drawOriginOffset;
                float scale = projectile.scale;

                MiscShaderData data = (MiscShaderData)applyShader(this);

                for(int i = trailLength; i >= 0; i--) {
                    if(i == 1)
                        continue;

                    Vector2 projectilePos = (i == 0 ? projectile.Center : projectile.oldPos[i - 1] + (new Vector2(projectile.width / 2f, projectile.height / 2f)));
                    Vector2 drawPos = projectilePos - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);

                    float rotation = (i == 0 ? projectile.rotation : projectile.oldRot[i - 1]);

                    float opacity = projectile.Opacity - (projectile.Opacity / (trailLength + 1)) * i;
                    float lightness = 1f - (.5f / (trailLength + 1)) * i;
                    /*
                    Color color;
                    if(i == 0) {
                        color = new Color(clamp(lightColor.R + projLight.R, 0, 255), clamp(lightColor.G + projLight.G, 0, 255), clamp(lightColor.B + projLight.B, 0, 255), lightColor.A);
                    } else {
                        color = lightColor;
                    }
                    color.A = (byte)Math.Round(opacity * 255);
                    color = Color.Multiply(color, lightness);
                    */

                    if(data != null)
                        data.UseOpacity(opacity).Apply();

                    spriteBatch.Draw(texture, drawPos, sourceRectangle, new Color(lightness,lightness,lightness,1f), rotation, origin, scale, SpriteEffects.None, 0f);

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
            }
			if(!hasGraphics) {
                createLight();
			}
        }
        #endregion

        #region painting

        public void paintAlongOldVelocity(Vector2 oldVelocity, bool blocks = true, bool walls = true) {
            if(!(blocks || walls))
                return;
            if(oldVelocity.Length() > 0 && !(startPosition.X == 0 && startPosition.Y == 0) && (startPosition - projectile.position).Length() > oldVelocity.Length()) {
                Vector2 unitVector = new Vector2(oldVelocity.X, oldVelocity.Y);
                unitVector.Normalize();
                for(int o = 0; o < Math.Ceiling(oldVelocity.Length()); o += 8) {
                    paint(projectile.Center - oldVelocity + (unitVector * o), blocks, walls);
                }
            }
        }
        public void paintBetweenPoints(Vector2 start, Vector2 end, bool blocks = true, bool walls = true) {
            if(!(blocks || walls))
                return;
            Vector2 unitVector = end - start;
            float distance = unitVector.Length();
            unitVector.Normalize();
            int iterations = (int)Math.Ceiling(distance / 8f);
            for(int i = 0; i < iterations; i++) {
                paint(start + (unitVector * i * 8), blocks, walls);
            }

        }

        /// <summary>
        /// Creates a circle of paint
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="blocks"></param>
        /// <param name="walls"></param>
        public void explode(Vector2 pos,float radius,bool blocks = true,bool walls = true) {
            for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
                if(currentLevel == 0) {
                    paint(pos,blocks,walls);
                }else {
                    for(int i = 0; i <= currentLevel * 2; i++) {
                        float xOffset;
                        float yOffset;
                        if(i <= currentLevel) {
                            xOffset = currentLevel;
                            yOffset = i;
                        }else {
                            xOffset = (currentLevel*2 - i + 1);
                            yOffset = (currentLevel + 1);
                        }
                        Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
                        if(offsetVector.Length() <= radius) {
                            for(int dir = 0; dir < 4; dir++) {
                                paint(pos + offsetVector.RotatedBy(dir * (Math.PI/2)),blocks,walls);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a splatter of paint
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="spokes"></param>
        /// <param name="blocks"></param>
        /// <param name="walls"></param>
        public void splatter(Vector2 pos,float radius,int spokes = 5,bool blocks = true,bool walls = true) {
            explode(pos, 48f, blocks, walls);
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
                        paint(convertPositionToTile(newPos),blocks,walls);
                    }
                }
            }
        }

        /// <summary>
        /// Used to paint blocks and walls. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="coordinates">The position of the tile to paint. Expects values using world coordinates</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        public void paint(Vector2 coordinates, bool blocksAllowed = true, bool wallsAllowed = true) { paint(convertPositionToTile(coordinates),blocksAllowed,wallsAllowed); }

        /// <summary>
        /// Used to paint blocks and walls. Paints the tile at projectile.Center. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        public void paint(bool blocksAllowed = true, bool wallsAllowed = true) { paint(convertPositionToTile(projectile.Center),blocksAllowed,wallsAllowed); }

        /// <summary>
        /// Used to paint blocks and walls. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="coordinates">The position of the tile to paint. Expects values using tile coordinates</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        public void paint(Point coordinates, bool blocksAllowed = true, bool wallsAllowed = true) { paint(coordinates.X, coordinates.Y, blocksAllowed, wallsAllowed); }

        /// <summary>
        /// Used to paint blocks and walls. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="x">Tile x coordinate</param>
        /// <param name="y">Tile y coordinate</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        public void paint(int x,int y,bool blocksAllowed = true, bool wallsAllowed = true) {
            if(!(blocksAllowed || wallsAllowed))
                return;
            if(x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;
            if(!paintedTiles.Contains(new Point(x, y))) {
                Player p = getOwner();
                if(projectile.owner == Main.myPlayer && p != null) {
                    WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
					if(player.paint(x, y, blocksAllowed, wallsAllowed)) {
                        paintedTiles.Add(new Point(x, y));
					}
                }
            }
        }
        #endregion

		#region lights
		/// <summary>
		/// Creates a light with the projectile's color. Uses the center of the projectile for position and projectile.light for brightness
		/// </summary>
		public Color createLight() {
            return createLight(projectile.Center, projectile.light);
		}

        /// <summary>
        /// Creates a light with the projectile's color
        /// </summary>
        /// <param name="pos">The position for the light. Expects values using world coordinates</param>
        /// <param name="brightness">The brightness of the light. Expects 0 to 1f</param>
        public Color createLight(Vector2 pos,float brightness) {
			Color c = getColor(this);
            Color adjustedColor = new Color((c.R / 255f) * brightness, (c.G / 255f) * brightness, (c.B / 255f) * brightness);
            Lighting.AddLight(pos, adjustedColor.ToVector3());
            return adjustedColor;
        }
        #endregion
    }
}
