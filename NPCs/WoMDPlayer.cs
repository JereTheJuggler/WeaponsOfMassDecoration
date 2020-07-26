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

namespace WeaponsOfMassDecoration.NPCs {
    public class WoMDPlayer : ModPlayer{
        public int currentPaintIndex;
        public int currentPaintToolIndex;

        public bool buffPainted;
        public int buffPaintedTime;
        public int buffPaintedColor;
        public CustomPaint buffPaintedCustomPaint;

        public bool accPalette = false;

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
    }
}
