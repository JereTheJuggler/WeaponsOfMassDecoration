using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace WeaponsOfMassDecoration{
	class WoMDConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(false)]
		[Label("Chaos Mode")]
		[Tooltip("This enables features that will cause more paint to be flung around the world")]
		public bool chaosModeEnabled;
	}
}
