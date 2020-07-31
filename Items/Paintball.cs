using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items{
    public class Paintball : PaintingItem{
		
        public Paintball() {
            usesGSShader = true;
		}

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
        
        protected override Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
            if((paintColor == -1 && customPaint == null) || method == PaintMethods.RemovePaint)
                return null;
            return getExtraTexture("PaintballPainted");
        }
    }
}
