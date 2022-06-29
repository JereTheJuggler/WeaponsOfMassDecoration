using Terraria.ID;
using Terraria.ModLoader;
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
			Item.damage = 7;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.knockBack = 1.5f;
			Item.value = 10;
			Item.rare = ItemRarityID.Green;
			Item.shoot = ProjectileType<Projectiles.PaintArrow>();
			Item.shootSpeed = 1f;
			Item.ammo = AmmoID.Arrow;
		}
	}
}