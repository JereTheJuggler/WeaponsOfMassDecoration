using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Items {
    class PaintSolution : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Solution");
            SetStaticDefaults("Used by the Clentaminator", "75% chance to not consume paint for each block/wall covered");
        }

        public override void SetDefaults() {
            item.shoot = ModContent.ProjectileType<Projectiles.PaintSolution>() - ProjectileID.PureSpray;
            item.ammo = AmmoID.Solution;
            item.width = 10;
            item.height = 12;
            item.value = Item.buyPrice(0, 0, 25, 0);
            item.rare = 3;
            item.maxStack = 999;
            item.consumable = true;
        }

        //public override void AddRecipes() {
        //    ModRecipe recipe = new ModRecipe(mod);
        //    recipe.AddIngredient(ItemID.GreenSolution, 9);
        //    recipe.AddTile(TileID.DyeVat);
        //    recipe.SetResult(this, 9);
        //    recipe.AddRecipe();
        //}
    }
}
