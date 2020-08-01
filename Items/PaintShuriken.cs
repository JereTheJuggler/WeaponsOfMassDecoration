using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintShuriken : PaintingItem {
        public PaintShuriken() : base() {
            usesGSShader = true;
            textureCount = 2;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Shuriken");
		}

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Shuriken);
            item.value = Item.sellPrice(silver: 5);
            item.shoot = ProjectileType<Projectiles.PaintShuriken>();
        }
	}
}
