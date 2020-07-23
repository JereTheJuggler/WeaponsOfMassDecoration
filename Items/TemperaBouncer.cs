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

namespace WeaponsOfMassDecoration.Items {
    class TemperaBouncer : PaintingItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tempera Bouncer");
            //Item.staff[mod.ItemType<PaintStaff>()] = true;
            base.SetStaticDefaults(halfDamageText);
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.CloneDefaults(ItemID.CursedFlames);
            item.shoot = ModContent.ProjectileType<Projectiles.TemperaBouncer>();
            item.rare = 2;
            item.damage = 60;
            //item.width = 42;
            //item.height = 30;
            //item.useTime = 35;
            //item.useAnimation = 35;
            item.useAmmo = -1;
            item.mana = 10;
            //item.useStyle = 5;
            item.noMelee = true;
            //item.knockBack = 4f;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item21;
            //item.autoReuse = true;
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

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            SetDefaults();
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            //return true;
        }
    }
}
