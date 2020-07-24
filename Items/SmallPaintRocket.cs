using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Items {
    public abstract class SmallPaintRocket : PaintingItem {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Rocket Mk I");
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            item.damage = 10;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;             //You need to set the item consumable so that the ammo would automatically consumed
            item.knockBack = 1.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ModContent.ProjectileType<Projectiles.SmallPaintRocket>();   //The projectile shoot when your weapon using this ammo
            item.shootSpeed = 3f;                  //The speed of the projectile
            item.ammo = AmmoID.Rocket;              //The ammo class this ammo belongs to.
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.RocketI, 9);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }
}
