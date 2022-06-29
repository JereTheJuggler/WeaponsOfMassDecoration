using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Buffs {
	class Painted : ModBuff {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Painted");
			Description.SetDefault("What a Mess!");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.pvpBuff[Type] = true;
			//longerExpertDebuff = false;
		}

		public override bool RightClick(int buffIndex) {
			return false;
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
					case PaintID.RedPaint:
					case PaintID.DeepRedPaint:
						//fire
						npc.AddBuff(BuffID.OnFire, 2);
						//buffIndex++;
						break;
					case PaintID.YellowPaint:
						if(npc.defense <= 0)
							break;
						if(npc.defense <= 5)
							npc.defense = 0;
						else
							npc.defense -= 5;
						break;
					case PaintID.DeepYellowPaint:
						if(npc.defense <= 0)
							break;
						if(npc.defense <= 10)
							npc.defense = 0;
						else
							npc.defense -= 10;
						break;
					case PaintID.GreenPaint:
					case PaintID.DeepGreenPaint:
						//poison
						break;
					case PaintID.PurplePaint:
					case PaintID.DeepPurplePaint:
						//venom
						break;
					case PaintID.CyanPaint:
					case PaintID.DeepCyanPaint:
						//frozen
						npc.AddBuff(BuffID.Frostburn, 2);
						buffIndex++;
						break;
					case PaintID.BlackPaint:
						//darkness
						break;
					case PaintID.ShadowPaint:
						//blackout
						break;
					case PaintID.NegativePaint:
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