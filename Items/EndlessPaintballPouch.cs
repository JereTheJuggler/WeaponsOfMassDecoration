﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	public class EndlessPaintballPouch : Paintball {

		public EndlessPaintballPouch() : base() {
			textureCount = 1;
			usesGSShader = false;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Endless Paintball Pouch");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.consumable = false;
			Item.maxStack = 1;
			Item.uniqueStack = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddIngredient(ItemType<Paintball>(), 999 * 4);
			recipe.AddTile(TileID.CrystalBall);
			recipe.Register();
		}
	}
}