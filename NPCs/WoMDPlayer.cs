using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
	public class WoMDPlayer : ModPlayer {
		protected override bool CloneNewInstances => false;

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

		public Vector2[] oldPos = new Vector2[4];

		/// <summary>
		/// Whether or not the player has the Artist's Palette accessory equipped
		/// </summary>
		public bool accPalette = false;

		/// <summary>
		/// Whether or not the player has the Rewards Program accessory equipped
		/// </summary>
		public bool accRewardsProgram = false;

		protected const int fartDelay = 60 * 10; //10 seconds
		protected uint mountUnicornTime = 0;
		protected uint lastFartTime = 0;

		public WoMDPlayer() : base() {
			accPalette = false;
			_paintData = new PaintData(paintCyclingTimeScale, -1, null, player: Player);
			_currentPaintIndex = -1;
			for(byte i = 0; i < oldPos.Length; i++)
				oldPos[i] = new Vector2(0, 0);
		}

		public override void ResetEffects() {
			buffPainted = false;
			accPalette = false;
			accRewardsProgram = false;
		}

		public override void UpdateDead() {
			buffPainted = false;
			for(byte i = 0; i < oldPos.Length; i++)
				oldPos[i] = new Vector2(0, 0);
		}

		public override void PreUpdate() {
			_paintDataSet = false;
		}

		public override void PostUpdateBuffs() {
			base.PostUpdateBuffs();
			if(chaosMode() && Player.whoAmI == Main.myPlayer) {
				if(Player.HasBuff(BuffID.UnicornMount)) {
					if(mountUnicornTime == 0)
						mountUnicornTime = Main.GameUpdateCount;
					if(Main.GameUpdateCount - lastFartTime > fartDelay && Main.GameUpdateCount - mountUnicornTime > fartDelay) {
						lastFartTime = Main.GameUpdateCount;
						if(Main.rand.NextFloat() <= .3f) {
							SoundEngine.PlaySound(SoundID.Item16, Player.position);
							int fartDirection = Player.direction * -1;
							Point fartPosition = (Player.position + new Vector2(32 * fartDirection, 48)).ToTileCoordinates();
							int[] heights = new int[] { 1, 3, 3, 5, 5, 7 };
							byte[] colors = new byte[] { PaintID.DeepRedPaint, PaintID.DeepOrangePaint, PaintID.DeepYellowPaint, PaintID.DeepGreenPaint, PaintID.DeepBluePaint, PaintID.DeepPurplePaint };

							for(int i = 0; i < 6; i++) {
								Dust d = Dust.NewDustPerfect(fartPosition.ToWorldCoordinates() + new Vector2(8, 8), 22, new Vector2(fartDirection * 3f, 0).RotatedBy(Main.rand.NextFloat((float)(Math.PI / -6f), (float)(Math.PI / 6f))) * 3f, 0, default, 1f);
								d.noGravity = true;
								d.fadeIn = 1.5f;
							}

							getWorld().addAnimation(new PaintAnimation(heights.Length - 1, 3, index => {
								int height = heights[index];
								for(int i = 0; i < height; i++) {
									Point coords = new Point(
										fartPosition.X + (index * fartDirection),
										fartPosition.Y + (((height - 1) / -2) + i)
									);
									paint(coords.X, coords.Y, new PaintData(colors[index]), true);
								}
								return true;
							}));
						}
					}
				} else {
					mountUnicornTime = 0;
				}
			}
		}

		public override void PostUpdate() {
			if(!Player.dead) {
				for(byte i = (byte)(oldPos.Length - 1); i > 0; i--)
					oldPos[i] = oldPos[i - 1];
				oldPos[0] = Player.Center;
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if(GetInstance<WoMDConfig>().chaosModeEnabled && Player.whoAmI == Main.myPlayer) {
				int projIndex = damageSource.SourceProjectileIndex;
				int npcIndex = damageSource.SourceNPCIndex;
				int playerIndex = damageSource.SourcePlayerIndex;
				int itemType = damageSource.SourceItemType;
				int otherIndex = damageSource.SourceOtherIndex;
				string customReason = damageSource.SourceCustomReason;
				bool preventDefault = false;
				if(customReason != null) {

				} else if(projIndex >= 0) {
					switch(damageSource.SourceProjectileType) {
						case ProjectileID.Boulder:
							if(true) {
								Projectile proj = getProjectile(projIndex);
								if(proj != null)
									WoMDProjectile.applyPainted(proj, new PaintData(PaintID.DeepRedPaint));
							}
							break;
					}
				} else if(npcIndex >= 0) {

				} else if(itemType > 0) {

				} else if(playerIndex >= 0) {

				} else if(otherIndex >= 0) {
					switch(otherIndex) {
						case OtherDeathID.Poisoned:
							byte[] colors = new byte[] { PaintID.DeepGreenPaint };
							if(Player.HasBuff(BuffID.Poisoned)) {
								colors = new byte[] { PaintID.DeepGreenPaint, PaintID.DeepLimePaint };
							}else if(Player.HasBuff(BuffID.Venom)) {
								colors = new byte[] { PaintID.DeepPurplePaint, PaintID.DeepVioletPaint };
							}
							byte color = colors[Main.rand.Next(colors.Length)];
							preventDefault = true;
							for(byte i = 0; i < 8; i++) {
								Projectile proj = Projectile.NewProjectileDirect(
									null,
									Player.Center, 
									new Vector2(0, 1).RotatedBy(((PI * 2f) / 8f) * i) * Main.rand.NextFloat(3,4), 
									ProjectileType<PaintSplatter>(),
									0,
									0
								);
								if(proj != null) {
									proj.timeLeft = 40;
									proj.velocity.Y -= 2;
									proj.velocity += Player.velocity.SafeNormalize(new Vector2(0,0))*4;
									PaintSplatter splatter = proj.ModProjectile as PaintSplatter;
									if(splatter != null)
										splatter.setOverridePaintData(new PaintData(color));
								}
							}
							break;
						case OtherDeathID.ChaosState:
						case OtherDeathID.ChaosState2:
							preventDefault = true;

							Vector2 dir = (Player.Center - oldPos[1]).SafeNormalize(new Vector2(0,1));
							PaintData data = new PaintData(PaintID.DeepPinkPaint);
							Vector2[] dirs = new Vector2[7];
							int[] lengths = new int[7];
							int maxLength = 0;
							for(byte i = 0; i < dirs.Length; i++) {
								dirs[i] = dir.RotatedBy(Main.rand.NextFloat(PI / -5, PI / 5));
								int length = Main.rand.Next(7, 13);
								lengths[i] = length;
								if(length > maxLength)
									maxLength = length;
							}

							getWorld().addAnimation(new PaintAnimation(maxLength, 1, index => {
								for(byte i = 0; i < dirs.Length; i++) {
									if(index <= lengths[i]) {
										Vector2 disp = dirs[i] * 16f * (float)index;
										paint((Player.Center + disp).ToTileCoordinates(), data, true);
									}
								}
								return true;
							}));
							break;
					}
				}
				if(!preventDefault)
					splatter(Player.Center, 130f, 6, new PaintData(PaintID.DeepRedPaint), true);
			}
		}

		protected void setPaintData() {
			//don't overwrite _paintData with a new PaintData
			//a PaintingProjectile could be in the middle of using the paint data while the player runs out of a certain type of paint
			_currentPaintIndex = -1;
			_paintData.PaintColor = -1;
			_paintData.CustomPaint = null;
			_paintData.sprayPaint = false;
			_paintData.player = getPlayer(Player.whoAmI);
			_paintData.paintMethod = PaintMethods.None;
			for(int i = 0; i < Player.inventory.Length && ((_paintData.PaintColor == -1 && _paintData.CustomPaint == null) || _paintData.paintMethod == PaintMethods.None); i++) {
				Item item = Player.inventory[i];
				if(item.active && item.stack > 0 && i != 58) {
					item.playerIndexTheItemIsReservedFor = Player.whoAmI;
					if(_currentPaintIndex < 0 && isPaint(item)) {
						_currentPaintIndex = i;
						if(item.paint == CustomPaint.paintValue && item.ModItem is CustomPaint) {
							_paintData.CustomPaint = item.ModItem as CustomPaint;
							_paintData.sprayPaint = _paintData.CustomPaint is ISprayPaint;
						} else {
							_paintData.PaintColor = item.paint;
						}
					} else if(_paintData.paintMethod == PaintMethods.None && isPaintingTool(item)) {
						if(item.ModItem is PaintingMultiTool) {
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
			return !(_paintData.paintMethod == PaintMethods.None || ((_paintData.PaintColor == -1 && _paintData.CustomPaint == null) && _paintData.paintMethod != PaintMethods.RemovePaint));
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
				if(_paintData.CustomPaint != null && _paintData.CustomPaint.paintConsumptionChance < 1f && Main.rand.NextFloat() <= _paintData.CustomPaint.paintConsumptionChance)
					return;
				Item item = Player.inventory[_currentPaintIndex];
				if(item.stack > 0)
					item.stack--;
				if(item.stack == 0) {
					item.active = false;
					setPaintData();
					data.PaintColor = _paintData.PaintColor;
					data.CustomPaint = _paintData.CustomPaint;
					data.sprayPaint = _paintData.sprayPaint;
				}
			}
		}
	}
}