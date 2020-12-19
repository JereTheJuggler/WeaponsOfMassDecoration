using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Buffs {
	class Painted : ModBuff {
		public override void SetDefaults() {
			DisplayName.SetDefault("Painted");
			Description.SetDefault("What a Mess!");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.pvpBuff[Type] = true;
			longerExpertDebuff = false;
			canBeCleared = false;
		}

		public override void ModifyBuffTip(ref string tip, ref int rare) {
			//if(GetInstance<WoMDConfig>().paintStatusEffects)
			//	tip += " Various debuffs based on color";
		}

		public override void Update(NPC npc, ref int buffIndex) {
			WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
			gNpc.painted = true;
			/* not sure I actually like this
			if(GetInstance<WoMDConfig>().paintStatusEffects) {
				PaintData data = gNpc.paintData;
				byte paintColor = data.TruePaintColor;
				if(paintColor == 0)
					return;
				switch(paintColor) {
					case PaintID.Red:
					case PaintID.DeepRed:
						//fire
						npc.AddBuff(BuffID.OnFire, 2);
						//buffIndex++;
						break;
					case PaintID.Yellow:
						if(npc.defense <= 0)
							break;
						if(npc.defense <= 5)
							npc.defense = 0;
						else
							npc.defense -= 5;
						break;
					case PaintID.DeepYellow:
						if(npc.defense <= 0)
							break;
						if(npc.defense <= 10)
							npc.defense = 0;
						else
							npc.defense -= 10;
						break;
					case PaintID.Green:
					case PaintID.DeepGreen:
						//poison
						break;
					case PaintID.Purple:
					case PaintID.DeepPurple:
						//venom
						break;
					case PaintID.Cyan:
					case PaintID.DeepCyan:
						//frozen
						npc.AddBuff(BuffID.Frostburn, 2);
						buffIndex++;
						break;
					case PaintID.Black:
						//darkness
						break;
					case PaintID.Shadow:
						//blackout
						break;
					case PaintID.Negative:
						//confused
						npc.AddBuff(BuffID.Confused, 2);
						buffIndex++;
						break;
				}
			}
			*/
		}
	}
}