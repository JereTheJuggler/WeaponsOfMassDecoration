using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class TemperaBouncer : PaintingItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Tempera Bouncer");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.CloneDefaults(ItemID.CursedFlames);
			Item.RebuildTooltip();
			Item.shoot = ProjectileType<Projectiles.TemperaBouncer>();
			Item.rare = ItemRarityID.Green;
			Item.damage = 60;
			Item.useAmmo = -1;
			Item.mana = 10;
			Item.noMelee = true;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.UseSound = SoundID.Item21;
			Item.shootSpeed = 15f;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SpellTome, 1);
			recipe.AddIngredient(ItemID.Paintbrush, 1);
			recipe.AddIngredient(ItemID.PaintRoller, 1);
			recipe.AddIngredient(ItemID.PaintScraper, 1);
			recipe.AddIngredient(ItemID.SoulofLight, 5);
			recipe.AddIngredient(ItemID.SoulofNight, 5);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}
}