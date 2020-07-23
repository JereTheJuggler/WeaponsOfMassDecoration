using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponsOfMassDecoration.Projectiles {
	class DamageProjectile : ModProjectile {
		public override void SetDefaults() {
			projectile.width = 6;
			projectile.height = 6;
			projectile.aiStyle = 0;
			projectile.friendly = true;
			projectile.timeLeft = 5;
			projectile.alpha = 255;
			projectile.magic = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			base.ModifyDamageHitbox(ref hitbox);
		}
	}
}
