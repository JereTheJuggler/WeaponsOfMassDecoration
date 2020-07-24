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
		//colors used for painting the world (and by extension rendering projectiles) will cycle at this speed for custom paints
		public const float paintCyclingTimeScale = .25f; 

		//colors used for rendering painted npcs will cycle at this speed for custom paints
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
		}

		public static void applyShader(Entity entity, ShaderData shader, DrawData? drawData = null, float opacity = 1) {
			if(shader == null)
				return;
			if(shader is ArmorShaderData) {
				((ArmorShaderData)shader).UseOpacity(opacity).Apply(entity, drawData);
			} else if(shader is MiscShaderData){
				((MiscShaderData)shader).UseOpacity(opacity).Apply(drawData);
			}
		}

		public static ShaderData getShaderData(WoMDGlobalNPC npc) {
			if(!npc.painted)
				return null;
			getRenderVars(npc, out byte currentColor, out byte? nextColor, out bool sprayPainted);
			if(currentColor == 0)
				return null;
			return getShaderData(currentColor, nextColor, sprayPainted, npcCyclingTimeScale, npc.paintedTime);
		}
		public static ShaderData getShaderData(PaintingProjectile projectile) {
			getRenderVars(projectile, out byte currentColor, out byte? nextColor, out bool sprayPainted);
			if(currentColor == 0)
				return null;
			//spray painted doesn't apply for projectiles
			return getShaderData(currentColor, nextColor, false, paintCyclingTimeScale);
		}
		
		public static ShaderData applyShader(WoMDGlobalNPC globalNpc, NPC npc, DrawData? drawData = null) {
			if(!globalNpc.painted)
				return null;
			getRenderVars(globalNpc, out byte currentColor, out byte? nextColor, out bool sprayPainted);
			if(currentColor == 0)
				return null;
			return applyShader(npc, currentColor, nextColor, sprayPainted, npcCyclingTimeScale, globalNpc.paintedTime, drawData, 1f);
		}
		public static ShaderData applyShader(PaintingProjectile projectile) {
			getRenderVars(projectile, out byte currentColor, out byte? nextColor, out bool sprayPainted);
			if(currentColor == 0)
				return null;
			return applyShader(projectile.projectile, currentColor, nextColor, false, paintCyclingTimeScale, 0, null);
		}

		private static ShaderData applyShader(Entity entity, byte currentColor, byte? nextColor, bool sprayPainted, float timeScale, float timeOffset = 0, DrawData? drawData = null, float opacity = 1f) {
			if(currentColor == PaintID.Negative) {
				return applyNegativeShader(entity).UseOpacity(opacity);
			} else if(sprayPainted){
				return applySprayPaintedShader(currentColor, nextColor, timeScale, timeOffset, drawData).UseOpacity(opacity);
			} else {
				return applyPaintedShader(currentColor, nextColor, timeScale, timeOffset).UseOpacity(opacity);
			}
		}
		private static ArmorShaderData applyNegativeShader(Entity entity) {
			ArmorShaderData data = GameShaders.Armor.GetShaderFromItemId(ItemID.NegativeDye);
			data.Apply(entity);
			return data;
		}
		private static MiscShaderData applyPaintedShader(byte currentColor, byte? nextColor, float timeScale, float timeOffset = 0) {
			MiscShaderData data = GameShaders.Misc["Painted"].UseColor(getColor(currentColor, nextColor, timeScale, timeOffset));
			data.Apply();
			return data;
		}
		private static MiscShaderData applySprayPaintedShader(byte currentColor, byte? nextColor, float timeScale, float timeOffset = 0, DrawData? drawData = null) {
			MiscShaderData data = GameShaders.Misc["SprayPainted"].UseColor(getColor(currentColor, nextColor, timeScale, timeOffset)).UseImage("Images/Misc/noise");
			data.Apply(drawData);
			return data;
		}

		public static Color getColor(WoMDGlobalNPC npc) {
			if(!npc.painted)
				return Color.White;
			getCurrentAndNextColorIDs(npc.paintColor, npc.customPaint, out byte currentColor, out byte? nextColor, npcCyclingTimeScale, npc.paintedTime);
			return getColor(currentColor, nextColor, npcCyclingTimeScale, npc.paintedTime);
		}
		public static Color getColor(PaintingProjectile projectile) {
			getPaintColorAndCustomPaintFromProjectile(projectile, out int paintColor, out CustomPaint customPaint);
			getCurrentAndNextColorIDs(paintColor, customPaint, out byte currentColor, out byte? nextColor, paintCyclingTimeScale);
			return getColor(currentColor, nextColor, paintCyclingTimeScale);
		}

		public static void getRenderVars(WoMDGlobalNPC npc, out byte currentColor, out byte? nextColor, out bool sprayPainted) {
			if(!npc.painted) {
				currentColor = 0;
				nextColor = null;
				sprayPainted = false;
			} else {
				getRenderVars(npc.paintColor, npc.customPaint, out currentColor, out nextColor, out sprayPainted, npcCyclingTimeScale, npc.paintedTime);
			}
		}
		public static void getRenderVars(PaintingProjectile projectile,out byte currentColor, out byte? nextColor, out bool sprayPainted) {
			getPaintColorAndCustomPaintFromProjectile(projectile, out int paintColor, out CustomPaint customPaint);
			getRenderVars(paintColor, customPaint,out currentColor,out nextColor,out sprayPainted, paintCyclingTimeScale, 0);
		}
		
		public static byte getCurrentColorID(WoMDGlobalNPC npc, bool forceColor = true) {
			if(!npc.painted)
				return 0;
			return getCurrentColorID(forceColor, npc.paintColor, npc.customPaint, npcCyclingTimeScale, npc.paintedTime);
		}
		public static byte getCurrentColorID(PaintingProjectile projectile, bool forceColor = true) {
			getPaintColorAndCustomPaintFromProjectile(projectile, out int paintColor, out CustomPaint customPaint);
			return getCurrentColorID(forceColor, paintColor, customPaint, paintCyclingTimeScale, 0);
		}

		public static void getPaintColorAndCustomPaintFromPlayer(Player p, out int paintColor, out CustomPaint customPaint) {
			paintColor = -1;
			customPaint = null;

			for(int i = 0; i < p.inventory.Length; i++) {
				if(p.inventory[i].type != ItemID.None && p.inventory[i].stack > 0) {
					if(PaintIDs.itemIds.Contains(p.inventory[i].type)) {
						for(int c = 0; c < PaintIDs.itemIds.Length; c++) {
							if(PaintIDs.itemIds[c] == p.inventory[i].type) {
								paintColor = c;
								return;
							}
						}
					} else if(p.inventory[i].modItem is CustomPaint){
						customPaint = (CustomPaint)p.inventory[i].modItem.Clone();
						return;
					}
				}
			}
		}

		private static void getPaintColorAndCustomPaintFromProjectile(PaintingProjectile projectile, out int paintColor, out CustomPaint customPaint) {
			paintColor = 0;
			customPaint = null;
			if(projectile.currentPaintIndex == -1)
				return;

			Player owner = projectile.getOwner();
			Item item = owner.inventory[projectile.currentPaintIndex];
			ModItem modItem = item.modItem;
			if(modItem is CustomPaint) {
				customPaint = (CustomPaint)modItem;
			} else {
				for(int i = 0; i < PaintIDs.itemIds.Length; i++) {
					if(item.type == PaintIDs.itemIds[i])
						paintColor = (byte)i;
				}
			}
		}

		private static ShaderData getShaderData(byte currentColor, byte? nextColor, bool sprayPainted, float timeScale, float timeOffset = 0) {
			if(currentColor == PaintID.Negative) {
				return GameShaders.Armor.GetShaderFromItemId(ItemID.NegativeDye);
			} else if(sprayPainted) {
				return GameShaders.Misc["SprayPainted"].UseColor(getColor(currentColor, nextColor, timeScale, timeOffset));
			} else {
				return GameShaders.Misc["Painted"].UseColor(getColor(currentColor, nextColor, timeScale, timeOffset));
			}
		}
		private static Color getColor(byte currentColor, byte? nextColor, float timeScale, float timeOffset = 0) {
			Color color = PaintColors.colors[currentColor];
			if(nextColor != null) {
				float lerpAmount = ((Main.GlobalTime - timeOffset) / timeScale) % 1;
				//System.Diagnostics.Debug.Print(lerpAmount.ToString().PadRight(20).Substring(0, 7) + " " + currentColor + " " + nextColor);
				color = Color.Lerp(color, PaintColors.colors[(int)nextColor], lerpAmount);
			}

			return color;
		}
		private static void getRenderVars(int paintColor, CustomPaint customPaint, out byte currentColor, out byte? nextColor, out bool sprayPainted, float timeScale, float timeOffset = 0) {
			getCurrentAndNextColorIDs(paintColor, customPaint, out currentColor, out nextColor, timeScale, timeOffset);
			sprayPainted = (customPaint != null && (customPaint is CustomSprayPaint || customPaint is VanillaSprayPaint));
		}
		private static void getCurrentAndNextColorIDs(int paintColor, CustomPaint customPaint, out byte currentColor, out byte? nextColor, float timeScale, float timeOffset = 0) {
			currentColor = getCurrentColorID(true, paintColor, customPaint, timeScale, timeOffset);
			nextColor = null;

			if(customPaint != null) {
				nextColor = customPaint.getNextColor(timeScale, true, timeOffset);
				if(nextColor == currentColor)
					nextColor = null;
			}
		}
		public static byte getCurrentColorID(bool forceColor, int paintColor,CustomPaint customPaint, float timeScale, float timeOffset = 0) {
			if(customPaint != null) {
				return customPaint.getColor(timeScale, forceColor, timeOffset);
			} else {
				return (byte)paintColor;
			}
		}

		public static void applyPaintedToNPC(NPC npc, int paintColor, CustomPaint customPaint, List<NPC> handledNpcs = null, bool preventRecursion = false) {
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
			NPC[] npcs = Main.npc;
			if(!globalNpc.painted || (customPaint != null && (globalNpc.customPaint == null || globalNpc.customPaint.displayName != customPaint.displayName)) || (paintColor != -1 && globalNpc.paintColor != paintColor))
				globalNpc.paintedTime = Main.GlobalTime;
			globalNpc.painted = true;
			if(customPaint != null) {
				globalNpc.paintColor = -1;
				globalNpc.customPaint = (CustomPaint)customPaint.Clone();
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
									applyPaintedToNPC(Main.npc[i], paintColor, customPaint, null, true);
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
								applyPaintedToNPC(prevSection, paintColor, customPaint, handledNpcs);
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
								applyPaintedToNPC(previSection, paintColor, customPaint, handledNpcs);
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
								applyPaintedToNPC(nextSection, paintColor, customPaint, handledNpcs);
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

	}
}
