using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintStaff : PaintingItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Staff");
			Item.staff[ItemType<PaintStaff>()] = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.CloneDefaults(ItemID.DiamondStaff);
			Item.shoot = ProjectileType<Projectiles.PaintStaff>();
			Item.rare = ItemRarityID.Green;
			Item.damage = 20;
			Item.width = 42;
			Item.height = 30;
			Item.useAmmo = -1;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.UseSound = SoundID.Item21;
			Item.shootSpeed = 12f;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.AmethystStaff);
			recipe.AddIngredient(ItemID.Paintbrush);
			recipe.AddIngredient(ItemID.PaintRoller);
			recipe.AddIngredient(ItemID.PaintScraper);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.TopazStaff);
			recipe2.AddIngredient(ItemID.Paintbrush);
			recipe2.AddIngredient(ItemID.PaintRoller);
			recipe2.AddIngredient(ItemID.PaintScraper);
			recipe2.AddTile(TileID.DyeVat);
			recipe2.Register();
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */ {
			return base.UseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SetDefaults();
			return base.Shoot(player, null, position, velocity, type, damage, knockback);
			//return true;
		}

	}
}