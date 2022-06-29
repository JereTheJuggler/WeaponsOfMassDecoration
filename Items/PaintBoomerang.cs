using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintBoomerang : PaintingItem {
		public PaintBoomerang() : base() {
			usesGSShader = true;
			textureCount = 3;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paint Boomerang");
			SetStaticDefaults("Stacks up to 3", "");
		}

		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < Item.stack;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamarang);
			Item.shootSpeed = 12f;
			Item.damage = 10;
			Item.maxStack = 3;

			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(silver: 5);
			Item.shoot = ProjectileType<Projectiles.PaintBoomerang>();
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.EnchantedBoomerang);
			recipe.AddIngredient(ItemID.Paintbrush);
			recipe.AddIngredient(ItemID.PaintRoller);
			recipe.AddIngredient(ItemID.PaintScraper);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.IceBoomerang);
			recipe2.AddIngredient(ItemID.Paintbrush);
			recipe2.AddIngredient(ItemID.PaintRoller);
			recipe2.AddIngredient(ItemID.PaintScraper);
			recipe2.AddTile(TileID.DyeVat);
			recipe2.Register();
		}
	}
}