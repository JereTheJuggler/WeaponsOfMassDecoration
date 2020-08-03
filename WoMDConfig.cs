﻿using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WeaponsOfMassDecoration {
	class WoMDConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(false)]
		[Label("Chaos Mode")]
		[Tooltip("This enables features that will cause more paint to be flung around the world")]
		public bool chaosModeEnabled;
	}
}