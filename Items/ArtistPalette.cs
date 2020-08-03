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
			item.maxStack = 1;
			item.accessory = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<DeepRainbowPaint>(), 999);
			recipe.AddIngredient(ItemID.Pearlwood, 15);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			player.GetModPlayer<WoMDPlayer>().accPalette = true;
		}
	}
}