using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class SplatterStaff : PaintingItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Splatter Staff");
			Item.staff[ItemType<SplatterStaff>()] = true;
		}

		public override int TextureCount => 1;
		public override bool UsesGSShader => false;

		public override void SetDefaults() {
			base.SetDefaults();
			Item.CloneDefaults(ItemID.DiamondStaff);
			Item.shoot = ProjectileType<Projectiles.SplatterStaff>();
			Item.rare = ItemRarityID.Green;
			Item.damage = 10;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.reuseDelay = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.UseSound = SoundID.Item21;
			Item.shootSpeed = 9f;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<PaintStaff>(), 1);
			recipe.AddIngredient(ItemType<PaintBomb>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}