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
using WeaponsOfMassDecoration.Items;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using System.Security.AccessControl;

namespace WeaponsOfMassDecoration.NPCs {
    class WoMDGlobalNPC : GlobalNPC{
        public bool painted = false;
        public int paintColor = -1;
        public CustomPaint customPaint = null;
		public float paintedTime = 0;

        public override bool InstancePerEntity {
            get {
                return true;
            }
        }

        public override void ResetEffects(NPC npc) {
            painted = false;
        }

		public override bool CloneNewInstances {
			get {
				return false;
			}
		}

		private bool resetBatchInPost;
		
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(painted && Main.netMode != NetmodeID.Server && (paintColor != -1 || customPaint != null)) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this, npc, new DrawData(Main.npcTexture[npc.type], npc.position, npc.frame, Color.White));
			}
			return true;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
		}

		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
            switch(type) {
                case NPCID.Steampunker:
                    shop.item[nextSlot].SetDefaults(ItemType<Items.PaintSolution>());
                    nextSlot++;
                    break;
                case NPCID.Merchant:
                    if(shouldSellPaintingStuff()) {
                        shop.item[nextSlot].SetDefaults(ItemType<Items.PaintArrow>());
                        nextSlot++;

                        shop.item[nextSlot].SetDefaults(ItemType<Items.ThrowingPaintbrush>());
                        nextSlot++;

                        shop.item[nextSlot].SetDefaults(ItemType<Items.PaintShuriken>());
                        nextSlot++;
                    }
                    break;
                case NPCID.Demolitionist:
                    if(shouldSellPaintingStuff()) {
                        shop.item[nextSlot].SetDefaults(ItemType<Items.PaintBomb>());
                        nextSlot++;

                        shop.item[nextSlot].SetDefaults(ItemType<Items.PaintDynamite>());
                        nextSlot++;
                    }
                    break;
                case NPCID.ArmsDealer:
                    if(shouldSellPaintingStuff()) {
                        shop.item[nextSlot].SetDefaults(ItemType<Items.Paintball>());
                        nextSlot++;
                    }
                    break;
                case NPCID.Painter:
                    break;
            }
        }

        /// <summary>
        /// A generic test to check if an npc should sell painting items. Will return true if the painter has moved in, or if the player is currently carrying any paint related items
        /// </summary>
        /// <returns></returns>
        public bool shouldSellPaintingStuff() {
            if(NPC.AnyNPCs(NPCID.Painter))
                return true;
            Player p = getPlayer(Main.myPlayer);
            if(p == null)
                return false;
            Item[] inv = p.inventory;
            for(int c = 0; c < inv.Length; c++) {
                Item i = inv[c];
                if(i.active && i.stack > 0) {
                    if(i.modItem is PaintingItem)
                        return true;
                    switch(i.type) {
                        case ItemID.Paintbrush:
                        case ItemID.PaintRoller:
                        case ItemID.PaintScraper:
                        case ItemID.SpectrePaintbrush:
                        case ItemID.SpectrePaintRoller:
                        case ItemID.SpectrePaintScraper:
                            return true;
                    }
                    if(PaintIDs.itemIds.Contains(i.type))
                        return true;
                }
            }
            return false;
        }
    }
}
