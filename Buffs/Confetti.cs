using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Buffs {
	internal class Confetti : ModBuff {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Painted");
			Description.SetDefault("What a Mess!");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.pvpBuff[Type] = false;
		}
		public override bool RightClick(int buffIndex) {
			return false;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
			gNpc.buffConfetti = true;
		}
	}
}
