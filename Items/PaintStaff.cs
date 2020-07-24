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

namespace WeaponsOfMassDecoration.Items {
    class PaintStaff : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Staff");
            Item.staff[ModContent.ItemType<PaintStaff>()] = true;
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            base.SetDefaults();
            item.CloneDefaults(ItemID.DiamondStaff);
            item.shoot = ModContent.ProjectileType<Projectiles.PaintStaff>();
            item.rare = ItemRarityID.Green;
            item.damage = 20;
            item.width = 42;
            item.height = 30;
            //item.useTime = 35;
            //item.useAnimation = 35;
            item.useAmmo = -1;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            //item.knockBack = 4f;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item21;
            //item.autoReuse = true;
            item.shootSpeed = 12f;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.AmethystStaff);
            recipe.AddIngredient(ItemID.Paintbrush);
            recipe.AddIngredient(ItemID.PaintRoller);
            recipe.AddIngredient(ItemID.PaintScraper);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();

            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.TopazStaff);
            recipe2.AddIngredient(ItemID.Paintbrush);
            recipe2.AddIngredient(ItemID.PaintRoller);
            recipe2.AddIngredient(ItemID.PaintScraper);
            recipe2.AddTile(TileID.DyeVat);
            recipe2.SetResult(this);
            recipe2.AddRecipe();
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        public override bool UseItem(Player player) {
            return base.UseItem(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            SetDefaults();
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            //return true;
        }

    }
}
