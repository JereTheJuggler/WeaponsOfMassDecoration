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
    class SplatterStaff : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Splatter Staff");
            Item.staff[ModContent.ItemType<SplatterStaff>()] = true;
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            base.SetDefaults();
            item.CloneDefaults(ItemID.DiamondStaff);
            item.shoot = ModContent.ProjectileType<Projectiles.SplatterStaff>();
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
            //item.knockBack = 4f;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item21;
            //item.autoReuse = true;
            item.shootSpeed = 9f;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType("PaintStaff"), 1);
            recipe.AddIngredient(mod.ItemType("PaintBomb"), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
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
