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
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintBoomerang : PaintingItem{
        public PaintBoomerang() : base() {
            usesGSShader = true;
            textureCount = 3;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Boomerang");
            SetStaticDefaults("Stacks up to 3","");
        }

        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[item.shoot] < item.stack;
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Flamarang);
            item.shootSpeed = 12f;
            item.damage = 10;
            item.maxStack = 3;

            item.UseSound = SoundID.Item1;
            item.value = Item.sellPrice(silver: 5);
            item.shoot = ProjectileType<Projectiles.PaintBoomerang>();
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
    }
}
