using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	public class PaintingMultiTool : PaintingItem {
		public float rotation;
		public int hitboxExtension = 0;
		public int soundFrequency = 1;
		public int soundTimer = 0;

		public PaintingMultiTool() : base() {
			textureCount = 2;
			usesGSShader = true;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paint Multi-Tool");
			Tooltip.SetDefault(halfDamageText + "Paints blocks and walls at the same time!\nCurrent Paint: ");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.Paintbrush);
			item.holdStyle = 0;
			item.useAnimation = 30;
			item.RebuildTooltip();
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.width = 29;
			item.height = 30;
			item.maxStack = 1;
			item.noUseGraphic = false;
			item.noMelee = false;
			item.melee = true;
			item.damage = 4;
			item.knockBack = 3f;
			item.autoReuse = true;
			item.value = Item.buyPrice(0, 0, 0, 10);
			item.rare = ItemRarityID.Green;
			item.useTime = 15;
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-26, 2).RotatedBy(rotation);
		}
		public override void HoldStyle(Player player) {
			rotation = 0;
		}

		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			hitbox = new Rectangle(
				(int)Math.Round(player.itemLocation.X) - hitboxExtension,
				(int)Math.Round(player.itemLocation.Y) - (hitboxExtension / 2),
				player.itemWidth + hitboxExtension * 2,
				player.itemHeight + hitboxExtension
			);
		}

		public override void UseStyle(Player player) {
			rotation += .2f;
			player.itemRotation = rotation * player.direction;
			player.itemLocation += new Vector2(7 * player.direction, 5);
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Paintbrush);
			recipe.AddIngredient(ItemID.PaintRoller);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool UseItem(Player player) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			if(p == null)
				return false;
			soundTimer--;
			if(soundTimer <= 0) {
				Main.PlaySound(SoundID.Item, player.position);
				soundTimer = soundFrequency;
			}
			Point mousePosition = Main.MouseWorld.ToTileCoordinates();
			Point playerPos = player.position.ToTileCoordinates();
			int xOffset = mousePosition.X - playerPos.X;
			int yOffset = mousePosition.Y - playerPos.Y;
			if(yOffset < 0)
				yOffset--;
			if(isInRange(player, xOffset, yOffset)) {
				PaintData data = p.paintData.clone();
				data.paintMethod = PaintMethods.BlocksAndWalls;
				paint(mousePosition.X, mousePosition.Y, data, true);
			}
			return true;
		}

		public bool isInRange(Player player, int xOffset, int yOffset) {
			return Math.Abs(xOffset) <= player.lastTileRangeX + item.tileBoost && Math.Abs(yOffset) <= player.lastTileRangeY + item.tileBoost;
		}

		protected override Texture2D getTexture(WoMDPlayer player) {
			if(player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null)
				return null;
			return getExtraTexture(GetType().Name + "Painted");
		}

		public override PaintMethods overridePaintMethod(WoMDPlayer player) => PaintMethods.BlocksAndWalls;
	}

	public class SpectrePaintingMultiTool : PaintingMultiTool {
		public SpectrePaintingMultiTool() : base() {
			hitboxExtension = 12;
			soundFrequency = 7;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spectre Paint Multi-Tool");
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SpectrePaintbrush);
			recipe.AddIngredient(ItemID.SpectrePaintRoller);
			recipe.AddTile(TileID.DyeVat);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void SetDefaults() {
			base.SetDefaults();

			item.value = Item.buyPrice(0, 0, 0, 10);
			item.rare = ItemRarityID.Green;
			item.damage = 70;
			item.knockBack = 7f;
			item.tileBoost = 3;
			item.useTime = 3;
			item.useAnimation = 7;
			hitboxExtension = 6;
		}

		public override void UseStyle(Player player) {
			rotation += .05f;
			base.UseStyle(player);
		}
	}
}