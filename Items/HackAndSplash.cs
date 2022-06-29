using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class HackAndSplash : PaintingItem {

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Hack 'n Splash");
		}

		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 48;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.shootSpeed = 15f;
			Item.shoot = ProjectileType<Projectiles.HackAndSplashBlob>();
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddRecipeGroup("WoMD:goldSword", 1);
			recipe.AddRecipeGroup("WoMD:basePaints", 10);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();

			Recipe recipe2 = CreateRecipe(1);
			recipe.AddRecipeGroup("WoMD:goldSword", 1);
			recipe2.AddRecipeGroup("WoMD:deepPaints", 5);
			recipe2.AddTile(TileID.DyeVat);
			recipe2.Register();
		}

		public override bool? UseItem(Player player){
			Vector2 vel = new Vector2(20f, 20f);
			int type = ProjectileType<Projectiles.HackAndSplashBlob>();
			Shoot(player, null, player.position, vel, type, Item.damage, Item.knockBack);
			return base.UseItem(player);
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			base.MeleeEffects(player, hitbox);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			position.Y -= 32;

			Item.shootSpeed = 15;

			Vector2 speed = Main.MouseWorld - position;
			speed.Normalize();
			speed *= Item.shootSpeed;

			float speedX = speed.X;
			float speedY = speed.Y;

			return base.Shoot(player, source, position, new Vector2(speedX, speedY), type, damage, knockback);
		}

	}
}