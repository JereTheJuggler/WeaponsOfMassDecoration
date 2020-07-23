using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Items {
    class PaintBomb : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Bomb");
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            item.useStyle = 1;
            item.shootSpeed = 4.5f;
            item.shoot = ModContent.ProjectileType<Projectiles.PaintBomb>();
            item.width = 18;
            item.height = 18;
            item.maxStack = 99;
            item.consumable = true;
            item.UseSound = SoundID.Item1;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useTime = 25;
            item.useAnimation = 25;
            item.value = Item.buyPrice(0, 0, 50, 0);
            item.rare = 2;
            item.autoReuse = false;
        }

        //public override void AddRecipes() {
        //    ModRecipe recipe = new ModRecipe(mod);
        //    recipe.AddIngredient(ItemID.Bomb, 9);
        //    recipe.AddTile(TileID.DyeVat);
        //    recipe.SetResult(this, 9);
        //    recipe.AddRecipe();
        //}
    }
}
