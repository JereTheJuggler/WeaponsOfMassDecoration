using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintBomb : PaintingItem {
		public override int TextureCount => 2;
		public override bool UsesGSShader => true;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Bomb");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shootSpeed = 4.5f;
			Item.shoot = ProjectileType<Projectiles.PaintBomb>();
			Item.width = 18;
			Item.height = 18;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.UseSound = SoundID.Item1;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.value = Item.buyPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = false;
		}
	}
}