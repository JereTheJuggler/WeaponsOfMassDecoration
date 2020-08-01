using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class SplatterStaff : PaintingItem{
        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Splatter Staff");
            Item.staff[ItemType<SplatterStaff>()] = true;
		}

        public override void SetDefaults() {
            base.SetDefaults();
            item.CloneDefaults(ItemID.DiamondStaff);
            item.shoot = ProjectileType<Projectiles.SplatterStaff>();
            item.rare = ItemRarityID.Green;
            item.damage = 10;
            item.width = 48;
            item.height = 48;
            item.useTime = 10;
            item.useAnimation = 10;
            item.reuseDelay = 10;
            item.useAmmo = -1;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item21;
            item.shootSpeed = 9f;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<PaintStaff>(), 1);
            recipe.AddIngredient(ItemType<PaintBomb>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
