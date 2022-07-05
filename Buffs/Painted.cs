using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
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
						npc.AddBuff(BuffAccRed.DebuffID, 2, true);
						break;
					case PaintID.YellowPaint:
						npc.AddBuff(BuffAccYellow.DebuffID, 2, true);
						break;
					case PaintID.LimePaint:
						npc.AddBuff(BuffAccLime.DebuffID, 2, true);
						break;
					case PaintID.GreenPaint:
						npc.AddBuff(BuffAccGreen.DebuffID, 2, true);
						break;
					case PaintID.CyanPaint:
						npc.AddBuff(BuffAccCyan.DebuffID, 2, true);
						break;
					case PaintID.PurplePaint:
						npc.AddBuff(BuffAccPurple.DebuffID, 2, true);
						break;
					case PaintID.PinkPaint:
						npc.AddBuff(BuffAccPink.DebuffID, 2, true);
						break;
					case PaintID.NegativePaint:
						npc.AddBuff(BuffAccNegative.DebuffID, 2, true);
						break;
				}
			}
		}
	}
}