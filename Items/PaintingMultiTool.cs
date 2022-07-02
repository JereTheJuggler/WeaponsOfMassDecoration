using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
	public class PaintingMultiTool : PaintingItem {
		protected static float rotation = 0;
		public int soundTimer = 0;

		public virtual int HitboxExtension => 0;
		public virtual int SoundFrequency => 1;

		public override int TextureCount => 2;
		public override bool UsesGSShader => true;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paint Multi-Tool");
			Tooltip.SetDefault(halfDamageText + "Paints blocks and walls at the same time!\nCurrent Paint: ");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Paintbrush);
			Item.holdStyle = 0;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 29;
			Item.height = 30;
			Item.maxStack = 1;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.DamageType = DamageClass.Melee;
			Item.damage = 4;
			Item.knockBack = 3f;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(0, 0, 0, 10);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 15;
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-26, 2);//.RotatedBy(rotation);
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			rotation = 0;
		}

		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			hitbox = new Rectangle(
				(int)Math.Round(player.itemLocation.X) - HitboxExtension,
				(int)Math.Round(player.itemLocation.Y) - (HitboxExtension / 2),
				player.itemWidth + HitboxExtension * 2,
				player.itemHeight + HitboxExtension
			);
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			rotation += .2f;
			player.itemRotation = rotation * player.direction;
			player.itemLocation += new Vector2(7 * player.direction, 5);
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Paintbrush);
			recipe.AddIngredient(ItemID.PaintRoller);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}

		public override bool? UseItem(Player player){
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			if(p == null)
				return null;
			soundTimer--;
			if(soundTimer <= 0) {
				//TODO get this sound working again
				SoundEngine.PlaySound(SoundID.Item1, player.position);
				soundTimer = SoundFrequency;
			}
			Point mousePosition = Main.MouseWorld.ToTileCoordinates();
			Point playerPos = player.position.ToTileCoordinates();
			int xOffset = mousePosition.X - playerPos.X;
			int yOffset = mousePosition.Y - playerPos.Y;
			if(yOffset < 0)
				yOffset--;
			if(IsInRange(player, xOffset, yOffset)) {
				PaintData data = p.paintData.Clone();
				data.paintMethod = PaintMethods.BlocksAndWalls;
				Paint(mousePosition.X, mousePosition.Y, data, true);
			}
			return true;
		}

		public bool IsInRange(Player player, int xOffset, int yOffset) {
			return Math.Abs(xOffset) <= player.lastTileRangeX + Item.tileBoost && Math.Abs(yOffset) <= player.lastTileRangeY + Item.tileBoost;
		}

		protected override Texture2D GetTexture(WoMDPlayer player) {
			if(player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null)
				return null;
			return GetExtraTexture(GetType().Name + "Painted");
		}

		public override PaintMethods OverridePaintMethod(WoMDPlayer player) => PaintMethods.BlocksAndWalls;
	}

	//TODO get the animation for spinning to work again
	public class SpectrePaintingMultiTool : PaintingMultiTool {
		public override int HitboxExtension => 6;
		public override int SoundFrequency => 7;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spectre Paint Multi-Tool");
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SpectrePaintbrush);
			recipe.AddIngredient(ItemID.SpectrePaintRoller);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Item.buyPrice(0, 0, 0, 10);
			Item.rare = ItemRarityID.Green;
			Item.damage = 70;
			Item.knockBack = 7f;
			Item.tileBoost = 3;
			Item.useTime = 3;
		}

		/*public override void UseStyle(Player player, Rectangle heldItemFrame) {
			rotation += .05f;
			base.UseStyle(player,heldItemFrame);
		}*/
	}
}