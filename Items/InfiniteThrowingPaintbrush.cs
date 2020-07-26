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
	class InfiniteThrowingPaintbrush : ThrowingPaintbrush {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Infinite Throwing Paintbrush");
			base.SetStaticDefaults(halfDamageText);
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.consumable = false;
			item.maxStack = 1;
			item.uniqueStack = true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ThrowingPaintbrush>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
