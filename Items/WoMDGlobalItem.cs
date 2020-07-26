using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class WoMDGlobalItem : GlobalItem{
        public override bool ConsumeAmmo(Item item, Player player) {
            if(new int[] { ItemID.Paintbrush,ItemID.SpectrePaintbrush,ItemID.PaintRoller,ItemID.SpectrePaintRoller}.Contains(item.type)) {
                for(int i = 0; i < player.armor.Length / 2; i++) {
                    if(player.armor[i].type == ItemType<ArtistPalette>())
                        return false;
                }
            }
            return true;
        }

		public override bool ConsumeItem(Item item, Player player) {
            if(PaintIDs.itemIds.Contains(item.type)) {
                for(int i = 0; i < player.armor.Length / 2; i++) {
                    if(player.armor[i].type == ItemType<ArtistPalette>())
                        return false;
                }
            }
            return base.ConsumeItem(item, player);
        }
    }
}
