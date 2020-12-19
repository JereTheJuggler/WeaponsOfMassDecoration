using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Projectiles {
	class PaintHammer : PaintingProjectile {

		public PaintHammer() : base() {
			explodesOnDeath = true;
			explosionRadius = 80f;

			usesGSShader = true;

			yFrameCount = 2;
			xFrameCount = 1;
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paint Hammer");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			projectile.CloneDefaults(ProjectileID.PaladinsHammerFriendly);
			projectile.width = 20;
			projectile.height = 20;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.melee = true;
			projectile.ignoreWater = true;

			light = .5f;
		}
	}
}
