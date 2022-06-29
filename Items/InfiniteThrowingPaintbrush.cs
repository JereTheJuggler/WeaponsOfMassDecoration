using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	class InfiniteThrowingPaintbrush : ThrowingPaintbrush {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Infinite Throwing Paintbrush");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.consumable = false;
			Item.maxStack = 1;
			Item.uniqueStack = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddIngredient(ItemType<ThrowingPaintbrush>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}

		protected override Texture2D GetTexture(WoMDPlayer player) {
			//Needs to be overridden in this class, because the texture names do not follow the convention of <Class name>Painted and <Class name>Scraper
			if(player.paintData.paintMethod == PaintMethods.RemovePaint)
				return GetExtraTexture("ThrowingPaintbrushScraper");
			if(player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null)
				return null;
			return GetExtraTexture("ThrowingPaintbrushPainted");
		}
	}
}