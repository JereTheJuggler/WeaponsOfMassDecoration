using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using static Terraria.ModLoader.ModContent;

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
		    recipe.AddIngredient(ItemType<PaintSolution>(), 999);
		    recipe.AddTile(TileID.DyeVat);
		    recipe.SetResult(this, 1);
		    recipe.AddRecipe();
		}

		protected override Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
			if((paintColor == -1 && customPaint == null) || method == PaintMethods.RemovePaint)
				return null;
			return getExtraTexture("InfinitePaintSolutionPainted");
		}
	}
}
