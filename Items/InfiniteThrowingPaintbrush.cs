using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	class InfiniteThrowingPaintbrush : ThrowingPaintbrush {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Infinite Throwing Paintbrush");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.consumable = false;
			item.maxStack = 1;
			item.uniqueStack = true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ThrowingPaintbrush>(), 999);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

		protected override Texture2D getTexture(WoMDPlayer player) {
			//Needs to be overridden in this class, because the texture names do not follow the convention of <Class name>Painted and <Class name>Scraper
			if(player.paintMethod == PaintMethods.RemovePaint)
				return getExtraTexture("ThrowingPaintbrushScraper");
			if(player.paintColor == -1 && player.customPaint == null)
				return null;
			return getExtraTexture("ThrowingPaintbrushPainted");
		}
	}
}
