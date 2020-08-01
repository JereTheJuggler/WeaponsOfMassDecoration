using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;

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
			item.consumable = false;
			item.maxStack = 1;
			item.uniqueStack = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<Paintball>(), 999 * 4);
			recipe.AddTile(TileID.CrystalBall);
			recipe.SetResult(this,1);
			recipe.AddRecipe();
		}
	}
}
