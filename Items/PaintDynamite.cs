using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
	class PaintDynamite : PaintingItem {
		public PaintDynamite() : base() {
			usesGSShader = true;
			textureCount = 2;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Dynamite");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shootSpeed = 4.5f;
			Item.shoot = ProjectileType<Projectiles.PaintDynamite>();
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.UseSound = SoundID.Item1;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.value = Item.buyPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = false;
		}
	}
}