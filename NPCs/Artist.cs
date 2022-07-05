using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.DataStructures;
using System.Collections.Generic;
using ReLogic.Content;
using WeaponsOfMassDecoration.Items;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
	[AutoloadHead]
	internal class Artist : ModNPC {
		public override void SetStaticDefaults() {
			// DisplayName automatically assigned from localization files, but the commented line below is the normal approach.
			DisplayName.SetDefault("Artist");

			Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Velocity = -1f, // Draws the NPC in the bestiary as if its walking -1 tiles in the x direction
				Direction = -1
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPC.Happiness
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Like)
				.SetBiomeAffection<DesertBiome>(AffectionLevel.Hate)
				.SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Love)
				.SetNPCAffection(NPCID.DyeTrader, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Painter, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.TaxCollector, AffectionLevel.Hate)
			;
		}

		public override void SetDefaults() {
			NPC.townNPC = true; // Sets NPC to be a Town NPC
			NPC.friendly = true; // NPC Will not attack player
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;

			AnimationType = NPCID.Guide;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

				// Sets your NPC's flavor text in the bestiary.
				new FlavorTextBestiaryInfoElement("Fed up with the establishment, he likes to paint by his own rules. Where's the fun in painting if you're not throwing brushes at the wall?")//,

				// You can add multiple elements if you really wanted to
				// You can also use localization keys (see Localization/en-US.lang)
				//new FlavorTextBestiaryInfoElement("Mods.ExampleMod.Bestiary.ExamplePerson")
			});
		}

		// The PreDraw hook is useful for drawing things before our sprite is drawn or running code before the sprite is drawn
		// Returning false will allow you to manually draw your NPC
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// This code slowly rotates the NPC in the bestiary
			// (simply checking NPC.IsABestiaryIconDummy and incrementing NPC.Rotation won't work here as it gets overridden by drawModifiers.Rotation each tick)
			if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers)) {
				drawModifiers.Rotation += 0.001f;

				// Replace the existing NPCBestiaryDrawModifiers with our new one with an adjusted rotation
				NPCID.Sets.NPCBestiaryDrawOffset.Remove(Type);
				NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
			}

			return true;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs, int money) { // Requirements for the town NPC to spawn.
			int painter = NPC.FindFirstNPC(NPCID.Painter);
			return painter >= 0;
			/*for (int k = 0; k < 255; k++) {
				Player player = Main.player[k];
				if (!player.active) {
					continue;
				}
				// Player has to have either an ExampleItem or an ExampleBlock in order for the NPC to spawn
				if (player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>() || item.type == ModContent.ItemType<Items.Placeable.ExampleBlock>())) {
					return true;
				}
			}*/

			//return false;
		}

		public override ITownNPCProfile TownNPCProfile() {
			return new ArtistProfile();
		}

		public override List<string> SetNPCNameList() {
			return new List<string> {
				"Andy",
				"Salvador",
				"Vincent",
				"Pablo",
				"Jackson",
				"Johannes"
			};
		}
		public override string GetChat() {
			WeightedRandom<string> chat = new();

			int cyborg = NPC.FindFirstNPC(NPCID.Cyborg);
			if (cyborg >= 0 && Main.rand.NextBool(4))
				chat.Add(Main.npc[cyborg].GivenName+" keeps telling me about these digital art things called NFTs. It just sounds like a scam to me.");
			
			int demo = NPC.FindFirstNPC(NPCID.Demolitionist);
			if(demo >= 0 && Main.rand.NextBool(4))
				chat.Add("I spoke with " + Main.npc[demo].GivenName + " earlier. It inspired me to make something that I'm sure will blow you away!");
			
			chat.Add("What's your favorite color? Mine? Oh, I love them all!");
			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
			button = Language.GetTextValue("LegacyInterface.28");
			/*button2 = "Awesomeify";
			if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack)) {
				button = "Upgrade " + Lang.GetItemNameValue(ItemID.HiveBackpack);
			}*/
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
			shop = true;
		}

		public override void SetupShop(Chest shop, ref int nextSlot) {
			shop.item[nextSlot].SetDefaults(ItemType<ThrowingPaintbrush>());
			nextSlot++;

			shop.item[nextSlot].SetDefaults(ItemType<PaintArrow>());
			nextSlot++;

			int armsDealer = NPC.FindFirstNPC(NPCID.ArmsDealer);
			if(armsDealer >= 0) {
				shop.item[nextSlot].SetDefaults(ItemType<Paintball>());
				nextSlot++;
			}

			shop.item[nextSlot].SetDefaults(ItemType<ThrowingPaintbrush>());
			nextSlot++;

			shop.item[nextSlot].SetDefaults(ItemType<PaintShuriken>());
			nextSlot++;

			int demo = NPC.FindFirstNPC(NPCID.Demolitionist);
			if (demo >= 0) {
				shop.item[nextSlot].SetDefaults(ItemType<PaintBomb>());
				nextSlot++;

				shop.item[nextSlot].SetDefaults(ItemType<PaintDynamite>());
				nextSlot++;
			}

			Player p = Main.player[Main.myPlayer];
			if(p.inventory.Any(item => item.type == ItemID.Clentaminator)) {
				shop.item[nextSlot].SetDefaults(ItemType<PaintSolution>());
				nextSlot++;
			}

			shop.item[nextSlot].SetDefaults(ItemType<BuffAccBase>());
			nextSlot++;
			shop.item[nextSlot].SetDefaults(ItemType<RewardsProgram>());
			nextSlot++;

			bool rewardsProgram = false;

			WoMDPlayer player = GetModPlayer(Main.myPlayer);
			if (player != null)
				rewardsProgram = player.accRewardsProgram;
			if (rewardsProgram) {
				for (int i = 0; i < shop.item.Length; i++) {
					Item item = shop.item[i];
					if (!item.active)
						continue;
					RewardsProgram.ModifyPrice(ref item);
				}
			}
		}

		public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		/*
		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ProjectileType<Projectiles.ThrowingPaintbrush>();
			attackDelay = 1;
		}
		*/

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 2f;
		}
	}

	public class ArtistProfile : ITownNPCProfile {
		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn)
				return Request<Texture2D>("WeaponsOfMassDecoration/NPCs/Artist");

			if (npc.altTexture == 1)
				return Request<Texture2D>("WeaponsOfMassDecoration/NPCs/Artist_Alt_1");

			return Request<Texture2D>("WeaponsOfMassDecoration/NPCs/Artist");
		}

		public int GetHeadTextureIndex(NPC npc) => GetModHeadSlot("WeaponsOfMassDecoration/NPCs/Artist_Head");
	}
}
