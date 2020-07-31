using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintDynamite : PaintingItem {
        public PaintDynamite() {
            usesGSShader = true;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Dynamite");
            SetStaticDefaults("","50% chance to not consume paint for each block/wall covered");
        }

        public override void SetDefaults() {
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.shootSpeed = 4.5f;
            item.shoot = ProjectileType<Projectiles.PaintDynamite>();
            item.width = 8;
            item.height = 8;
            item.maxStack = 99;
            item.consumable = true;
            item.UseSound = SoundID.Item1;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useTime = 35;
            item.useAnimation = 35;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Green;
            item.autoReuse = false;
        }

        protected override Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
            if((paintColor == -1 && customPaint == null) || method == PaintMethods.RemovePaint)
                return null;
            return getExtraTexture("PaintDynamitePainted");
        }
    }
}
