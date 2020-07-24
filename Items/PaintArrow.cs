using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Items {
    public class PaintArrow : PaintingItem {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Arrow");
			base.SetStaticDefaults(halfDamageText);
		}
        public override void SetDefaults() {
            item.damage = 7;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;             //You need to set the item consumable so that the ammo would automatically consumed
            item.knockBack = 1.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ModContent.ProjectileType<Projectiles.PaintArrow>();   //The projectile shoot when your weapon using this ammo
            item.shootSpeed = 1f;                  //The speed of the projectile
            item.ammo = AmmoID.Arrow;              //The ammo class this ammo belongs to.
        }

        //public override void AddRecipes() {
        //    ModRecipe recipe = new ModRecipe(mod);
        //    recipe.AddIngredient(ItemID.WoodenArrow, 9);
        //    recipe.AddTile(TileID.DyeVat);
        //    recipe.SetResult(this, 9);
        //    recipe.AddRecipe();
        //}
    }
}
