using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class ArtistPalette : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Artist's Palette");
			Tooltip.SetDefault("Paint won't be consumed when worn");
		}

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.accessory = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<DeepRainbowPaint>(), 999);
			recipe.AddIngredient(ItemID.Pearlwood, 15);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			player.GetModPlayer<WoMDPlayer>().accPalette = true;
		}
	}
}