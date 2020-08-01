using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class PaintSolution : PaintingItem{
        public PaintSolution() : base(){
            usesGSShader = true;
            textureCount = 2;
		}

        public override void SetStaticDefaults() {
            SetStaticDefaults("Used by the Clentaminator", "", false);
            DisplayName.SetDefault("Paint Solution");
        }

        public override void SetDefaults() {
            item.shoot = ProjectileType<Projectiles.PaintSolution>() - ProjectileID.PureSpray;
            item.ammo = AmmoID.Solution;
            item.width = 10;
            item.height = 12;
            item.value = Item.buyPrice(0, 0, 25, 0);
            item.rare = ItemRarityID.Orange;
            item.maxStack = 999;
            item.consumable = true;
        }
    }
}
