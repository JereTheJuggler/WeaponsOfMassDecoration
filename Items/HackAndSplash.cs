using Microsoft.Xna.Framework;
using Terraria;
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
			item.damage = 20;
			item.melee = true;
			item.width = 40;
			item.height = 48;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 5;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
			item.shootSpeed = 15f;
			item.shoot = ProjectileType<Projectiles.HackAndSplashBlob>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("WoMD:goldSword", 1);
			recipe.AddRecipeGroup("WoMD:basePaints", 10);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();

			ModRecipe recipe2 = new ModRecipe(mod);
			recipe.AddRecipeGroup("WoMD:goldSword", 1);
			recipe2.AddRecipeGroup("WoMD:deepPaints", 5);
			recipe2.AddTile(TileID.DyeVat);
			recipe2.SetResult(this, 1);
			recipe2.AddRecipe();
		}

		public override bool UseItem(Player player) {
			float xVel = 20f;
			float yVel = 20f;
			int type = ProjectileType<Projectiles.HackAndSplashBlob>();
			Shoot(player, ref player.position, ref xVel, ref yVel, ref type, ref item.damage, ref item.knockBack);
			return base.UseItem(player);
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			base.MeleeEffects(player, hitbox);
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			position.Y -= 32;

			item.shootSpeed = 15;

			Vector2 speed = Main.MouseWorld - position;
			speed.Normalize();
			speed *= item.shootSpeed;

			speedX = speed.X;
			speedY = speed.Y;

			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}

	}
}