using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintDynamite : PaintingItem {
        public PaintDynamite() : base() {
            usesGSShader = true;
            textureCount = 2;
            paintConsumptionChance = .5f;
		}

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Dynamite");
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
    }
}
