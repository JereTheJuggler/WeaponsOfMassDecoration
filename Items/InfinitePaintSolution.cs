using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Items {
	class InfinitePaintSolution : PaintSolution {

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Infinite Paint Solution");
		}

		public override void SetDefaults(){
			base.SetDefaults();
			item.uniqueStack = true;
			item.maxStack = 1;
			item.consumable = false;
		}

		public override void AddRecipes() {
		    ModRecipe recipe = new ModRecipe(mod);
		    recipe.AddIngredient(ModContent.ItemType<PaintSolution>(), 999);
		    recipe.AddTile(TileID.DyeVat);
		    recipe.SetResult(this, 1);
		    recipe.AddRecipe();
		}
	}
}
