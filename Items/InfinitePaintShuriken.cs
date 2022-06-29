using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	class InfinitePaintShuriken : PaintShuriken {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Infinite Paint Shuriken");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.consumable = false;
			Item.maxStack = 1;
			Item.uniqueStack = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(1);
			recipe.AddIngredient(ItemType<PaintShuriken>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}
		protected override Texture2D GetTexture(WoMDPlayer player) {
			//Needs to be overridden in this class, because the texture names do not follow the convention of <Class name>Painted
			if((player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null) || player.paintData.paintMethod == PaintMethods.RemovePaint)
				return null;
			return GetExtraTexture("PaintShurikenPainted");
		}
	}
}