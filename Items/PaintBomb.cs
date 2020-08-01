using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintBomb : PaintingItem{
        public PaintBomb() : base() {
            usesGSShader = true;
            textureCount = 2;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Bomb");
		}

        public override void SetDefaults() {
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.shootSpeed = 4.5f;
            item.shoot = ProjectileType<Projectiles.PaintBomb>();
            item.width = 18;
            item.height = 18;
            item.maxStack = 99;
            item.consumable = true;
            item.UseSound = SoundID.Item1;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useTime = 25;
            item.useAnimation = 25;
            item.value = Item.buyPrice(0, 0, 50, 0);
            item.rare = ItemRarityID.Green;
            item.autoReuse = false;
        }
    }
}
