using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;

namespace WeaponsOfMassDecoration.Items{
    public class Paintball : PaintingItem{
		
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Paintball");
			base.SetStaticDefaults(halfDamageText);
		}

		public override void SetDefaults(){
            item.damage = 7;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;             //You need to set the item consumable so that the ammo would automatically consumed
            item.knockBack = 1.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ModContent.ProjectileType<Projectiles.Paintball>();   //The projectile shoot when your weapon using this ammo
            item.shootSpeed = 14f;                  //The speed of the projectile
            item.ammo = AmmoID.Bullet;              //The ammo class this ammo belongs to.
        }

  //      public override void AddRecipes(){
		//	ModRecipe recipe = new ModRecipe(mod);
		//	recipe.AddIngredient(ItemID.MusketBall, 9);
		//	recipe.AddTile(TileID.DyeVat);
		//	recipe.SetResult(this,9);
		//	recipe.AddRecipe();
		//}
	}
}
