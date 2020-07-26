using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WeaponsOfMassDecoration.Items;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using System.Runtime.Remoting;

namespace WeaponsOfMassDecoration.NPCs {
    public class WoMDPlayer : ModPlayer{
        public int currentPaintIndex;
        public int currentPaintToolIndex;

        public bool buffPainted;
        public int buffPaintedTime;
        public int buffPaintedColor;
        public CustomPaint buffPaintedCustomPaint;

        public bool accPalette = false;

        public float mountUnicornTime = 0;
        public float lastFartTime = 0;
        public int fartAnimationFrame = -1;
        public Point fartPosition = new Point(0,0);
        public int fartDirection = 1;

        public override void ResetEffects() {
            buffPainted = false;
            accPalette = false;
        }

        public override void UpdateDead() {
            buffPainted = false;
        }

		public override void PreUpdate() {
            updateIndexes();
		}

		public override void PostUpdateBuffs() {
			base.PostUpdateBuffs();
			if(player.HasBuff(BuffID.UnicornMount)){
                if(mountUnicornTime == 0)
                    mountUnicornTime = Main.GlobalTime;
                if(fartAnimationFrame == -1 && Main.GlobalTime - lastFartTime > 10 && Main.GlobalTime - mountUnicornTime > 10) {
                    if(Main.rand.NextFloat() <= .3f) {
                        fartAnimationFrame = 0;
                        lastFartTime = Main.GlobalTime;
                        fartDirection = player.direction * -1;
                        fartPosition = (player.position + new Vector2(32 * fartDirection, 48)).ToTileCoordinates();
                        Main.PlaySound(SoundID.Item16, player.position);
                        for(int i = 0; i < 6; i++) {
                            Dust d = Dust.NewDustPerfect(fartPosition.ToWorldCoordinates() + new Vector2(8, 8), 22, new Vector2(fartDirection * 3f, 0).RotatedBy(Main.rand.NextFloat((float)(Math.PI / -6f), (float)(Math.PI / 6f))) * 3f, 0, default, 1f);
                            d.noGravity = true;
                            d.fadeIn = 1.5f;
                        }
                    }
                }
            } else {
                mountUnicornTime = 0;
			}
            if(fartAnimationFrame >= 0 && fartAnimationFrame <= 3 && Main.rand.NextFloat() < .5f) {
                Dust d = Dust.NewDustPerfect(fartPosition.ToWorldCoordinates() + new Vector2(8, 8), 22, new Vector2(fartDirection * 3f, 0).RotatedBy(Main.rand.NextFloat((float)(Math.PI / -6f), (float)(Math.PI / 6f))) * 3f, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            if(fartAnimationFrame >= 0 && Main.GlobalTime - lastFartTime > (.05f * fartAnimationFrame)) {
                int[] heights = new int[] { 1, 3, 3, 5, 5, 7 };
                byte[] colors = new byte[] { PaintID.DeepRed, PaintID.DeepOrange, PaintID.DeepYellow, PaintID.DeepGreen, PaintID.DeepBlue, PaintID.DeepPurple };
                int height = heights[fartAnimationFrame];
                for(int i = 0; i < height; i++) {
                    Point coords = new Point(
                        fartPosition.X + (fartAnimationFrame * fartDirection), 
                        fartPosition.Y + (((height - 1) / -2) + i)
                    );
                    paintDirect(coords.X, coords.Y, colors[fartAnimationFrame], PaintMethods.TilesAndWalls, true, true, true);
				}
                fartAnimationFrame++;
                if(fartAnimationFrame >= heights.Length)
                    fartAnimationFrame = -1;
			}
		}

		/// <summary>
		/// Looks through the player's inventory to reset the currentPaintIndex and currentPaintingToolIndex
		/// </summary>
		/// <param name="updatePaint">Set to false to prevent updating the currentPaintIndex</param>
		/// <param name="updateTool">Set to false to prevent updating the currentPaintingToolIndex</param>
		public void updateIndexes(bool updatePaint = true, bool updateTool = true) {
            if(updatePaint)
                currentPaintIndex = -1;
            if(updateTool)
                currentPaintToolIndex = -1;
            for(int i = 0; i < player.inventory.Length && ((updateTool && currentPaintToolIndex < 0) || (updatePaint && currentPaintIndex < 0)); i++) {
                Item item = player.inventory[i];
                if(item.active && item.stack > 0) {
                    if(updatePaint && currentPaintIndex < 0 && isPaint(item))
                        currentPaintIndex = i;
                    if(updateTool && currentPaintToolIndex < 0 && isPaintingTool(item))
                        currentPaintToolIndex = i;
                }
			}
		}

        /// <summary>
        /// Checks the item in the player's currentPaintIndex and sets variables for paintColor and customPaint
        /// </summary>
        /// <param name="paintColor">If no paint is found, or a custom paint is found first, this will be -1. Otherwise it will match a value from PaintID</param>
        /// <param name="customPaint">If no paint is found, or a vanilla paint is found first, this will be null. Otherwise it will be an instance of CustomPaint</param>
        public void getPaintVars(out int paintColor, out CustomPaint customPaint) {
            paintColor = -1;
            customPaint = null;
            PaintMethods method = getPaintMethod();
            if(method == PaintMethods.None || method == PaintMethods.RemovePaint) {
                paintColor = 0;
                return;
            }
            if(currentPaintIndex >= 0) {
                Item item = player.inventory[currentPaintIndex];
                if(item.modItem is CustomPaint)
                    customPaint = (CustomPaint)item.modItem;
                else
                    paintColor = Array.IndexOf(PaintIDs.itemIds, item.type);
            }
		}

        /// <summary>
        /// Checks the item in the player's currentPaintingToolIndex and returns a member from PaintMethod
        /// </summary>
        /// <returns></returns>
        public PaintMethods getPaintMethod() {
            if(currentPaintToolIndex < 0)
                return PaintMethods.None;
            Item item = player.inventory[currentPaintToolIndex];
            if(item.active && item.stack > 0) {
                if(item.modItem is PaintingMultiTool)
                    return PaintMethods.TilesAndWalls;
				switch(item.type) {
                    case ItemID.Paintbrush:
                    case ItemID.SpectrePaintbrush:
                        return PaintMethods.Tiles;
                    case ItemID.PaintRoller:
                    case ItemID.SpectrePaintRoller:
                        return PaintMethods.Walls;
                    case ItemID.PaintScraper:
                    case ItemID.SpectrePaintScraper:
                        return PaintMethods.RemovePaint;
				}
			}
            return PaintMethods.None;
		}

        /// <summary>
        /// A quick check to test if the player is currently able to paint. Useful for optimization to check before running a bunch of subsequent functions that would need to check this individually
        /// </summary>
        /// <returns></returns>
        public bool canPaint() {
            PaintMethods method = getPaintMethod();
            return !(method == PaintMethods.None || (currentPaintIndex < 0 && method != PaintMethods.RemovePaint));
		}

        /// <summary>
        /// Consumes paint if necessary when a player paints a block or wall
        /// </summary>
		public void consumePaint() {
            if(accPalette)
                return;
            if(getPaintMethod() == PaintMethods.RemovePaint)
                return;
            if(currentPaintIndex >= 0) {
                Item item = player.inventory[currentPaintIndex];
                if(item.stack > 0)
                    item.stack--;
                if(item.stack == 0) {
                    item.active = false;
                    updateIndexes(true,false);
				}
			}
		}

        /// <summary>
        /// Used to paint blocks and walls. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="x">Tile x coordinate</param>
        /// <param name="y">Tile y coordinate</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the current painting method</param>
        public bool paint(int x, int y, bool blocksAllowed = true, bool wallsAllowed = true, bool dontConsumePaint = false) {
            if(Main.netMode == NetmodeID.Server)
                return false;
            if(!(blocksAllowed || wallsAllowed))
                return false;
            if(x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return false;
            PaintMethods method = getPaintMethod();
            if(method == PaintMethods.None || (currentPaintIndex == -1 && method != PaintMethods.RemovePaint))
                return false;
            byte targetColor;
            if(method == PaintMethods.RemovePaint) {
                targetColor = 0;
            } else {
                getPaintVars(out int paintColor, out CustomPaint customPaint);
                if(customPaint == null) {
                    targetColor = (byte)paintColor;
				} else {
                    targetColor = customPaint.getPaintID(new CustomPaintData(false, paintCyclingTimeScale, 0, player));
				}
                if(method == PaintMethods.Tiles) {
                    wallsAllowed = false;
                } else if(method == PaintMethods.Walls) {
                    blocksAllowed = false;
                }
            }
            return paintDirect(x,y,targetColor,method,blocksAllowed,wallsAllowed,dontConsumePaint);
        }

        private bool paintDirect(int x, int y, byte color, PaintMethods method, bool blocksAllowed = true, bool wallsAllowed = true, bool dontConsumePaint = false) {
            if(!WorldGen.InWorld(x, y, 10))
                return false;
            Tile t = Main.tile[x, y];
            bool updated = false;
            if(blocksAllowed && t.active() && t.color() != color && (color != 0 || method == PaintMethods.RemovePaint)) {
                t.color(color);
                updated = true;
            }
            if(wallsAllowed && t.wall > 0 && t.wallColor() != color && (color != 0 || method == PaintMethods.RemovePaint)) {
                t.wallColor(color);
                updated = true;
            }
            if(updated) {
                if(!dontConsumePaint && method != PaintMethods.RemovePaint)
                    consumePaint();
                if(Main.netMode == NetmodeID.MultiplayerClient)
                    sendTileFrame(x, y);
            }
            return updated;
        }

        public static void sendTileFrame(int x, int y) {
            //WorldGen.SquareTileFrame(x, y);
            //WorldGen.SquareWallFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y, 1);
        }
    }
}
