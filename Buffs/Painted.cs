using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Buffs {
    class Painted : ModBuff{
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
