using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Items {
    class PaintDynamite : PaintingItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Dynamite");
            SetStaticDefaults(halfDamageText,"50% chance to not consume paint for each block/wall covered");
        }

        public override void SetDefaults() {
            item.useStyle = 1;
            item.shootSpeed = 4.5f;
            item.shoot = ModContent.ProjectileType<Projectiles.PaintDynamite>();
            item.width = 8;
            item.height = 8;
            item.maxStack = 99;
            item.consumable = true;
            item.UseSound = SoundID.Item1;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useTime = 35;
            item.useAnimation = 35;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = 2;
            item.autoReuse = false;
        }

        //public override void AddRecipes() {
        //    ModRecipe recipe = new ModRecipe(mod);
        //    recipe.AddIngredient(ItemID.Dynamite, 9);
        //    recipe.AddTile(TileID.DyeVat);
        //    recipe.SetResult(this, 9);
        //    recipe.AddRecipe();
        //}
    }
}
