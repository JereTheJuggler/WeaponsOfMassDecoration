using Microsoft.Xna.Framework.Graphics;
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
			item.consumable = false;
			item.maxStack = 1;
			item.uniqueStack = true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<PaintShuriken>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
		protected override Texture2D getTexture(WoMDPlayer player) {
			//Needs to be overridden in this class, because the texture names do not follow the convention of <Class name>Painted
			if((player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null) || player.paintData.paintMethod == PaintMethods.RemovePaint)
				return null;
			return getExtraTexture("PaintShurikenPainted");
		}
	}
}