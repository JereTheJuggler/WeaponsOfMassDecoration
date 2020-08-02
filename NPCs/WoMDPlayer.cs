using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Constants;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using Terraria.DataStructures;

namespace WeaponsOfMassDecoration.NPCs {
    public class WoMDPlayer : ModPlayer{
        /// <summary>
        /// The index of the first slot containing paint in the player's inventory
        /// </summary>
        protected int _currentPaintIndex;
        /// <summary>
        /// The index of the first slot containing paint in the player's inventory
        /// </summary>
        public int currentPaintIndex { get {
            if(!_indexesSet)
                updateIndexes();
            return _currentPaintIndex;
		} }
        /// <summary>
        /// The index of the first slot containing a painting tool in the player's inventory
        /// </summary>
        protected int _currentPaintToolIndex;
        /// <summary>
        /// The index of the first slot containing a painting tool in the player's inventory
        /// </summary>
        public int currentPaintToolIndex { get {
            if(!_indexesSet)
                updateIndexes();
            return _currentPaintToolIndex;
        } }

        /// <summary>
        /// The paint method attached to the first painting tool found in the player's inventory
        /// </summary>
        protected PaintMethods _paintMethod;
        /// <summary>
        /// The paint method attached to the first painting tool found in the player's inventory
        /// </summary>
        public PaintMethods paintMethod { 
            get {
                if(!_indexesSet)
                    updateIndexes();
                return _paintMethod; 
            } 
        }

        /// <summary>
        /// This is used to keep track of whether or not the current indexes for paint and paint tools has been updated during this game tick. Allows the indexes to not be set at all for ticks where they are not needed
        /// </summary>
        protected bool _indexesSet = false;

        /// <summary>
        /// The PaintID of the first paint found in the player's inventory. If a custom paint is the first paint found, then this will be -1
        /// </summary>
        protected int _paintColor;
        /// <summary>
        /// The PaintID of the first paint found in the player's inventory. If a custom paint is the first paint found, then this will be -1
        /// </summary>
        public int paintColor { 
            get {
                if(!_indexesSet)
                    updateIndexes();
                return _paintColor; 
            } 
        }
        /// <summary>
        /// The CustomPaint instance for the first paint found in the player's inventory. If a vanilla paint is the first paint found, then this will be null
        /// </summary>
        protected CustomPaint _customPaint;
        /// <summary>
        /// The CustomPaint instance for the first paint found in the player's inventory. If a vanilla paint is the first paint found, then this will be null
        /// </summary>
        public CustomPaint customPaint { 
            get {
                if(!_indexesSet)
                    updateIndexes();
                return _customPaint == null ? null : (CustomPaint)_customPaint.Clone(); 
            } 
        }
        /// <summary>
        /// The color to use during rendering based on the first paint in the player's inventory. Combination of paintColor, customPaint, paintMethod, and interpolation between custom paint colors
        /// </summary>
        protected Color _renderColor = default;
        /// <summary>
        /// The color to use during rendering based on the first paint in the player's inventory. Combination of paintColor, customPaint, paintMethod, and interpolation between custom paint colors
        /// </summary>
        public Color renderColor { 
            get {
                if(!_indexesSet)
                    updateIndexes();
                return new Color(_renderColor.ToVector3()); 
            } 
        }

        public bool buffPainted;
        public int buffPaintedTime;
        public int buffPaintedColor;
        public CustomPaint buffPaintedCustomPaint;

        /// <summary>
        /// Whether or not the player has the Artist's Palette accessory equipped
        /// </summary>
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

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			base.Kill(damage, hitDirection, pvp, damageSource);
			if(GetInstance<WoMDConfig>().chaosModeEnabled)
                splatter(player.Center, 130f, 6, PaintID.DeepRed, null, new CustomPaintData());
		}

		public override void PreUpdate() {
            _indexesSet = false;
		}

		public override void PostUpdateBuffs() {
			base.PostUpdateBuffs();
            if(GetInstance<WoMDConfig>().chaosModeEnabled) {
                if(player.HasBuff(BuffID.UnicornMount)) {
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
                        WeaponsOfMassDecoration.paint(coords.X, coords.Y, colors[fartAnimationFrame], PaintMethods.BlocksAndWalls, true, true);
                    }
                    fartAnimationFrame++;
                    if(fartAnimationFrame >= heights.Length)
                        fartAnimationFrame = -1;
                }
            }
		}

		/// <summary>
		/// Looks through the player's inventory to reset the currentPaintIndex and currentPaintingToolIndex
		/// </summary>
		/// <param name="updatePaint">Set to false to prevent updating the currentPaintIndex</param>
		/// <param name="updateTool">Set to false to prevent updating the currentPaintingToolIndex</param>
		protected void updateIndexes(bool updatePaint = true, bool updateTool = true) {
            if(updatePaint) {
                _currentPaintIndex = -1;
                _paintColor = -1;
                _customPaint = null;
                _renderColor = PaintColors.list[0];
            }
            if(updateTool) {
                _currentPaintToolIndex = -1;
                _paintMethod = PaintMethods.None;
			}
            for(int i = 0; i < player.inventory.Length && ((updateTool && _currentPaintToolIndex < 0) || (updatePaint && _currentPaintIndex < 0)); i++) {
                Item item = player.inventory[i];
                if(item.active && item.stack > 0) {
                    if(updatePaint && i != 58 && _currentPaintIndex < 0 && isPaint(item)) {
                        _currentPaintIndex = i;
                        setPaintColors();
                    }
                    if(updateTool && i != 58 && _currentPaintToolIndex < 0 && isPaintingTool(item)) {
                        _currentPaintToolIndex = i;
                        setPaintMethod();
                    }
                }
			}
            if(updateTool && updatePaint)
                _indexesSet = true;
		}

        /// <summary>
        /// Sets _paintColor, _customPaint, and _renderColor based on the currentPaintIndex
        /// </summary>
        protected void setPaintColors() {
            _paintColor = -1;
            _customPaint = null;
            if(_currentPaintIndex >= 0) {
                Item item = player.inventory[_currentPaintIndex];
                if(item.modItem is CustomPaint) {
                    _customPaint = (CustomPaint)item.modItem;
                    _renderColor = _customPaint.getColor(new CustomPaintData(true, paintCyclingTimeScale, player));
                } else {
                    _paintColor = Array.IndexOf(PaintItemID.list, item.type);
                    _renderColor = PaintColors.list[_paintColor];
                }
            }
        }

        /// <summary>
        /// Sets _paintMethod based on the currentPaintToolIndex
        /// </summary>
        protected void setPaintMethod() {
            if(_currentPaintToolIndex < 0) {
                _paintMethod = PaintMethods.None;
                return;
            }
            Item item = player.inventory[_currentPaintToolIndex];
            if(item.active && item.stack > 0) {
                if(item.modItem is PaintingMultiTool) {
                    _paintMethod = PaintMethods.BlocksAndWalls;
                    return;
                }
                switch(item.type) {
                    case ItemID.Paintbrush:
                    case ItemID.SpectrePaintbrush:
                        _paintMethod = PaintMethods.Blocks;
                        return;
                    case ItemID.PaintRoller:
                    case ItemID.SpectrePaintRoller:
                        _paintMethod = PaintMethods.Walls;
                        return;
                    case ItemID.PaintScraper:
                    case ItemID.SpectrePaintScraper:
                        _paintMethod = PaintMethods.RemovePaint;
                        return;
                }
            }
            _paintMethod = PaintMethods.None;
        }

        /// <summary>
        /// A quick check to test if the player is currently able to paint. Useful for optimization to check before running a bunch of subsequent functions that would need to check this individually
        /// </summary>
        /// <returns></returns>
        public bool canPaint() {
            if(!_indexesSet)
                updateIndexes();
            return !(_paintMethod == PaintMethods.None || (_currentPaintIndex < 0 && _paintMethod != PaintMethods.RemovePaint));
		}

        /// <summary>
        /// Consumes paint if necessary when a player paints a block or wall
        /// </summary>
		public void consumePaint() {
            if(accPalette)
                return;
            if(!_indexesSet)
                updateIndexes();
            if(_paintMethod == PaintMethods.RemovePaint)
                return;
            if(_currentPaintIndex >= 0) {
                if(_customPaint != null && _customPaint.paintConsumptionChance < 1f && Main.rand.NextFloat() <= _customPaint.paintConsumptionChance)
                    return;
                Item item = player.inventory[_currentPaintIndex];
                if(item.stack > 0)
                    item.stack--;
                if(item.stack == 0) {
                    item.active = false;
                    updateIndexes(true,false);
				}
			}
		}

        /// <summary>
        /// Used to paint blocks and walls. Must be used to have paint properly be consumed.
        /// </summary>
        /// <param name="x">Tile x coordinate</param>
        /// <param name="y">Tile y coordinate</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the current painting method</param>
        /// <param name="dontConsumePaint"></param>
        /// <param name="useWorldgen">If true, WorldGen.PaintTile and PaintWall will be used. This will cause dust to appear when blocks or tiles are painted</param>
        public bool paint(int x, int y, bool blocksAllowed = true, bool wallsAllowed = true, bool dontConsumePaint = false, bool useWorldgen = false) {
            if(Main.netMode == NetmodeID.Server)
                return false;
            if(!(blocksAllowed || wallsAllowed))
                return false;
            if(x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return false;
            if(!_indexesSet)
                updateIndexes();

            PaintMethods method = _paintMethod;

            //extra checking for held and selected items in case the player is holding an actual painting tool.
            //is they area, paint method needs to be overridden by the held or selected item's paint method instead of using the first one found in the inventory
            Item paintTool = null;
            if(player.inventory[58] != null && player.inventory[58].active && player.inventory[58].stack > 0 && isPaintingTool(player.inventory[58])) {
                paintTool = player.inventory[58];
			}else if(player.inventory[player.selectedItem] != null && player.inventory[player.selectedItem].active && isPaintingTool(player.inventory[player.selectedItem])) {
                paintTool = player.inventory[player.selectedItem];
            }
            if(paintTool != null) {
                if(paintTool.modItem is PaintingMultiTool) {
                    method = PaintMethods.BlocksAndWalls;
				} else {
					switch(paintTool.type) {
                        case ItemID.Paintbrush:
                        case ItemID.SpectrePaintbrush:
                            method = PaintMethods.Blocks;
                            break;
                        case ItemID.PaintRoller:
                        case ItemID.SpectrePaintRoller:
                            method = PaintMethods.Walls;
                            break;
                        case ItemID.PaintScraper:
                        case ItemID.SpectrePaintScraper:
                            method = PaintMethods.RemovePaint;
                            break;
					}
				}
			}

            if(method == PaintMethods.None || (_currentPaintIndex < 0 && method != PaintMethods.RemovePaint))
                return false;
            byte targetColor;
            if(method == PaintMethods.RemovePaint) {
                targetColor = 0;
            } else {
                if(_customPaint == null) {
                    targetColor = (byte)_paintColor;
				} else {
                    targetColor = _customPaint.getPaintID(new CustomPaintData(false, paintCyclingTimeScale, 0, player));
				}
                if(method == PaintMethods.Blocks) {
                    wallsAllowed = false;
                } else if(method == PaintMethods.Walls) {
                    blocksAllowed = false;
                }
            }
            if(WeaponsOfMassDecoration.paint(x, y, targetColor, method, blocksAllowed, wallsAllowed, useWorldgen)) {
                if(!dontConsumePaint && method != PaintMethods.RemovePaint)
                    consumePaint();
                return true;
            }
            return false;
        }
    }
}