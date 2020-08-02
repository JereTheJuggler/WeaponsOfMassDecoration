using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WeaponsOfMassDecoration.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    public class WoMDItem : GlobalItem {
        public override bool ConsumeItem(Item item, Player player) {
            if(item.paint > 0) {
                WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
                if(p != null) {
                    if(p.accPalette)
                        return false;
                }
            }
            return base.ConsumeItem(item,player);
        }

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            switch(item.type) {
                case ItemID.Paintbrush:
                case ItemID.SpectrePaintbrush:
                case ItemID.PaintRoller:
                case ItemID.SpectrePaintRoller:
                    Player p = getPlayer(item.owner);
                    if(p == null)
                        return;
                    WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                    if(player == null)
                        return;
                    tooltips.Add(new TooltipLine(player.mod, "CurrentPaintColor", "Current Color: " + getPaintColorName(player.paintColor, player.customPaint)));
                    break;
            }
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            string itemName;
            switch(item.type) {
                case ItemID.Paintbrush:
                    itemName = "Paintbrush";
                    break;
                case ItemID.SpectrePaintbrush:
                    itemName = "SpectrePaintbrush";
                    break;
                case ItemID.PaintRoller:
                    itemName = "PaintRoller";
                    break;
                case ItemID.SpectrePaintRoller:
                    itemName = "SpectrePaintRoller";
                    break;
                default:
                    return true;
            }
            Player p = getPlayer(item.owner);
            if(p == null)
                return true;
            WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
            if(player == null)
                return true;
            //if(player.paintColor == -1 && player.customPaint == null)
            //    return true;
            Texture2D texture = getTexture(itemName, player);
            if(texture == null)
                texture = Main.itemTexture[item.type];
            MiscShaderData shader = null;
            if(player.paintColor != -1 || player.customPaint != null)
                shader = getShader(item.GetGlobalItem<WoMDItem>(), player);
            //if(shader == null)
            //    return true;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, shader != null ? SamplerState.PointClamp : SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

            if(shader != null) {
                shader.Apply();
            }

            spriteBatch.Draw(texture, position, frame, drawColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

            if(shader != null) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
            return false;
        }

        protected Texture2D getTexture(string itemname, WoMDPlayer player) {
            if(player.paintColor == -1 && player.customPaint == null)
                return null;
            return getExtraTexture(itemname + "Painted");
        }

		public override bool UseItem(Item item, Player player) {
            PaintMethods method = PaintMethods.None;
            switch(item.type) {
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
            if(method != PaintMethods.None) {
                WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
                if(p == null)
                    return true;
                Point coords;
                if(Main.SmartCursorEnabled) {
                    coords = new Point(Main.SmartCursorX, Main.SmartCursorY);
                } else {
                    coords = Main.MouseWorld.ToTileCoordinates();
                    Point playerPos = player.position.ToTileCoordinates();
                    int xOffset = coords.X - playerPos.X;
                    int yOffset = coords.Y - playerPos.Y;
                    if(yOffset < 0)
                        yOffset--;
                    if(!isInRange(player, xOffset, yOffset, item))
                        return false;
                }
                p.paint(coords.X, coords.Y, method != PaintMethods.Walls, method != PaintMethods.Blocks, false, true);
                return false;
            }
            return false;
        }

        public bool isInRange(Player player, int xOffset, int yOffset, Item item) => Math.Abs(xOffset) <= player.lastTileRangeX + item.tileBoost && Math.Abs(yOffset) <= player.lastTileRangeY + item.tileBoost;
    }
}