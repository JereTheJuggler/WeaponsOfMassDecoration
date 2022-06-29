using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
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
			Projectile.CloneDefaults(ProjectileID.PaladinsHammerFriendly);
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ignoreWater = true;

			light = .5f;
		}
	}
}
