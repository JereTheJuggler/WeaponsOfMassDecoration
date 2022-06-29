using Terraria.ID;
using Terraria.ModLoader;
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
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.rare = ItemRarityID.Green;

			Item.DamageType = DamageClass.Ranged;
			Item.shoot = ProjectileType<Projectiles.ThrowingPaintbrush>();
		}
	}
}