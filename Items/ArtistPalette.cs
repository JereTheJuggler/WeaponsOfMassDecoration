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

namespace WeaponsOfMassDecoration.Items {
    class ArtistPalette : ModItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Artist's Palette");
            Tooltip.SetDefault("Paint won't be consumed when worn");
        }

        public override void SetDefaults() {
            item.maxStack = 1;
            item.accessory = true;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<DeepRainbowPaint>(), 999);
            recipe.AddIngredient(ItemID.Pearlwood,15);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
