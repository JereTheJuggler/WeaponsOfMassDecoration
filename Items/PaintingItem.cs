using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponsOfMassDecoration.Items {
    public abstract class PaintingItem : ModItem{

		public const string halfDamageText = "Damage is halved if you don't have any paint.";

		public override void SetStaticDefaults() {
            SetStaticDefaults("", "");
        }

        public virtual void SetStaticDefaults(string preToolTip,string postToolTip = "") {
            Tooltip.SetDefault((preToolTip != "" ? preToolTip + "\n" : "") +
                "Paints blocks and walls!\nBrushing, rolling, or scraping is determined by the first painting tool in your inventory" +
                (postToolTip != "" ? "\n" + postToolTip : ""));
        }

        public int numFrames = 31;

        public int color;
        public int currentPaintIndex;

        public Player getOwner() {
            if(item.owner >= 0 && item.owner < Main.player.Length) {
                return Main.player[item.owner];
            }
            return null;
        }

		public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat){
			setCurrentPaintIndex(player);
			if(currentPaintIndex < 0)
				mult *= .5f;
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			WoMDGlobalNPC npc = target.GetGlobalNPC<WoMDGlobalNPC>();
			if(npc != null) {
				setCurrentPaintIndex(player);
				if(currentPaintIndex >= 0) {
					if(player.inventory[currentPaintIndex].modItem is CustomPaint) {
						WeaponsOfMassDecoration.applyPaintedToNPC(target, -1, (CustomPaint)player.inventory[currentPaintIndex].modItem);
					} else {
						for(int i = 0; i < PaintIDs.itemIds.Length; i++) {
							if(player.inventory[currentPaintIndex].type == PaintIDs.itemIds[i]) {
								WeaponsOfMassDecoration.applyPaintedToNPC(target, (byte)i, null);
								break;
							}
						}
					}
				} else {
					npc.customPaint = null;
					npc.paintColor = -1;
					npc.paintedTime = 0;
				}
			}
		}

		public void setCurrentPaintIndex() {
            Player player = getOwner();
            setCurrentPaintIndex(player);
        }

        public void setCurrentPaintIndex(Player player) {
            color = -2;
			currentPaintIndex = -1;
            try {
                for(int i = 0; i <= player.inventory.Length && color == -2; i++) {
                    if(i == player.inventory.Length) {
                        color = -1;
						currentPaintIndex = -1;
                    } else {
                        if(player.inventory[i].type != 0 && player.inventory[i].stack > 0) {
                            if(PaintIDs.itemIds.Contains(player.inventory[i].type)) {
                                for(int c = 0; c < PaintIDs.itemIds.Length; c++) {
                                    if(PaintIDs.itemIds[c] == player.inventory[i].type) {
                                        color = c;
                                        break;
                                    }
                                }
                            } else if(player.inventory[i].modItem is CustomPaint) {
                                color = PaintIDs.Custom;
                            }
                            if(color != -2)
                                currentPaintIndex = i;
                        }
                    }
                }
            } catch {
                currentPaintIndex = -1;
            }
        }

        public bool shouldConsumePaint() {
            Player p = getOwner();
            for(int i = 0; i < p.armor.Length / 2; i++) {
                if(p.armor[i].type == ModContent.ItemType<ArtistPalette>())
                    return false;
            }
            if(p.inventory[currentPaintIndex].modItem is CustomPaint)
                return Main.rand.NextFloat(0, 1) <= ((CustomPaint)p.inventory[currentPaintIndex].modItem).consumptionChance;
            return true;
        }
    }
}
