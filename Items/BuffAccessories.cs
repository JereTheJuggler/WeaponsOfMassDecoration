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
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {

	public class BuffAccBase : ModItem, IRewardsProgramItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blank Emblem");
		}

		public override void SetDefaults() {
			Item.material = true;
			Item.noMelee = true;
			Item.damage = 0;
			Item.noUseGraphic = true;
			Item.value = Item.buyPrice(gold: 4);
		}
	}

	public abstract class BuffAccessory : ModItem {
		public virtual byte PaintColor { get; }
		public virtual string DebuffName { get; }

		protected virtual string GetTooltip() => "Inflicts enemies painted " + ColorNames.list[PaintColor] + " with " + DebuffName;

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.accessory = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.rare = ItemRarityID.Green;
			if(Tooltip != null)
				Tooltip.SetDefault(GetTooltip());	
			if(DisplayName != null)
				DisplayName.SetDefault(ColorNames.list[PaintColor] + " Emblem");
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<BuffAccBase>());
			recipe.AddIngredient(PaintItemID.list[PaintColor], 10);
			AddSpecialIngredients(ref recipe);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}

		protected abstract void AddSpecialIngredients(ref Recipe recipe);

		public override void UpdateAccessory(Player player, bool hideVisual) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			p.buffConfig.SetColorEnabled(PaintColor, true);
		}
	}

	public class BuffAccRed : BuffAccessory {
		public override byte PaintColor => PaintID.RedPaint;
		public override string DebuffName => "Fire";

		public static int DebuffID => BuffID.OnFire;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.Torch, 15);
		}
	}
	public class BuffAccYellow : BuffAccessory {
		public override byte PaintColor => PaintID.YellowPaint;
		public override string DebuffName => "Ichor";

		public static int DebuffID => BuffID.Ichor;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.Ichor, 15);
		}
	}
	public class BuffAccLime : BuffAccessory {
		public override byte PaintColor => PaintID.LimePaint;
		public override string DebuffName => "Cursed Flames";

		public static int DebuffID => BuffID.CursedInferno;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.CursedFlame, 15);
		}
	}
	public class BuffAccGreen : BuffAccessory {
		public override byte PaintColor => PaintID.GreenPaint;
		public override string DebuffName => "Poison";
		public static int DebuffID => BuffID.Poisoned; 

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.Stinger, 10);
		}
	}
	public class BuffAccCyan : BuffAccessory {
		public override byte PaintColor => PaintID.CyanPaint;
		public override string DebuffName => "Frostburn";
		public static int DebuffID => BuffID.Frostburn;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.FrostCore, 1);
		}
	}
	public class BuffAccPurple : BuffAccessory {
		public override byte PaintColor => PaintID.PurplePaint;
		public override string DebuffName => "Venom";		
		public static int DebuffID => BuffID.Venom;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.SpiderFang, 15);
			recipe.AddIngredient(ItemID.VialofVenom, 10);
		}
	}
	public class BuffAccPink : BuffAccessory{
		public override byte PaintColor => PaintID.PinkPaint;
		public override string DebuffName => "Party";
		public static int DebuffID => BuffType<Confetti>();

		protected override string GetTooltip() => "Enemies painted " + ColorNames.list[PaintColor] + " will drop confetti";

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.Confetti, 15);
		}
	}
	public class BuffAccNegative : BuffAccessory {
		public override byte PaintColor => PaintID.NegativePaint;
		public override string DebuffName => "Confusion";		
		public static int DebuffID => BuffID.Confused;

		protected override void AddSpecialIngredients(ref Recipe recipe) {
			recipe.AddIngredient(ItemID.LightShard, 1);
			recipe.AddIngredient(ItemID.DarkShard, 1);
			recipe.AddIngredient(ItemID.SoulofLight, 3);
			recipe.AddIngredient(ItemID.SoulofNight, 3);
		}
	}
}
