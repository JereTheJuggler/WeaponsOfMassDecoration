using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using WeaponsOfMassDecoration.Buffs;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration {
	public enum PaintMethods{
		None,
		RemovePaint,
		Tiles,
		Walls,
		TilesAndWalls,
		NotSet
	}
	class WeaponsOfMassDecoration : Mod{
		/// <summary>
		/// The speed that custom paints cycle through colors for painting tiles. Also applies to the colors for projectile shaders to line up with the color they are painting.
		/// </summary>
		public const float paintCyclingTimeScale = .25f; 

		/// <summary>
		/// The speed that custom paints cycle through colors for npc shaders
		/// </summary>
		public const float npcCyclingTimeScale = 1f;

		public WeaponsOfMassDecoration(){
			Properties = new ModProperties(){
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
        }
		
		public override void Load() {
			if(Main.netMode != NetmodeID.Server) {
				Ref<Effect> paintedRef = new Ref<Effect>(GetEffect("Effects/Painted"));
				GameShaders.Misc["Painted"] = new MiscShaderData(paintedRef, "paintedColor").UseColor(1f, 0, 0).UseOpacity(1f);
				
				/*Ref<Effect> paintedNegativeRef = new Ref<Effect>(GetEffect("Effects/PaintedNegative"));
				GameShaders.Misc["PaintedNegative"] = new MiscShaderData(paintedNegativeRef, "paintedNegativeColor");*/

				Ref<Effect> sprayPaintedRef = new Ref<Effect>(GetEffect("Effects/SprayPainted"));
				GameShaders.Misc["SprayPainted"] = new MiscShaderData(sprayPaintedRef, "sprayPaintedColor").UseImage("Images/Misc/noise");
			}
		}

		public override void AddRecipeGroups() {
			RecipeGroup hmBarGroup1 = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Cobalt Bar", new int[]{
				ItemID.CobaltBar,
				ItemID.PalladiumBar
			});
			RecipeGroup.RegisterGroup("WoMD:hmBar1", hmBarGroup1);

			RecipeGroup basePaintGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Base Paint", new int[]{
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
			RecipeGroup.RegisterGroup("WoMD:basePaints", basePaintGroup);

			RecipeGroup deepPaintGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Deep Paint", new int[]{
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
			RecipeGroup.RegisterGroup("WoMD:deepPaints", deepPaintGroup);

			RecipeGroup goldSwordGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Gold Broadsword", new int[]{
				ItemID.GoldBroadsword,
				ItemID.PlatinumBroadsword
			});
			RecipeGroup.RegisterGroup("WoMD:goldSword", goldSwordGroup);
		}

	#region getting from arrays
		/// <summary>
		/// Safely gets a Dust object from Main.dust. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the dust in Main.dust</param>
		/// <returns></returns>
		public static Dust getDust(int index) {
			if(index < 0 || index >= Main.dust.Length - 1)
				return null;
			return Main.dust[index];
		}

		/// <summary>
		/// Safely gets a Projectile object from Main.projectile. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the projectile in Main.projectile</param>
		/// <returns></returns>
		public static Projectile getProjectile(int index) {
			if(index < 0 || index >= Main.projectile.Length - 1)
				return null;
			return Main.projectile[index];
		}

		/// <summary>
		/// Safely gets a Player object from Main.player. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the player in Main.player</param>
		/// <returns></returns>
		public static Player getPlayer(int index) {
			if(index < 0 || index >= Main.player.Length - 1)
				return null;
			return Main.player[index];
		}

		/// <summary>
		/// Safely gets a NPC object from Main.npc. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the npc in Main.npc</param>
		/// <returns></returns>
		public static NPC getNPC(int index) {
			if(index < 0 || index >= Main.npc.Length - 1)
				return null;
			return Main.npc[index];
		}
	#endregion

		/// <summary>
		/// Checks if the provided item is either a vanilla paint, or a CustomPaint
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool isPaint(Item item) {
			if(item.modItem is CustomPaint)
				return true;
			if(PaintIDs.itemIds.Contains(item.type))
				return true;
			return false;
		}
		/// <summary>
		/// Checks if the provided item is either a vanilla painting tool, or a PaintingMultiTool
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool isPaintingTool(Item item) {
			if(item.modItem is PaintingMultiTool)
				return true;
			switch(item.type) {
				case ItemID.Paintbrush:
				case ItemID.PaintRoller:
				case ItemID.PaintScraper:
				case ItemID.SpectrePaintbrush:
				case ItemID.SpectrePaintRoller:
				case ItemID.SpectrePaintScraper:
					return true;
			}
			return false;
		}

	#region shaders
		/// <summary>
		/// Applies a shader for the provided WoMDGlobalNPC, based on its painted, paintColor, customPaint, and paintedTime properties.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="npc"></param>
		/// <param name="drawData"></param>
		/// <returns></returns>
		public static ShaderData applyShader(WoMDGlobalNPC globalNpc, NPC npc, DrawData? drawData = null) {
			if(!globalNpc.painted)
				return null;
			if(globalNpc.paintColor == PaintID.Negative)
				return applyNegativeShader(npc);
			Color color = getColor(globalNpc.paintColor, globalNpc.customPaint, npcCyclingTimeScale, globalNpc.paintedTime, null);
			if(globalNpc.sprayPainted)
				return applySprayPaintedShader(color, drawData);
			return applyPaintedShader(color);
		}
		/// <summary>
		/// Applies a shader for the provided PaintingProjectile
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static ShaderData applyShader(PaintingProjectile projectile) {
			WoMDPlayer player = projectile.getModPlayer();
			if(player == null)
				return null;
			player.getPaintVars(out int paintColor, out CustomPaint customPaint);
			if(paintColor == -1 && customPaint == null)
				return null;
			if(paintColor == PaintID.Negative)
				return applyNegativeShader(projectile.projectile);
			Color c = getColor(paintColor, customPaint, paintCyclingTimeScale, 0, projectile.getOwner());
			return applyPaintedShader(c);
		}

		private static ArmorShaderData applyNegativeShader(Entity entity) {
			ArmorShaderData data = GameShaders.Armor.GetShaderFromItemId(ItemID.NegativeDye);
			data.Apply(entity);
			return data;
		}
		private static MiscShaderData applyPaintedShader(Color c) {
			MiscShaderData data = GameShaders.Misc["Painted"].UseColor(c).UseOpacity(1f);
			data.Apply();
			return data;
		}
		private static MiscShaderData applySprayPaintedShader(Color color, DrawData? drawData = null) {
			MiscShaderData data = GameShaders.Misc["SprayPainted"].UseColor(color).UseImage("Images/Misc/noise").UseOpacity(1f);
			data.Apply(drawData);
			return data;
		}

		/// <summary>
		/// Gets the color for the provided PaintingProjectile, based on properties from the projectile's owner
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static Color getColor(PaintingProjectile projectile) {
			WoMDPlayer player = projectile.getModPlayer();
			if(player == null)
				return Color.White;
			player.getPaintVars(out int paintColor, out CustomPaint customPaint);
			return getColor(paintColor, customPaint, paintCyclingTimeScale, 0, projectile.getOwner());
		}

		private static Color getColor(int paintColor, CustomPaint customPaint, float timeScale, float timeOffset = 0, Player player = null) {
			if(customPaint == null)
				return PaintColors.colors[paintColor];
			return customPaint.getColor(new CustomPaintData(true, timeScale, timeOffset, player));
		}
	#endregion

		/// <summary>
		/// Applies the painted buff to the provided npc, based on the paintColor and customPaint provided
		/// </summary>
		/// <param name="npc">The npc to apply the buff to</param>
		/// <param name="paintColor">The PaintID to use for painting the npc. Should be -1 when using a CustomPaint</param>
		/// <param name="customPaint">The CustomPaint to use for painting the npc. Should be null when using a vanilla paint</param>
		/// <param name="handledNpcs">Should not be provided</param>
		/// <param name="preventRecursion">Should not be provided</param>
		public static void applyPaintedToNPC(NPC npc, int paintColor, CustomPaint customPaint, CustomPaintData data, List<NPC> handledNpcs = null, bool preventRecursion = false) {
			switch(npc.type) {
				case NPCID.CultistDragonBody1:
				case NPCID.CultistDragonBody2:
				case NPCID.CultistDragonBody3:
				case NPCID.CultistDragonBody4:
				case NPCID.CultistDragonHead:
				case NPCID.CultistDragonTail:
				case NPCID.CultistBossClone:
				case NPCID.CultistBoss:
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				case NPCID.AncientCultistSquidhead:
				case NPCID.NebulaBeast:
				case NPCID.LunarTowerNebula:
				case NPCID.LunarTowerSolar:
				case NPCID.StardustWormTail:
				case NPCID.StardustWormBody:
				case NPCID.StardustWormHead:
				case NPCID.LunarTowerStardust:
				case NPCID.LunarTowerVortex:
				case NPCID.DungeonSpirit:
				case NPCID.Tumbleweed:
				case NPCID.DesertDjinn:
				case NPCID.Ghost:
				case NPCID.MartianSaucer:
				case NPCID.MartianSaucerCannon:
				case NPCID.MartianSaucerCore:
				case NPCID.MartianSaucerTurret:
				case NPCID.Poltergeist:
				case NPCID.Pumpking:
				case NPCID.PumpkingBlade:
					return;
			}
			if(preventRecursion)
				return;

			npc.AddBuff(ModContent.BuffType<Painted>(), 6000);

			WoMDGlobalNPC globalNpc = npc.GetGlobalNPC<WoMDGlobalNPC>();

			if(customPaint != null)
				customPaint.getPaintVarsForNpc(out paintColor,out customPaint, data);

			if(!globalNpc.painted || (customPaint != null && (globalNpc.customPaint == null || globalNpc.customPaint.displayName != customPaint.displayName)) || (paintColor != -1 && globalNpc.paintColor != paintColor))
				globalNpc.paintedTime = Main.GlobalTime;
			globalNpc.painted = true;
			if(customPaint != null) {
				globalNpc.paintColor = -1;
				globalNpc.customPaint = (CustomPaint)customPaint.Clone();
				globalNpc.sprayPainted = customPaint is ISprayPaint;
			} else {
				globalNpc.customPaint = null;
				globalNpc.paintColor = paintColor;
			}

			if(Main.netMode == NetmodeID.SinglePlayer) {
				if(handledNpcs == null)
					handledNpcs = new List<NPC>();
				handledNpcs.Add(npc);
				switch(npc.type) {
					case NPCID.MoonLordHead:
					case NPCID.MoonLordHand:
					case NPCID.MoonLordCore:
					case NPCID.MoonLordLeechBlob:
						List<int> moonLordParts = new List<int> { NPCID.MoonLordCore, NPCID.MoonLordHand, NPCID.MoonLordHead, NPCID.MoonLordLeechBlob };
						for(int i = 0; i < Main.npc.Length; i++) {
							if(Main.npc[i].TypeName == "")
								break;
							switch(Main.npc[i].type) {
								case NPCID.MoonLordHead:
								case NPCID.MoonLordHand:
								case NPCID.MoonLordCore:
									applyPaintedToNPC(Main.npc[i], paintColor, customPaint, data ,null, true);
									break;
							}
						}
						break;
					case NPCID.EaterofWorldsBody:
					case NPCID.DevourerBody:
					case NPCID.WyvernBody:
					case NPCID.WyvernBody2:
					case NPCID.WyvernBody3:
					case NPCID.WyvernLegs:
					case NPCID.GiantWormBody:
					case NPCID.DiggerBody:
					case NPCID.SeekerBody: //world feeder
					case NPCID.TombCrawlerBody:
					case NPCID.DuneSplicerBody:
					case NPCID.SolarCrawltipedeBody:
					case NPCID.BoneSerpentBody:
						NPC prevSection = getNpcById(npc.ai[0]);
						if(prevSection != null && !handledNpcs.Contains(prevSection)) {
							NPC thisSection = getNpcById(prevSection.ai[1]);
							if(thisSection != null && thisSection.Equals(npc)) {
								applyPaintedToNPC(prevSection, paintColor, customPaint, data, handledNpcs);
							}
						}
						goto case NPCID.EaterofWorldsTail;
					case NPCID.EaterofWorldsHead:
					case NPCID.DevourerHead:
					case NPCID.WyvernHead:
					case NPCID.GiantWormHead:
					case NPCID.DiggerHead:
					case NPCID.SeekerHead: //world feeder
					case NPCID.TombCrawlerHead:
					case NPCID.DuneSplicerHead:
					case NPCID.SolarCrawltipedeHead:
					case NPCID.BoneSerpentHead:
						NPC previSection = getNpcById(npc.ai[0]);
						if(previSection != null && !handledNpcs.Contains(previSection)) {
							NPC thisSection = getNpcById(previSection.ai[1]);
							if(thisSection != null && thisSection.Equals(npc)) {
								applyPaintedToNPC(previSection, paintColor, customPaint, data, handledNpcs);
							}
						}
						break;
					case NPCID.EaterofWorldsTail:
					case NPCID.DevourerTail:
					case NPCID.WyvernTail:
					case NPCID.GiantWormTail:
					case NPCID.DiggerTail:
					case NPCID.SeekerTail: //world feeder
					case NPCID.TombCrawlerTail:
					case NPCID.DuneSplicerTail:
					case NPCID.SolarCrawltipedeTail:
					case NPCID.BoneSerpentTail:
						NPC nextSection = getNpcById(npc.ai[1]);
						if(nextSection != null && !handledNpcs.Contains(nextSection)) {
							NPC thisSection = getNpcById(nextSection.ai[0]);
							if(thisSection != null && thisSection.Equals(npc)) {
								applyPaintedToNPC(nextSection, paintColor, customPaint, data, handledNpcs);
							}
						}
						break;
				}
			}
		}

		private static NPC getNpcById(float id) {
			if(id != Math.Round(id))
				return null;
			if(id >= 0 && id < Main.npc.Length)
				return Main.npc[(int)id];
			return null;
		}

		/// <summary>
		/// Clamps a value between a min and max value
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float clamp(float value, float min, float max) {
			if(value < min)
				return min;
			if(value > max)
				return max;
			return value;
		}

		/// <summary>
		/// Clamps a value between a min and max value
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int clamp(int value, int min, int max) {
			if(value < min)
				return min;
			if(value > max)
				return max;
			return value;
		}

		/// <summary>
		/// Clamps a value between a min and max value
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static double clamp(double value, double min, double max) {
			if(value < min)
				return min;
			if(value > max)
				return max;
			return value;
		}

	}
}
