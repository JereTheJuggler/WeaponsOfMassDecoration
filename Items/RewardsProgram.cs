using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	class RewardsProgram : ModItem{

		public const float costMultiplier = .8f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rewards Program");
			Tooltip.SetDefault(string.Join("\n", new string[]{
				"20% off on paint related items!"
			}));
		}

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.accessory = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.rare = ItemRarityID.Green;
			Item.width = 38;
			Item.height = 24;
			Item.value = Item.buyPrice(1);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			player.GetModPlayer<WoMDPlayer>().accRewardsProgram = true;
		}

		public static void ModifyPrice(ref Item item) {
			if (ShouldModifyPrice(item))
				item.value = (int)Math.Ceiling((float)item.value * .8f);
		}

		protected static bool ShouldModifyPrice(Item item) {
			if (IsPaint(item) || IsPaintingTool(item) || IsPaintingItem(item))
				return true;
			ModItem i = item.ModItem;
			if (i != null && i is IRewardsProgramItem)
				return true;
			return false;
		}
	}

	interface IRewardsProgramItem { }
}
