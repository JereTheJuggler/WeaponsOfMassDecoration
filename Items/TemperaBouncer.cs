using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class TemperaBouncer : PaintingItem {
        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Tempera Bouncer");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.CloneDefaults(ItemID.CursedFlames);
            item.RebuildTooltip();
            item.shoot = ProjectileType<Projectiles.TemperaBouncer>();
            item.rare = ItemRarityID.Green;
            item.damage = 60;
            item.useAmmo = -1;
            item.mana = 10;
            item.noMelee = true;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item21;
            item.shootSpeed = 15f;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SpellTome, 1);
            recipe.AddIngredient(ItemID.Paintbrush, 1);
            recipe.AddIngredient(ItemID.PaintRoller, 1);
            recipe.AddIngredient(ItemID.PaintScraper, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 5);
            recipe.AddIngredient(ItemID.SoulofNight, 5);
            recipe.AddTile(TileID.Bookcases);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
