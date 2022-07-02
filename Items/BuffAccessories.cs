using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ReLogic.Content;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Projectiles;

namespace WeaponsOfMassDecoration.Items {
	public abstract class BuffAccessory : ModItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.accessory = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes() {
			//Recipe recipe = CreateRecipe();
			
		}

		protected abstract void AddSpecialIngredients(ref Recipe recipe);
	}

	public class BuffAccRed : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffRed = true;
		}

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.RedPaint, 10);
			recipe.AddIngredient(ItemID.Hellstone, 15);
		}
	}
	public class BuffAccYellow : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffYellow = true;
		}
		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.YellowPaint, 10);
			recipe.AddIngredient(ItemID.Ichor, 15);
		}
	}
	public class BuffAccLime : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffLime = true;
		}
		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.LimePaint, 10);
			recipe.AddIngredient(ItemID.CursedFlame, 15);
		}
	}
	public class BuffAccGreen : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffGreen = true;
		}

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.GreenPaint, 10);
		}
	}
	public class BuffAccCyan : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffCyan = true;
		}

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.CyanPaint, 10);
			recipe.AddIngredient(ItemID.FrostCore, 1);
		}
	}
	public class BuffAccPurple : BuffAccessory {
		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.accBuffPurple = true;
		}
		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.PurplePaint, 10);
			recipe.AddIngredient(ItemID.SpiderFang, 15);
		}
	}
}
