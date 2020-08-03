using Terraria;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration.Buffs {
	class Painted : ModBuff {
		public override void SetDefaults() {
			DisplayName.SetDefault("Painted");
			Description.SetDefault("What a Mess!");
			Main.buffNoSave[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<NPCs.WoMDNPC>().painted = true;
		}
	}
}