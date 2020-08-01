using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Constants;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;

namespace WeaponsOfMassDecoration {
	/// <summary>
	/// The different methods that can be used for painting
	/// </summary>
	public enum PaintMethods{
		/// <summary>
		/// The method when the player has no painting tools in their inventory
		/// </summary>
		None,
		/// <summary>
		/// The method used by paint scrapers
		/// </summary>
		RemovePaint,
		/// <summary>
		/// The method used by paintbrushes
		/// </summary>
		Blocks,
		/// <summary>
		/// The method used by paint rollers
		/// </summary>
		Walls,
		/// <summary>
		/// The method used by painting multi-tools
		/// </summary>
		BlocksAndWalls
	}

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
	}

	public class WeaponsOfMassDecoration : Mod {
		/// <summary>
		/// The speed that custom paints cycle through colors for painting tiles. Also applies to the colors for projectile shaders to line up with the color they are painting.
		/// </summary>
		public const float paintCyclingTimeScale = .25f;

		/// <summary>
		/// The speed that custom paints cycle through colors for npc shaders
		/// </summary>
		public const float npcCyclingTimeScale = 1f;

		protected static Dictionary<string, Texture2D> extraTextures;
		protected static Dictionary<byte, string> paintNames;

		public WeaponsOfMassDecoration() {
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			switch(reader.ReadByte()) {
				case WoMDMessageTypes.SetNPCColors:
					WoMDNPC.readColorPacket(reader, out WoMDNPC gNpc, out NPC npc);
					if(server()) {
						WoMDNPC.sendColorPacket(gNpc, npc);
					}
					break;
				case WoMDMessageTypes.SetProjectileColor:
					if(multiplayer())
						WoMDProjectile.readProjectileColorPacket(reader, out _, out _);
					break;
				case WoMDMessageTypes.SetProjNPCOwner:
					if(multiplayer())
						PaintingProjectile.readProjNPCOwnerPacket(reader);
					break;
			}
		}

	#region loading/unloading/textures
		public override void Load() {
			if(Main.netMode != NetmodeID.Server) {
				#region load shaders
				Ref<Effect> paintedRef = new Ref<Effect>(GetEffect("Effects/Painted"));
				GameShaders.Misc["Painted"] = new MiscShaderData(paintedRef, "paintedColor").UseColor(1f, 0, 0).UseOpacity(1f);

				Ref<Effect> gsPaintedRef = new Ref<Effect>(GetEffect("Effects/GreenScreenPainted"));
				GameShaders.Misc["GSPainted"] = new MiscShaderData(gsPaintedRef, "gsPaintedColor").UseColor(1f, 0, 0).UseOpacity(1f);

				Ref<Effect> paintedNegativeRef = new Ref<Effect>(GetEffect("Effects/PaintedNegative"));
				GameShaders.Misc["PaintedNegative"] = new MiscShaderData(paintedNegativeRef, "paintedNegativeColor");

				Ref<Effect> sprayPaintedRef = new Ref<Effect>(GetEffect("Effects/SprayPainted"));
				GameShaders.Misc["SprayPainted"] = new MiscShaderData(sprayPaintedRef, "sprayPaintedColor").UseImage("Images/Misc/noise");
				#endregion

				#region load paint items
				paintNames = new Dictionary<byte, string> {
					{ 1 , "Red" },
					{ 2 , "Orange" },
					{ 3 , "Yellow" },
					{ 4 , "Lime" },
					{ 5 , "Green" },
					{6,"Teal" },
					{7,"Cyan" },
					{8,"Sky Blue" },
					{9,"Blue" },
					{10,"Purple" },
					{11,"Violet" },
					{12,"Pink" },
					{13,"Deep Red" },
					{14,"Deep Orange" },
					{15,"Deep Yellow" },
					{16,"Deep Lime" },
					{17,"Deep Green" },
					{18,"Deep Teal" },
					{19,"Deep Cyan" },
					{20,"Deep Sky Blue" },
					{21,"Deep Blue" },
					{22,"Deep Purple" },
					{23,"Deep Violet" },
					{24,"Deep Pink" },
					{25,"Black" },
					{26,"White" },
					{27,"Gray" },
					{28,"Brown" },
					{29,"Shadow" },
					{30,"Negative" }
				};
				#endregion

				#region load extra textures
				extraTextures = new Dictionary<string, Texture2D>();

				loadExtraTexture("Items/PaintbrushPainted");
				loadExtraTexture("Items/SpectrePaintbrushPainted");
				loadExtraTexture("Items/PaintRollerPainted");
				loadExtraTexture("Items/SpectrePaintRollerPainted");
				loadExtraTexture("Items/PaintingMultiToolPainted");
				loadExtraTexture("Items/SpectrePaintingMultiToolPainted");

				loadExtraTexture("Items/PaintArrowScraper");
				loadExtraTexture("Items/PaintArrowPainted");

				loadExtraTexture("Items/PaintShurikenPainted");

				loadExtraTexture("Items/PaintDynamitePainted");

				loadExtraTexture("Items/ThrowingPaintbrushPainted");
				loadExtraTexture("Items/ThrowingPaintbrushScraper");

				loadExtraTexture("Items/PaintBombPainted");

				loadExtraTexture("Items/PaintBoomerangPainted");
				loadExtraTexture("Items/PaintBoomerangScraper");

				loadExtraTexture("Items/PaintSolutionPainted");
				loadExtraTexture("Items/InfinitePaintSolutionPainted");

				loadExtraTexture("Items/PaintballPainted");
				#endregion

				IL.Terraria.Player.PlaceThing += HookPlaceThing;
			}
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
		public static Texture2D getExtraTexture(string name) {
			if(extraTextures.ContainsKey(name))
				return extraTextures[name];
			return null;
		}
		/// <summary>
		/// Loads an additional texture that won't be found with autoload. Uses the filename as the key in the dictionary
		/// </summary>
		/// <param name="filename">The file location + name for the texture. Extension should not be provided</param>
		private void loadExtraTexture(string filename) {
			loadExtraTexture(filename, filename.Substring(filename.LastIndexOf("/") + 1));
		}
		/// <summary>
		/// Loads an additional texture that won't be found with autoload. Uses a provided name as the key in the dictionary
		/// </summary>
		/// <param name="filename">The file location + name for the texture. Extension should not be provided</param>
		/// <param name="name">The key to use for this texture's dictionary entry</param>
		private void loadExtraTexture(string filename,string name) {
			try {
				Texture2D texture = GetTexture(filename);
				extraTextures.Add(name, texture);
			} catch {
			}
		}

		public override void Unload() {
			extraTextures = null;
			paintNames = null;
			base.Unload();
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
	
		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.SinglePlayer
		/// </summary>
		/// <returns></returns>
		public static bool singlePlayer() => Main.netMode == NetmodeID.SinglePlayer;
		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.MultiplayerClient
		/// </summary>
		/// <returns></returns>
		public static bool multiplayer() => Main.netMode == NetmodeID.MultiplayerClient;
		/// <summary>
		/// A shortcut for Main.netMode == NetmodeID.Server
		/// </summary>
		/// <returns></returns>
		public static bool server() => Main.netMode == NetmodeID.Server;

		/// <summary>
		/// Checks if the provided item is either a vanilla paint, or a CustomPaint
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool isPaint(Item item) {
			if(item.modItem is CustomPaint)
				return true;
			if(PaintItemID.list.Contains(item.type))
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
		/// <summary>
		/// Gets the name for a type of painting tool based on a provided paint method
		/// </summary>
		/// <param name="paintMethod"></param>
		/// <returns></returns>
		public static string getPaintToolName(PaintMethods paintMethod) {
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
		public static string getPaintColorName(int paintColor, CustomPaint customPaint) {
			if(paintColor == -1 && customPaint == null)
				return "None";
			if(customPaint != null) {
				return customPaint.displayName;
			} else {
				if(paintNames.ContainsKey((byte)paintColor)) {
					return paintNames[(byte)paintColor]+" Paint";
				}
			}
			return "None";
		}
	#endregion

	#region shaders
		/// <summary>
		/// Applies a shader for the provided WoMDProjectile. Possible results are the Painted and PaintedNegative shaders.
		/// </summary>
		/// <param name="gProjectile"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(WoMDProjectile gProjectile) {
			MiscShaderData shader = getShader(gProjectile);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided WoMDNPC. Possible results are the Painted, SprayPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="drawData">This is necessary for the SprayPainted shader to work</param>
		/// <returns></returns>
		public static MiscShaderData applyShader(WoMDNPC globalNpc, DrawData? drawData = null) {
			MiscShaderData shader = getShader(globalNpc, drawData);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided PaintingItem. Possible results are the GSPainted and PaintedNegative shader.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(PaintingItem item, out WoMDPlayer player) {
			MiscShaderData shader = getShader(item, out player);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(PaintingProjectile projectile) {
			MiscShaderData shader = getShader(projectile);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Gets a shader for the provided WoMDProjectile. Possible results are the Painted and PaintedNegative shaders.
		/// </summary>
		/// <param name="gProjectile"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDProjectile gProjectile) {
			if(!gProjectile.painted)
				return null;
			if(gProjectile.paintColor == PaintID.Negative)
				return getNegativeShader();
			Color color = getColor(gProjectile.paintColor, gProjectile.customPaint, npcCyclingTimeScale, gProjectile.paintedTime, null);
			return getPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDNPC. Possible results are the Painted, SprayPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="drawData">This is necessary for the SprayPainted shader to work</param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDNPC globalNpc, DrawData? drawData = null) {
			if(!globalNpc.painted)
				return null;
			if(globalNpc.paintColor == PaintID.Negative)
				return getNegativeShader();
			Color color = getColor(globalNpc.paintColor, globalNpc.customPaint, npcCyclingTimeScale, globalNpc.paintedTime, null);
			if(globalNpc.sprayPainted)
				return getSprayPaintedShader(color, drawData);
			return getPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingItem. Possible results are the GSPainted and PaintedNegative shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(PaintingItem item, out WoMDPlayer player) {
			player = null;
			Player p = item.getOwner();
			if(p == null)
				return null;
			player = p.GetModPlayer<WoMDPlayer>();
			if(player == null)
				return null;
			if(player.paintColor == PaintID.Negative)
				return getNegativeShader();
			return getGSShader(player.renderColor);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(PaintingProjectile projectile) {
			int paintColor;
			CustomPaint customPaint;
			float timeScale;
			float timeOffset = 0;
			Color renderColor = default;
			if(projectile.npcOwner == -1) {
				WoMDPlayer player = projectile.getModPlayer();
				if(player == null)
					return null;
				paintColor = player.paintColor;
				customPaint = player.customPaint;
				timeScale = paintCyclingTimeScale;
				if(player.paintMethod == PaintMethods.RemovePaint)
					renderColor = PaintColors.list[0];
				else
					renderColor = player.renderColor;
			} else {
				NPC npc = getNPC(projectile.npcOwner);
				if(npc == null)
					return null;
				WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
				if(gNpc == null)
					return null;
				gNpc.getPaintVars(out paintColor, out customPaint);
				timeScale = npcCyclingTimeScale;
				timeOffset = gNpc.paintedTime;
			}
			if(paintColor == -1 && customPaint == null)
				return null;
			if(paintColor == PaintID.Negative) {
				if(projectile.usesGSShader)
					return null;
				return getNegativeShader();
			}
			if(projectile.npcOwner != -1) {
				renderColor = getColor(paintColor, customPaint, timeScale, timeOffset);
			}
			if(projectile.usesGSShader) 
				return getGSShader(renderColor);
			return getPaintedShader(renderColor);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDItem. Possible results are the GSPainted and NegativePainted shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDItem item, WoMDPlayer player) {
			if(player.paintColor == PaintID.Negative)
				return getNegativeShader();
			return getGSShader(player.renderColor);
		}
		/// <summary>
		/// Gets the data for the PaintedNegative shader
		/// </summary>
		/// <returns></returns>
		private static MiscShaderData getNegativeShader() {
			MiscShaderData data = GameShaders.Misc["PaintedNegative"];
			return data;
		}
		/// <summary>
		/// Gets the data for the Painted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData getPaintedShader(Color color) {
			MiscShaderData data = GameShaders.Misc["Painted"].UseColor(color).UseOpacity(1f);
			return data;
		}
		/// <summary>
		/// Gets the data for the SprayPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <param name="drawData"></param>
		/// <returns></returns>
		private static MiscShaderData getSprayPaintedShader(Color color, DrawData? drawData = null) {
			MiscShaderData data = GameShaders.Misc["SprayPainted"].UseColor(color).UseImage("Images/Misc/noise").UseOpacity(1f);
			return data;
		}
		/// <summary>
		/// Gets the data for the GSPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData getGSShader(Color color) {
			MiscShaderData data = GameShaders.Misc["GSPainted"].UseColor(color).UseOpacity(1f);
			return data;
		}

		/// <summary>
		/// Gets the color for the provided PaintingProjectile, based on properties from the projectile's Player or NPC owner
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static Color getColor(PaintingProjectile projectile) {
			int paintColor;
			CustomPaint customPaint;
			float timeScale;
			float timeOffset = 0;
			if(projectile.npcOwner == -1) {
				WoMDPlayer player = projectile.getModPlayer();
				if(player == null)
					return PaintColors.list[0];
				if(player.paintMethod == PaintMethods.RemovePaint)
					return PaintColors.list[0];
				return player.renderColor;
			} else {
				NPC npc = getNPC(projectile.npcOwner);
				if(npc == null)
					return Color.White;
				WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
				if(gNpc == null)
					return Color.White;
				gNpc.getPaintVars(out paintColor, out customPaint);
				timeScale = npcCyclingTimeScale;
				timeOffset = gNpc.paintedTime;
			}
			return getColor(paintColor, customPaint, timeScale, timeOffset, projectile.getOwner());
		}

		private static Color getColor(int paintColor, CustomPaint customPaint, float timeScale, float timeOffset = 0, Player player = null) {
			if(paintColor == -1 && customPaint == null)
				return Color.White;
			if(customPaint == null)
				return PaintColors.list[paintColor];
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

			WoMDNPC globalNpc = npc.GetGlobalNPC<WoMDNPC>();

			if(customPaint != null)
				customPaint.getPaintVarsForNpc(out paintColor,out customPaint, data);

			float paintedTime;

			globalNpc.painted = true;

			if(!globalNpc.painted || (customPaint != null && (globalNpc.customPaint == null || globalNpc.customPaint.displayName != customPaint.displayName)) || (paintColor != -1 && globalNpc.paintColor != paintColor))
				paintedTime = Main.GlobalTime;
			else
				paintedTime = globalNpc.paintedTime;

			if(customPaint != null) {
				globalNpc.setColors(npc, -1, (CustomPaint)customPaint.Clone(), customPaint is ISprayPaint, paintedTime);
			} else {
				globalNpc.setColors(npc, paintColor, null, false, paintedTime);
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
						if(npc.ai[0] == Math.Round(npc.ai[0])) {
							NPC prevSection = getNPC((int)npc.ai[0]);
							if(prevSection != null && !handledNpcs.Contains(prevSection)) {
								if(prevSection.ai[1] == Math.Round(prevSection.ai[1])) {
									NPC thisSection = getNPC((int)prevSection.ai[1]);
									if(thisSection != null && thisSection.Equals(npc)) {
										applyPaintedToNPC(prevSection, paintColor, customPaint, data, handledNpcs);
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
							NPC prevSection = getNPC((int)npc.ai[0]);
							if(prevSection != null && !handledNpcs.Contains(prevSection)) {
								if(prevSection.ai[1] == Math.Round(prevSection.ai[1])) {
									NPC thisSection = getNPC((int)prevSection.ai[1]);
									if(thisSection != null && thisSection.Equals(npc)) {
										applyPaintedToNPC(prevSection, paintColor, customPaint, data, handledNpcs);
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
							NPC nextSection = getNPC((int)npc.ai[1]);
							if(nextSection != null && !handledNpcs.Contains(nextSection)) {
								if(nextSection.ai[0] == Math.Round(nextSection.ai[0])) {
									NPC thisSection = getNPC((int)nextSection.ai[0]);
									if(thisSection != null && thisSection.Equals(npc)) {
										applyPaintedToNPC(nextSection, paintColor, customPaint, data, handledNpcs);
									}
								}
							}
						}
						break;
				}
			}
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

	#region painting
		/// <summary>
		/// Paints tiles between the 2 provided world coordinates. Should not be used for projectiles controlled by the player, as paint will not be consumed
		/// </summary>
		/// <param name="start">The starting position of the line to paint. Expects values in world coordinates</param>
		/// <param name="end">The ending position of the line to paint. Expects values in world coordinates</param>
		/// <param name="paintColor">The PaintID to use for vanilla paints. -1 for custom paints</param>
		/// <param name="customPaint">An instance of CustomPaint to use for painting. null for vanilla paints</param>
		/// <param name="data">An instance of CustomPaintData to use for determining what color custom paints will output</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>The number of tiles that were updated</returns>
		public int paintBetweenPoints(Vector2 start, Vector2 end, int paintColor, CustomPaint customPaint, CustomPaintData data, PaintMethods method = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			if(!(blocksAllowed || wallsAllowed))
				return 0;
			Vector2 unitVector = end - start;
			float distance = unitVector.Length();
			unitVector.Normalize();
			int iterations = (int)Math.Ceiling(distance / 8f);
			int count = 0;
			for(int i = 0; i < iterations; i++) {
				if(paint(start + (unitVector * i * 8), paintColor, customPaint, data, method, blocksAllowed, wallsAllowed, useWorldGen)) 
					count++;
			}
			return count;
		}

		/// <summary>
		/// Creates a circle of paint. Should not be used for projectiles controlled by the player, as paint will not be consumed
		/// </summary>
		/// <param name="pos">The position of the center of the circle. Expects values in world coordinates</param>
		/// <param name="radius">The radiues of the circle. 16 for each tile</param>
		/// <param name="paintColor">The PaintID to use for vanilla paints. -1 for custom paints</param>
		/// <param name="customPaint">An instance of CustomPaint to use for painting. null for vanilla paints</param>
		/// <param name="data">An instance of CustomPaintData to use for determining what color custom paints will output</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>The number of tiles that were updated</returns>
		public int explode(Vector2 pos, float radius, int paintColor, CustomPaint customPaint, CustomPaintData data, PaintMethods method = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			int count = 0;
			for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
				if(currentLevel == 0) {
					if(paint(pos, paintColor, customPaint, data, method, blocksAllowed, wallsAllowed))
						count++;
				} else {
					for(int i = 0; i <= currentLevel * 2; i++) {
						float xOffset;
						float yOffset;
						if(i <= currentLevel) {
							xOffset = currentLevel;
							yOffset = i;
						} else {
							xOffset = (currentLevel * 2 - i + 1);
							yOffset = (currentLevel + 1);
						}
						Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
						if(offsetVector.Length() <= radius) {
							for(int dir = 0; dir < 4; dir++) {
								if(paint(pos + offsetVector.RotatedBy(dir * (Math.PI / 2)), paintColor, customPaint, data, method, blocksAllowed, wallsAllowed, useWorldGen))
									count++;
							}
						}
					}
				}
			}
			return count;
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="pos">The position of the tile. Expects values in world coordinates</param>
		/// <param name="paintColor">The PaintID to use for vanilla paints. -1 for custom paints</param>
		/// <param name="customPaint">An instance of CustomPaint to use for painting. null for vanilla paints</param>
		/// <param name="data">An instance of CustomPaintData to use for determining what color custom paints will output</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(Vector2 pos, int paintColor, CustomPaint customPaint, CustomPaintData data, PaintMethods method = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			Point p = pos.ToTileCoordinates();
			return paint(p.X, p.Y, paintColor, customPaint, data, method, blocksAllowed, wallsAllowed, useWorldGen);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="paintColor">The PaintID to use for vanilla paints. -1 for custom paints</param>
		/// <param name="customPaint">An instance of CustomPaint to use for painting. null for vanilla paints</param>
		/// <param name="data">An instance of CustomPaintData to use for determining what color custom paints will output</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(int x, int y, int paintColor, CustomPaint customPaint, CustomPaintData data, PaintMethods method = PaintMethods.BlocksAndWalls,bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			if(!WorldGen.InWorld(x, y, 10))
				return false;
			if(paintColor == -1 && customPaint == null)
				return false;
			byte targetColor;
			if(customPaint != null) {
				targetColor = customPaint.getPaintID(data);
			} else {
				targetColor = (byte)paintColor;
			}
			return paint(x, y, targetColor, method, blocksAllowed, wallsAllowed, useWorldGen);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="pos">The position of the tile. Expects values in world coordinates</param>
		/// <param name="color">The PaintID of the color to use</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(Vector2 pos, byte color, PaintMethods method, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			Point p = pos.ToTileCoordinates();
			return paint(p.X, p.Y, color, method, blocksAllowed, wallsAllowed, useWorldGen);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="color">The PaintID of the color to use</param>
		/// <param name="method">The painting method to use</param>
		/// <param name="blocksAllowed">Can be set to false to prevent painting walls regardless of paint method</param>
		/// <param name="wallsAllowed">Can be set to false to prevent painting tiles regardless of paint method</param>
		/// <param name="useWorldGen">Can be set to true to use WorldGen.paintTile and WorldGen.paintWall instead of modifying the tile directly. Using WorldGen causes additional visuals to be created when changing a tile's color</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(int x, int y, byte color, PaintMethods method, bool blocksAllowed = true, bool wallsAllowed = true, bool useWorldGen = false) {
			if(!WorldGen.InWorld(x, y, 10))
				return false;
			Tile t = Main.tile[x, y];
			if(t == null)
				return false;
			bool updated = false;
			if(blocksAllowed && t.active() && t.color() != color && (color != 0 || method == PaintMethods.RemovePaint)) {
				if(useWorldGen)
					WorldGen.paintTile(x, y, color, false);
				else
					t.color(color);
				updated = true;
			}
			if(wallsAllowed && t.wall > 0 && t.wallColor() != color && (color != 0 || method == PaintMethods.RemovePaint)) {
				if(useWorldGen)
					WorldGen.paintWall(x, y, color, false);
				else 
					t.wallColor(color);
				updated = true;
			}
			if(updated) {
				if(server())
					sendTileFrame(x, y);
			}
			return updated;
		}

		/// <summary>
		/// Sends a net message to update the tile at the given position
		/// </summary>
		/// <param name="x">The tile's x coordinate. Expects values in tile coordinates</param>
		/// <param name="y">The tile's y coordinate. Expects values in tile coordinates</param>
		public static void sendTileFrame(int x, int y) {
			NetMessage.SendTileSquare(-1, x, y, 1);
		}
	#endregion
	}
}
