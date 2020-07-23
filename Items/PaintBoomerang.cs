using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WeaponsOfMassDecoration.Items {
    class PaintBoomerang : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Boomerang");
            SetStaticDefaults("Stacks up to 3\n"+halfDamageText,"");
        }

        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[item.shoot] < item.stack;
        }

        public override void SetDefaults() {
            // Alter any of these values as you see fit, but you should probably keep useStyle on 1, as well as the noUseGraphic and noMelee bools
            item.CloneDefaults(ItemID.Flamarang);
            item.shootSpeed = 12f;
            item.damage = 10;
            item.maxStack = 3;
            //item.knockBack = 5f;
            //item.useStyle = 1;
            //item.useAnimation = 25;
            //item.useTime = 25;
            //item.width = 18;
            //item.height = 36;
            //item.maxStack = 1;
            //item.rare = 2;

            //item.consumable = false;
            //item.noUseGraphic = true;
            //item.noMelee = true;
            //item.autoReuse = false;
            //item.melee = true;

            item.UseSound = SoundID.Item1;
            item.value = Item.sellPrice(silver: 5);
            item.shoot = ModContent.ProjectileType<Projectiles.PaintBoomerang>();
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.EnchantedBoomerang);
            recipe.AddIngredient(ItemID.Paintbrush);
            recipe.AddIngredient(ItemID.PaintRoller);
            recipe.AddIngredient(ItemID.PaintScraper);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();

            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.IceBoomerang);
            recipe2.AddIngredient(ItemID.Paintbrush);
            recipe2.AddIngredient(ItemID.PaintRoller);
            recipe2.AddIngredient(ItemID.PaintScraper);
            recipe2.AddTile(TileID.DyeVat);
            recipe2.SetResult(this);
            recipe2.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            //SetDefaults();
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }
}
