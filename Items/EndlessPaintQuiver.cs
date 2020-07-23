﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Items {
	public class EndlessPaintQuiver : PaintArrow {

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Endless Paint Quiver");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 16;
			item.height = 16;
			item.maxStack = 1;
			item.uniqueStack = true;
			item.consumable = false;
		}

		public override void AddRecipes() {
		    ModRecipe recipe = new ModRecipe(mod);
		    recipe.AddIngredient(ModContent.ItemType<PaintArrow>(), 999 * 4);
		    recipe.AddTile(TileID.CrystalBall);
		    recipe.SetResult(this, 1);
		    recipe.AddRecipe();
		}
	}
}
