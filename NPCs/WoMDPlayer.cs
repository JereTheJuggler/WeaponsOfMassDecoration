using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Items;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
	public class WoMDPlayer : ModPlayer {
		public override bool CloneNewInstances => false;

		/// <summary>
		/// The index of the first slot containing paint in the player's inventory
		/// </summary>
		protected int _currentPaintIndex;

		protected bool _paintDataSet = false;
		protected PaintData _paintData = new PaintData(paintCyclingTimeScale, 0);
		public PaintData paintData {
			get {
				if(!_paintDataSet)
					setPaintData();
				return _paintData;
			}
		}

		public bool buffPainted;
		public int buffPaintedTime;
		public int buffPaintedColor;
		public CustomPaint buffPaintedCustomPaint;

		/// <summary>
		/// Whether or not the player has the Artist's Palette accessory equipped
		/// </summary>
		public bool accPalette = false;

		public float mountUnicornTime = 0;
		public float lastFartTime = 0;
		public int fartAnimationFrame = -1;
		public Point fartPosition = new Point(0, 0);
		public int fartDirection = 1;

		public WoMDPlayer() : base() {
			accPalette = false;
			_paintData = new PaintData(paintCyclingTimeScale, -1, null, player: player);
			_currentPaintIndex = -1;
		}

		public override void ResetEffects() {
			buffPainted = false;
			accPalette = false;
		}

		public override void UpdateDead() {
			buffPainted = false;
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			base.Kill(damage, hitDirection, pvp, damageSource);
			if(GetInstance<WoMDConfig>().chaosModeEnabled)
				splatter(player.Center, 130f, 6, new PaintData(PaintID.DeepRed), true);
		}

		public override void PreUpdate() {
			_paintDataSet = false;
		}

		public override void PostUpdateBuffs() {
			base.PostUpdateBuffs();
			if(GetInstance<WoMDConfig>().chaosModeEnabled) {
				if(player.HasBuff(BuffID.UnicornMount)) {
					if(mountUnicornTime == 0)
						mountUnicornTime = Main.GlobalTime;
					if(fartAnimationFrame == -1 && Main.GlobalTime - lastFartTime > 10 && Main.GlobalTime - mountUnicornTime > 10) {
						if(Main.rand.NextFloat() <= .3f) {
							fartAnimationFrame = 0;
							lastFartTime = Main.GlobalTime;
							fartDirection = player.direction * -1;
							fartPosition = (player.position + new Vector2(32 * fartDirection, 48)).ToTileCoordinates();
							Main.PlaySound(SoundID.Item16, player.position);
							for(int i = 0; i < 6; i++) {
								Dust d = Dust.NewDustPerfect(fartPosition.ToWorldCoordinates() + new Vector2(8, 8), 22, new Vector2(fartDirection * 3f, 0).RotatedBy(Main.rand.NextFloat((float)(Math.PI / -6f), (float)(Math.PI / 6f))) * 3f, 0, default, 1f);
								d.noGravity = true;
								d.fadeIn = 1.5f;
							}
						}
					}
				} else {
					mountUnicornTime = 0;
				}
				if(fartAnimationFrame >= 0 && fartAnimationFrame <= 3 && Main.rand.NextFloat() < .5f) {
					Dust d = Dust.NewDustPerfect(fartPosition.ToWorldCoordinates() + new Vector2(8, 8), 22, new Vector2(fartDirection * 3f, 0).RotatedBy(Main.rand.NextFloat((float)(Math.PI / -6f), (float)(Math.PI / 6f))) * 3f, 0, default, 1f);
					d.noGravity = true;
					d.fadeIn = 1.5f;
				}
				if(fartAnimationFrame >= 0 && Main.GlobalTime - lastFartTime > (.05f * fartAnimationFrame)) {
					int[] heights = new int[] { 1, 3, 3, 5, 5, 7 };
					byte[] colors = new byte[] { PaintID.DeepRed, PaintID.DeepOrange, PaintID.DeepYellow, PaintID.DeepGreen, PaintID.DeepBlue, PaintID.DeepPurple };
					int height = heights[fartAnimationFrame];
					for(int i = 0; i < height; i++) {
						Point coords = new Point(
							fartPosition.X + (fartAnimationFrame * fartDirection),
							fartPosition.Y + (((height - 1) / -2) + i)
						);
						paint(coords.X, coords.Y, new PaintData(colors[fartAnimationFrame]), true);
					}
					fartAnimationFrame++;
					if(fartAnimationFrame >= heights.Length)
						fartAnimationFrame = -1;
				}
			}
		}

		protected void setPaintData() {
			//don't overwrite _paintData with a new PaintData
			//a PaintingProjectile could be in the middle of using the paint data while the player runs out of a certain type of paint
			_currentPaintIndex = -1;
			_paintData.paintColor = -1;
			_paintData.customPaint = null;
			_paintData.sprayPaint = false;
			_paintData.player = getPlayer(player.whoAmI);
			_paintData.paintMethod = PaintMethods.None;
			for(int i = 0; i < player.inventory.Length && ((_paintData.paintColor == -1 && _paintData.customPaint == null) || _paintData.paintMethod == PaintMethods.None); i++) {
				Item item = player.inventory[i];
				if(item.active && item.stack > 0 && i != 58) {
					item.owner = player.whoAmI;
					if(_currentPaintIndex < 0 && isPaint(item)) {
						_currentPaintIndex = i;
						if(item.paint == CustomPaint.paintValue && item.modItem is CustomPaint) {
							_paintData.customPaint = item.modItem as CustomPaint;
							_paintData.sprayPaint = _paintData.customPaint is ISprayPaint;
						} else {
							_paintData.paintColor = item.paint;
						}
					} else if(_paintData.paintMethod == PaintMethods.None && isPaintingTool(item)) {
						if(item.modItem is PaintingMultiTool) {
							_paintData.paintMethod = PaintMethods.BlocksAndWalls;
						} else {
							switch(item.type) {
								case ItemID.Paintbrush:
								case ItemID.SpectrePaintbrush:
									_paintData.paintMethod = PaintMethods.Blocks;
									break;
								case ItemID.PaintRoller:
								case ItemID.SpectrePaintRoller:
									_paintData.paintMethod = PaintMethods.Walls;
									break;
								case ItemID.PaintScraper:
								case ItemID.SpectrePaintScraper:
									_paintData.paintMethod = PaintMethods.RemovePaint;
									break;
							}
						}
					}
				}
			}
			_paintDataSet = true;
		}

		/// <summary>
		/// A quick check to test if the player is currently able to paint. Useful for optimization to check before running a bunch of subsequent functions that would need to check this individually
		/// </summary>
		/// <returns></returns>
		public bool canPaint() {
			if(!_paintDataSet)
				setPaintData();
			return !(_paintData.paintMethod == PaintMethods.None || ((_paintData.paintColor == -1 && _paintData.customPaint == null) && _paintData.paintMethod != PaintMethods.RemovePaint));
		}

		/// <summary>
		/// Consumes paint if necessary when a player paints a block or wall
		/// </summary>
		/// <param name="data">An instance of PaintData. This is provided for scenarios where the paintMethod is altered before painting occurred. All other properties from PaintData are gathered from the player's protected _paintData</param>
		public void consumePaint(PaintData data) {
			if(accPalette)
				return;
			if(!_paintDataSet)
				setPaintData();
			if(data.paintMethod == PaintMethods.RemovePaint)
				return;
			if(_currentPaintIndex >= 0) {
				if(_paintData.customPaint != null && _paintData.customPaint.paintConsumptionChance < 1f && Main.rand.NextFloat() <= _paintData.customPaint.paintConsumptionChance)
					return;
				Item item = player.inventory[_currentPaintIndex];
				if(item.stack > 0)
					item.stack--;
				if(item.stack == 0) {
					item.active = false;
					setPaintData();
					data.paintColor = _paintData.paintColor;
					data.customPaint = _paintData.customPaint;
					data.sprayPaint = _paintData.sprayPaint;
				}
			}
		}
	}
}