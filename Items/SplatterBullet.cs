using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	public class SplatterBullet : PaintingItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Splatter Bullet");
		}

		public override void SetDefaults() {
			Item.damage = 16;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.knockBack = 5f;
			Item.value = 10;
			Item.rare = ItemRarityID.Green;
			Item.shoot = ProjectileType<Projectiles.SplatterBullet>();
			Item.shootSpeed = 14f;
			Item.ammo = AmmoID.Bullet;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(50);
			recipe.AddIngredient(ItemType<Paintball>(), 50);
			recipe.AddIngredient(ItemID.Grenade, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}