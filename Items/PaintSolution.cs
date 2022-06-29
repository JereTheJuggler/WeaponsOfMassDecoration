using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintSolution : PaintingItem {
		public PaintSolution() : base() {
			usesGSShader = true;
			textureCount = 2;
		}

		public override void SetStaticDefaults() {
			SetStaticDefaults("Used by the Clentaminator", "", false);
			DisplayName.SetDefault("Paint Solution");
		}

		public override void SetDefaults() {
			Item.shoot = ProjectileType<Projectiles.PaintSolution>() - ProjectileID.PureSpray;
			Item.ammo = AmmoID.Solution;
			Item.width = 10;
			Item.height = 12;
			Item.value = Item.buyPrice(0, 0, 25, 0);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 999;
			Item.consumable = true;
		}
	}
}