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
			item.CloneDefaults(ItemID.Shuriken);
			item.value = Item.sellPrice(silver: 5);
			item.shoot = ProjectileType<Projectiles.PaintShuriken>();
		}
	}
}