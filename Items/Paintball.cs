using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	public class Paintball : PaintingItem {

		public Paintball() : base() {
			usesGSShader = true;
			textureCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paintball");
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
			item.shoot = ProjectileType<Projectiles.Paintball>();
			item.shootSpeed = 14f;
			item.ammo = AmmoID.Bullet;
		}
	}
}