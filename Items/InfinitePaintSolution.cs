using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
			Item.uniqueStack = true;
			Item.maxStack = 1;
			Item.consumable = false;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddIngredient(ItemType<PaintSolution>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}

		protected override Texture2D GetTexture(WoMDPlayer player) {
			if((player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null) || player.paintData.paintMethod == PaintMethods.RemovePaint)
				return null;
			return GetExtraTexture("InfinitePaintSolutionPainted");
		}
	}
}