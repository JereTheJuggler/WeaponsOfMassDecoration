using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WeaponsOfMassDecoration.NPCs;

namespace WeaponsOfMassDecoration.Items {
    class HackAndSplash : PaintingItem {

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hack 'n Splash");
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            item.damage = 20;
            item.melee = true;
            item.width = 40;
            item.height = 48;
            item.useTime = 20;
            item.useAnimation = 20;
            item.useStyle = 1;
            item.knockBack = 5;
            item.value = 10000;
            item.rare = 2;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.shootSpeed = 15f;
            item.shoot = ModContent.ProjectileType<Projectiles.HackAndSplashBlob>();
        }

        public override void AddRecipes() {
            RecipeGroup basePaintGroup = new RecipeGroup(new Func<string>(delegate () {
                return "Any Base Paint";
            }), new int[]{
                ItemID.RedPaint,
                ItemID.OrangePaint,
                ItemID.YellowPaint,
                ItemID.LimePaint,
                ItemID.GreenPaint,
                ItemID.TealPaint,
                ItemID.CyanPaint,
                ItemID.BluePaint,
                ItemID.PurplePaint,
                ItemID.VioletPaint,
                ItemID.PinkPaint
            });
            RecipeGroup.RegisterGroup("basePaints", basePaintGroup);

            RecipeGroup deepPaintGroup = new RecipeGroup(new Func<string>(delegate () {
                return "Any Deep Paint";
            }), new int[]{
                ItemID.DeepRedPaint,
                ItemID.DeepOrangePaint,
                ItemID.DeepYellowPaint,
                ItemID.DeepLimePaint,
                ItemID.DeepGreenPaint,
                ItemID.DeepTealPaint,
                ItemID.DeepCyanPaint,
                ItemID.DeepBluePaint,
                ItemID.DeepPurplePaint,
                ItemID.DeepVioletPaint,
                ItemID.DeepPinkPaint
            });
            RecipeGroup.RegisterGroup("deepPaints", deepPaintGroup);

            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.GoldBroadsword, 1);
            recipe.AddRecipeGroup("basePaints", 10);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();

            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.PlatinumBroadsword, 1);
            recipe2.AddRecipeGroup("basePaints", 10);
            recipe2.AddTile(TileID.DyeVat);
            recipe2.SetResult(this, 1);
            recipe2.AddRecipe();
        }

        public override bool UseItem(Player player) {
            float xVel = 20f;
            float yVel = 20f;
            int type = ModContent.ProjectileType<Projectiles.HackAndSplashBlob>();
            Shoot(player, ref player.position, ref xVel, ref yVel, ref type, ref item.damage, ref item.knockBack);
            return base.UseItem(player);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			base.OnHitNPC(player, target, damage, knockBack, crit);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox) {
            base.MeleeEffects(player, hitbox);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            position.Y -= 32;

			item.shootSpeed = 15;

			Vector2 speed = Main.MouseWorld - position;
			speed.Normalize();
			speed *= item.shootSpeed;// * Main.rand.NextFloat(.9f, 1.1f);
			
			speedX = speed.X;
			speedY = speed.Y;
			
			//Main.PlaySound(SoundID.DD2_OgreSpit, position);

			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

    }
}
