using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Items {
    class PaintBomb : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Bomb");
            // TODO: Insert halfDamageText in base call after making it actually damage enemies
            base.SetStaticDefaults();
		}

        public override void SetDefaults() {
            item.useStyle = ItemUseStyleID.SwingThrow;
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
            item.rare = ItemRarityID.Green;
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
