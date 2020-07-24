using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;

namespace WeaponsOfMassDecoration.Items {
	public class EndlessSplatterBulletPouch : SplatterBullet {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Endless Splatter Bullet Pouch");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.consumable = false;
			item.maxStack = 1;
			item.uniqueStack = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<SplatterBullet>(), 999 * 4);
			recipe.AddTile(TileID.CrystalBall);
			recipe.SetResult(this,1);
			recipe.AddRecipe();
		}
	}
}
