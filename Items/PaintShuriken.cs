using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class PaintShuriken : PaintingItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Shuriken");
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Shuriken);
            item.value = Item.sellPrice(silver: 5);
            item.shoot = ProjectileType<Projectiles.PaintShuriken>();
        }
    }
}
