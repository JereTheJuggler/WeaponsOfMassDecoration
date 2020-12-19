using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	class InfinitePaintSolution : PaintSolution {

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Infinite Paint Solution");
		}

		public override void SetDefaults() {
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

		protected override Texture2D getTexture(WoMDPlayer player) {
			if((player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null) || player.paintData.paintMethod == PaintMethods.RemovePaint)
				return null;
			return getExtraTexture("InfinitePaintSolutionPainted");
		}
	}
}