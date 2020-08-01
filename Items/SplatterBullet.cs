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
            base.SetStaticDefaults();
            DisplayName.SetDefault("Splatter Bullet");
        }

        public override void SetDefaults() {
            item.damage = 16;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;
            item.knockBack = 5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<Projectiles.SplatterBullet>();
            item.shootSpeed = 14f;
            item.ammo = AmmoID.Bullet;
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
