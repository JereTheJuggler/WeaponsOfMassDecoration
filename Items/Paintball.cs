using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using static Terraria.ModLoader.ModContent;

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
            item.consumable = true;
            item.knockBack = 1.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<Projectiles.Paintball>();
            item.shootSpeed = 14f;
            item.ammo = AmmoID.Bullet;
        }
	}
}
