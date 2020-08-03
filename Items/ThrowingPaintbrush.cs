using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class ThrowingPaintbrush : PaintingItem {
		public ThrowingPaintbrush() : base() {
			usesGSShader = true;
			textureCount = 3;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Throwing Paintbrush");
			SetStaticDefaults("", "This is how most modern art is created anyways, right?");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.ThrowingKnife);
			item.rare = ItemRarityID.Green;

			item.thrown = true;
			item.shoot = ProjectileType<Projectiles.ThrowingPaintbrush>();
		}
	}
}