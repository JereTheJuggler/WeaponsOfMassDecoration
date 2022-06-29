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
using static Mono.Cecil.Cil.OpCodes;

namespace WeaponsOfMassDecoration {

	/// <summary>
	/// Contains constants used to specify a message type of a ModPacket. One of these values should always be the first thing written when sending a ModPacket
	/// </summary>
	public static class WoMDMessageTypes {
		/// <summary>
		/// Specifies that the packet is for syncing variables related to an enemies painted buff
		/// </summary>
		public const byte SetNPCColors = 1;
		/// <summary>
		/// Specifies that the packet is for syncing the color of WoMDProjectile objects
		/// </summary>
		public const byte SetProjectileColor = 2;
		/// <summary>
		/// Specifies that the packet is for syncing the npcOwner property of PaintingProjectile objects
		/// </summary>
		public const byte SetProjNPCOwner = 3;
		/// <summary>
		/// Specifies that the packet is for syncing the npcOwner property of multiple PaintingProjectile objects
		/// </summary>
		public const byte SetMultiProjNPCOwner = 4;
		/// <summary>
		/// Specifies that the packet is for syncing the _overridePaintData property of a PaintingProjectile
		/// </summary>
		public const byte SetPPOverrideData = 5;
	}

	public class WeaponsOfMassDecoration : Mod {
		public const float PI = 3.14159265f;

		public const int paintedBuffDuration = 60 * 60; //1 minute

		protected static Dictionary<string, Texture2D> extraTextures;

		public WeaponsOfMassDecoration() {
			ContentAutoloadingEnabled = true;
			GoreAutoloadingEnabled = true;
			MusicAutoloadingEnabled = true;
			BackgroundAutoloadingEnabled = true;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			switch(reader.ReadByte()) {
				case WoMDMessageTypes.SetNPCColors:
					if(Multiplayer() || Server()) {
						WoMDNPC.ReadColorPacket(reader, out WoMDNPC gNpc, out NPC npc);
						if(Server()) {
							WoMDNPC.SendColorPacket(gNpc, npc);
						}
					}
					break;
				case WoMDMessageTypes.SetProjectileColor:
					if(Multiplayer())
						WoMDProjectile.ReadProjectileColorPacket(reader, out _, out _);
					break;
				case WoMDMessageTypes.SetProjNPCOwner:
					if(Multiplayer())
						PaintingProjectile.ReadProjNPCOwnerPacket(reader);
					break;
				case WoMDMessageTypes.SetMultiProjNPCOwner:
					if(Multiplayer())
						PaintingProjectile.ReadMultiProjNPCOwnerPacket(reader);
					break;
				case WoMDMessageTypes.SetPPOverrideData:
					if(Multiplayer() || Server()) {
						PaintingProjectile.ReadPPOverrideDataPacket(reader, out PaintingProjectile proj);
						if(Server() && proj != null) {
							PaintingProjectile.SendPPOverrideDataPacket(proj);
						}
					}
					break;
			}
		}

		#region loading/unloading/textures
		public override void Load() {
			if(Main.netMode != NetmodeID.Server) {
				#region load shaders
				Ref<Effect> paintedRef = new Ref<Effect>(ModContent.Request<Effect>("WeaponsOfMassDecoration/Effects/Painted", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["Painted"] = new MiscShaderData(paintedRef, "paintedColor").UseColor(1f, 0, 0).UseOpacity(1f);

				Ref<Effect> gsPaintedRef = new Ref<Effect>(ModContent.Request<Effect>("WeaponsOfMassDecoration/Effects/GreenScreenPainted", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["GSPainted"] = new MiscShaderData(gsPaintedRef, "gsPaintedColor").UseColor(1f, 0, 0).UseOpacity(1f);

				Ref<Effect> paintedNegativeRef = new Ref<Effect>(ModContent.Request<Effect>("WeaponsOfMassDecoration/Effects/PaintedNegative", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["PaintedNegative"] = new MiscShaderData(paintedNegativeRef, "paintedNegativeColor");

				Ref<Effect> sprayPaintedRef = new Ref<Effect>(ModContent.Request<Effect>("WeaponsOfMassDecoration/Effects/SprayPainted", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["SprayPainted"] = new MiscShaderData(sprayPaintedRef, "sprayPaintedColor").UseImage0("Images/Misc/noise");
				#endregion

				#region load extra textures
				extraTextures = new Dictionary<string, Texture2D>();

				LoadExtraTexture("Items/PaintbrushPainted");
				LoadExtraTexture("Items/SpectrePaintbrushPainted");
				LoadExtraTexture("Items/PaintRollerPainted");
				LoadExtraTexture("Items/SpectrePaintRollerPainted");
				LoadExtraTexture("Items/PaintingMultiToolPainted");
				LoadExtraTexture("Items/SpectrePaintingMultiToolPainted");

				LoadExtraTexture("Items/PaintArrowScraper");
				LoadExtraTexture("Items/PaintArrowPainted");

				LoadExtraTexture("Items/PaintShurikenPainted");

				LoadExtraTexture("Items/PaintDynamitePainted");

				LoadExtraTexture("Items/ThrowingPaintbrushPainted");
				LoadExtraTexture("Items/ThrowingPaintbrushScraper");

				LoadExtraTexture("Items/PaintBombPainted");

				LoadExtraTexture("Items/PaintBoomerangPainted");
				LoadExtraTexture("Items/PaintBoomerangScraper");

				LoadExtraTexture("Items/PaintSolutionPainted");
				LoadExtraTexture("Items/InfinitePaintSolutionPainted");

				LoadExtraTexture("Items/PaintballPainted");
				#endregion

				IL.Terraria.Player.PlaceThing += HookPlaceThing;
			}
		}
		public override void Unload() {
			extraTextures = null;
			IL.Terraria.Player.PlaceThing -= HookPlaceThing;
			base.Unload();
		}

		//The following 3 things are used for an IL edit that disables the vanilla painting handled in Player.PlaceThing
		private void HookPlaceThing(ILContext il) {
			var c = new ILCursor(il);

			var label = il.DefineLabel();

			c.Emit(Ldarg_0);
			c.Emit(OpCodes.Call, ((shouldInterruptPlaceThingDelegate)shouldInterruptPlaceThing).Method);
			c.Emit(Brfalse_S, label);

			c.Emit(Ret);

			c.MarkLabel(label);
		}
		private delegate bool shouldInterruptPlaceThingDelegate(object player);
		private static bool shouldInterruptPlaceThing(object player) {
			Player p = player as Player;
			if(p == null)
				return false;
			switch(p.inventory[p.selectedItem].type) {
				case ItemID.Paintbrush:
				case ItemID.SpectrePaintbrush:
				case ItemID.PaintRoller:
				case ItemID.SpectrePaintRoller:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Used to get an additional texture that was loaded with the mod
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Texture2D GetExtraTexture(string name) {
			if(extraTextures.ContainsKey(name))
				return extraTextures[name];
			return null;
		}
		/// <summary>
		/// Loads an additional texture that won't be found with autoload. Uses the filename as the key in the dictionary
		/// </summary>
		/// <param name="filename">The file location + name for the texture. Extension should not be provided</param>
		private static void LoadExtraTexture(string filename) {
			LoadExtraTexture(filename, filename.Substring(filename.LastIndexOf("/") + 1));
		}
		/// <summary>
		/// Loads an additional texture that won't be found with autoload. Uses a provided name as the key in the dictionary
		/// </summary>
		/// <param name="filename">The file location + name for the texture. Extension should not be provided</param>
		/// <param name="name">The key to use for this texture's dictionary entry</param>
		private static void LoadExtraTexture(string filename, string name) {
			try {
				Texture2D texture = ModContent.Request<Texture2D>("WeaponsOfMassDecoration/"+filename).Value;
				extraTextures.Add(name, texture);
			} catch {
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
		#endregion

		#region data retrieval
		/// <summary>
		/// Safely gets a Dust object from Main.dust. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the dust in Main.dust</param>
		/// <returns></returns>
		public static Dust GetDust(int index) {
			if(index < 0 || index >= Main.dust.Length - 1)
				return null;
			return Main.dust[index];
		}

		/// <summary>
		/// Safely gets a Projectile object from Main.projectile. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the projectile in Main.projectile</param>
		/// <returns></returns>
		public static Projectile GetProjectile(int index) {
			if(index < 0 || index >= Main.projectile.Length - 1)
				return null;
			return Main.projectile[index];
		}

		/// <summary>
		/// Safely gets a WoMDProjectile object from Main.projectile. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the projectile in Main.projectile</param>
		/// <returns></returns>
		public static WoMDProjectile GetGlobalProjectile(int index) {
			if(index < 0 || index >= Main.projectile.Length - 1)
				return null;
			Projectile p = Main.projectile[index];
			if(p == null)
				return null;
			return p.GetGlobalProjectile<WoMDProjectile>();
		}

		/// <summary>
		/// Safely gets a Player object from Main.player. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the player in Main.player</param>
		/// <returns></returns>
		public static Player GetPlayer(int index) {
			if(index < 0 || index >= Main.player.Length - 1)
				return null;
			return Main.player[index];
		}

		/// <summary>
		/// Safely gets a WoMDPlayer object from Main.player. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the player in Main.player</param>
		/// <returns></returns>
		public static WoMDPlayer GetModPlayer(int index) {
			if(index < 0 || index >= Main.player.Length - 1)
				return null;
			Player p = Main.player[index];
			if(p == null)
				return null;
			return p.GetModPlayer<WoMDPlayer>();
		}

		/// <summary>
		/// Safely gets a NPC object from Main.npc. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the npc in Main.npc</param>
		/// <returns></returns>
		public static NPC GetNPC(int index) {
			if(index < 0 || index >= Main.npc.Length - 1)
				return null;
			return Main.npc[index];
		}
		
		/// <summary>
		/// Safely gets a WoMDNPC object from Main.npc. If the provided index is out of range, null will be returned
		/// </summary>
		/// <param name="index">The index of the npc in Main.npc</param>
		/// <returns></returns>
		public static WoMDNPC GetGlobalNPC(int index) {
			if(index < 0 || index >= Main.npc.Length - 1)
				return null;
			NPC npc = Main.npc[index];
			if(npc == null)
				return null;
			return npc.GetGlobalNPC<WoMDNPC>();
		}

		/// <summary>
		/// Gets an instance of WoMDWorld
		/// </summary>
		/// <returns></returns>
		public static WoMDWorld GetWorld() => ModContent.GetInstance<WoMDWorld>();

		/// <summary>
		/// Returns whether or not Chaos Mode is enabled
		/// </summary>
		/// <returns></returns>
		public static bool ChaosMode() => ModContent.GetInstance<WoMDConfig>().chaosModeEnabled;

		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.SinglePlayer
		/// </summary>
		/// <returns></returns>
		public static bool SinglePlayer() => Main.netMode == NetmodeID.SinglePlayer;
		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.MultiplayerClient
		/// </summary>
		/// <returns></returns>
		public static bool Multiplayer() => Main.netMode == NetmodeID.MultiplayerClient;
		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.Server
		/// </summary>
		/// <returns></returns>
		public static bool Server() => Main.netMode == NetmodeID.Server;

		/// <summary>
		/// Checks if the provided item is either a vanilla paint, or a CustomPaint
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPaint(Item item) {
			if(item == null)
				return false;
			if(!item.active)
				return false;
			if(item.ModItem is CustomPaint)
				return true;
			if(item.paint > 0)
				return true;
			return false;
		}
		/// <summary>
		/// Checks if the provided item is either a vanilla painting tool, or a PaintingMultiTool
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPaintingTool(Item item) {
			if(item == null)
				return false;
			if(!item.active)
				return false;
			if(item.ModItem is PaintingMultiTool)
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
		/// <summary>
		/// Checks if the provided item is a PaintingItem
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPaintingItem(Item item) {
			if(item == null)
				return false;
			if(!item.active)
				return false;
			if(item.ModItem is PaintingItem)
				return true;
			return false;
		}
		/// <summary>
		/// Gets the name for a type of painting tool based on a provided paint method
		/// </summary>
		/// <param name="paintMethod"></param>
		/// <returns></returns>
		public static string GetPaintToolName(PaintMethods paintMethod) {
			switch(paintMethod) {
				case PaintMethods.Blocks: return "Paintbrush";
				case PaintMethods.Walls: return "Paint Roller";
				case PaintMethods.BlocksAndWalls: return "Painting Multi-Tool";
				case PaintMethods.RemovePaint: return "Paint Scraper";
			}
			return "None";
		}
		

		/// <summary>
		/// Gets the name for a type of paint, provided a paintColor and customPaint
		/// </summary>
		/// <param name="paintColor">Specifies a value from PaintID. -1 for custom paints</param>
		/// <param name="customPaint">Specifies an instance of CustomPaint to use. null for vanilla paints</param>
		/// <returns></returns>
		public static string GetPaintColorName(PaintData paintData) {
			//TODO: Make this get the display names from the vanilla paints instead of hardcoded values. Maybe add in translations for custom paints
			if(paintData.PaintColor == -1 && paintData.CustomPaint == null)
				return "None";
			if(paintData.CustomPaint != null) {
				return paintData.CustomPaint.displayName;
			} else {
				if(paintData.PaintColor < ColorNames.list.Length)
					return ColorNames.list[paintData.PaintColor];
			}
			return "None";
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
		public static void ApplyPaintedToNPC(NPC npc, PaintData data, List<NPC> handledNpcs = null, bool preventRecursion = false) {
			switch(npc.type) {
				//cultist fight
				case NPCID.CultistDragonBody1:
				case NPCID.CultistDragonBody2:
				case NPCID.CultistDragonBody3:
				case NPCID.CultistDragonBody4:
				case NPCID.CultistDragonHead:
				case NPCID.CultistDragonTail:
				case NPCID.CultistBossClone:
				case NPCID.CultistBoss:
				case NPCID.AncientCultistSquidhead:
				//destroyer
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				//pillars
				case NPCID.LunarTowerNebula:
				case NPCID.NebulaBeast:
				case NPCID.LunarTowerSolar:
				case NPCID.LunarTowerStardust:
				case NPCID.StardustWormTail:
				case NPCID.StardustWormBody:
				case NPCID.StardustWormHead:
				case NPCID.LunarTowerVortex:
				//martian saucer
				case NPCID.MartianSaucer:
				case NPCID.MartianSaucerCannon:
				case NPCID.MartianSaucerCore:
				case NPCID.MartianSaucerTurret:
				//misc bosses
				case NPCID.Pumpking:
				case NPCID.PumpkingBlade:
				//misc random mobs
				case NPCID.DungeonSpirit:
				case NPCID.Tumbleweed:
				case NPCID.DesertDjinn:
				case NPCID.Ghost:
				case NPCID.Poltergeist:
					return;
			}
			if(preventRecursion)
				return;

			npc.AddBuff(ModContent.BuffType<Painted>(), paintedBuffDuration);

			WoMDNPC globalNpc = npc.GetGlobalNPC<WoMDNPC>();

			if(data.CustomPaint != null)
				data.CustomPaint.modifyPaintDataForNpc(ref data);

			if(globalNpc.painted) {
				PaintData existingData = globalNpc.PaintData;
				if(existingData.PaintColor == data.PaintColor &&
				   (existingData.CustomPaint == null) == (data.CustomPaint == null) && //either both or neither are null
				   existingData.CustomPaint.GetType().Equals(data.CustomPaint.GetType()) &&
				   existingData.sprayPaint == data.sprayPaint)
					return; //nothing needs to be updated
			}

			globalNpc.SetPaintData(npc, data);

			if(SinglePlayer()) {
				//TODO: do this whole section better
				if(handledNpcs == null)
					handledNpcs = new List<NPC>();
				handledNpcs.Add(npc);
				switch(npc.type) {
					case NPCID.MoonLordHead:
					case NPCID.MoonLordHand:
					case NPCID.MoonLordCore:
					case NPCID.MoonLordLeechBlob:
						for(int i = 0; i < Main.npc.Length; i++) {
							if(Main.npc[i].TypeName == "")
								break;
							switch(Main.npc[i].type) {
								case NPCID.MoonLordHead:
								case NPCID.MoonLordHand:
								case NPCID.MoonLordCore:
									ApplyPaintedToNPC(Main.npc[i], data, null, true);
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
						if(npc.ai[0] == Math.Round(npc.ai[0])) {
							NPC prevSection = GetNPC((int)npc.ai[0]);
							if(prevSection != null && !handledNpcs.Contains(prevSection)) {
								if(prevSection.ai[1] == Math.Round(prevSection.ai[1])) {
									NPC thisSection = GetNPC((int)prevSection.ai[1]);
									if(thisSection != null && thisSection.Equals(npc)) {
										ApplyPaintedToNPC(prevSection, data, handledNpcs);
									}
								}
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
						if(npc.ai[0] == Math.Round(npc.ai[0])) {
							NPC prevSection = GetNPC((int)npc.ai[0]);
							if(prevSection != null && !handledNpcs.Contains(prevSection)) {
								if(prevSection.ai[1] == Math.Round(prevSection.ai[1])) {
									NPC thisSection = GetNPC((int)prevSection.ai[1]);
									if(thisSection != null && thisSection.Equals(npc)) {
										ApplyPaintedToNPC(prevSection, data, handledNpcs);
									}
								}
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
						if(npc.ai[1] == Math.Round(npc.ai[1])) {
							NPC nextSection = GetNPC((int)npc.ai[1]);
							if(nextSection != null && !handledNpcs.Contains(nextSection)) {
								if(nextSection.ai[0] == Math.Round(nextSection.ai[0])) {
									NPC thisSection = GetNPC((int)nextSection.ai[0]);
									if(thisSection != null && thisSection.Equals(npc)) {
										ApplyPaintedToNPC(nextSection, data, handledNpcs);
									}
								}
							}
						}
						break;
				}
			}
		}

		//Hamstar's Mod Helpers Integration
		public static string GithubUserName => "JereTheJuggler";
		public static string GithubProjectName => "WeaponsOfMassDecoration";
	}
}