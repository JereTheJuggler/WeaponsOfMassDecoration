using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	public class EndlessPaintQuiver : PaintArrow {

		public override int TextureCount => 1;
		public override bool UsesGSShader => false;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Endless Paint Quiver");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 1;
			Item.uniqueStack = true;
			Item.consumable = false;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddIngredient(ItemType<PaintArrow>(), 999 * 4);
			recipe.AddTile(TileID.CrystalBall);
			recipe.Register();
		}
	}
}