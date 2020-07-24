using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaponsOfMassDecoration.Projectiles;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items{
	public class SplatterBullet : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Splatter Bullet");
            base.SetStaticDefaults(halfDamageText);
        }

        public override void SetDefaults() {
            item.damage = 16;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;             //You need to set the item consumable so that the ammo would automatically consumed
            item.knockBack = 5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<Projectiles.SplatterBullet>();   //The projectile shoot when your weapon using this ammo
            item.shootSpeed = 14f;                  //The speed of the projectile
            item.ammo = AmmoID.Bullet;              //The ammo class this ammo belongs to.
        }

		public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<Paintball>(), 50);
            recipe.AddIngredient(ItemID.Grenade, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this,50);
            recipe.AddRecipe();
        }
    }
}
