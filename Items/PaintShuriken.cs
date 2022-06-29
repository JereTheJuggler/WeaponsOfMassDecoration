using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintShuriken : PaintingItem {
		public PaintShuriken() : base() {
			usesGSShader = true;
			textureCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Shuriken");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Shuriken);
			Item.value = Item.sellPrice(silver: 5);
			Item.shoot = ProjectileType<Projectiles.PaintShuriken>();
		}
	}
}