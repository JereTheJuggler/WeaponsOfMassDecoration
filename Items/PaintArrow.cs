﻿using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	public class PaintArrow : PaintingItem {
		public PaintArrow() : base() {
			usesGSShader = true;
			textureCount = 3;
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Arrow");
		}
		public override void SetDefaults() {
			item.damage = 7;
			item.ranged = true;
			item.width = 8;
			item.height = 8;
			item.maxStack = 999;
			item.consumable = true;
			item.knockBack = 1.5f;
			item.value = 10;
			item.rare = ItemRarityID.Green;
			item.shoot = ProjectileType<Projectiles.PaintArrow>();
			item.shootSpeed = 1f;
			item.ammo = AmmoID.Arrow;
		}
	}
}