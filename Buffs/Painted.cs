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
			gNpc.RefreshPainted();
			PaintData data = gNpc.PaintData;
			if (data == null) return;
			byte trueColor = PaintUtils.GetBasePaintID(data.TruePaintColor);
			if (data.buffConfig.IsColorEnabled(trueColor)) {
				switch (trueColor) {
					case PaintID.RedPaint:
						npc.AddBuff(BuffID.OnFire, 2, true);
						break;
					case PaintID.YellowPaint:
						npc.AddBuff(BuffID.Ichor, 2, true);
						break;
					case PaintID.LimePaint:
						npc.AddBuff(BuffID.CursedInferno, 2, true);
						break;
					case PaintID.GreenPaint:
						npc.AddBuff(BuffID.Poisoned, 2, true);
						break;
					case PaintID.CyanPaint:
						npc.AddBuff(BuffID.Frostburn, 2, true);
						break;
					case PaintID.PurplePaint:
						npc.AddBuff(BuffID.Venom, 2, true);
						break;
					case PaintID.PinkPaint:
						npc.AddBuff(BuffType<Confetti>(), 2, true);
						break;
					case PaintID.NegativePaint:
						npc.AddBuff(BuffID.Confused, 2, true);
						break;
				}
			}
		}
	}
}