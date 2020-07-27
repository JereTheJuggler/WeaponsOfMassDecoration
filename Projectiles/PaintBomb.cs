using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintBomb : PaintingProjectile{
        public static int bounceRadius = 25;

		public PaintBomb() : base() {
			explodesOnDeath = true;
			explosionRadius = 80;

            manualRotation = true;
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Bomb");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 18;
            projectile.height = 18;
            projectile.aiStyle = 1;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.penetrate = 2;
            projectile.timeLeft = 300;
            projectile.alpha = 0;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.extraUpdates = 1;
            aiType = ProjectileID.BouncyBomb;
            projectile.gfxOffY = 19;
            projectile.light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            rotation += (projectile.velocity.X < 0 ? -.1f : .1f);
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Main.expertMode) {
                if(target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                    damage /= 5;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.ai[1] != 0)
                return true;
            if(projectile.soundDelay == 0) {
                Main.PlaySound(SoundID.Dig);
            }
            projectile.soundDelay = 10;
            if(projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
                projectile.velocity.X = oldVelocity.X * -.75f;
            if(projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
                projectile.velocity.Y = oldVelocity.Y * -.75f;
            if(projectile.penetrate == 1) {
                return true;
            }

            explode(projectile.Center, bounceRadius, true, true);

            projectile.penetrate--;
            return false;
        }

        public override void AI() {
            if(Main.rand.Next(2) == 0) {
                Dust d = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default, 1f));
                if(d != null) {
                    d.scale = .1f + Main.rand.Next(5) * .1f;
                    d.fadeIn = 1.5f + Main.rand.Next(5) * .1f;
                    d.noGravity = true;
                    d.position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2)).RotatedBy(rotation, default) * 1.1f;
                }
                d = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default, 1f));
                if(d != null){
                    d.scale = 1f + Main.rand.Next(5) * .1f;
                    d.noGravity = true;
                    d.position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2 - 6)).RotatedBy(rotation, default) * 1.1f;
                }
            }
            base.AI();
        }

		public override bool PreKill(int timeLeft) {
			int projectileId = Projectile.NewProjectile(
				projectile.Center - new Vector2(explosionRadius / 2, explosionRadius / 2),
				new Vector2(0,0),
				ProjectileType<DamageProjectile>(),
				projectile.damage,
				projectile.knockBack,
				projectile.owner
			);
            //doesn't do damage to mobs for whatever reason
			Projectile damageArea = Main.projectile[projectileId];
			damageArea.CloneDefaults(ProjectileID.Grenade);
			damageArea.Size = new Vector2(explosionRadius, explosionRadius);
			damageArea.position = projectile.Center - new Vector2(explosionRadius / 2, explosionRadius / 2);
			damageArea.timeLeft = 5;
			damageArea.damage = projectile.damage;
			ModProjectile p = damageArea.modProjectile;
			p.aiType = 0;
			return base.PreKill(timeLeft);
		}

		public override void Kill(int timeLeft) {
            Main.PlaySound(SoundID.Item14, projectile.Center);
            //smoke dust
            for(int i = 0; i < 15; i++) {
                Dust smoke = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default, 2f));
                if(smoke != null)
                    smoke.velocity *= 1.4f;
            }
            //fire dust
            for(int i = 0; i < 30; i++) {
                Dust fire = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 3, 0f, 0f, 100, default, 1f));
                if(fire != null) {
                    fire.noGravity = true;
                    fire.velocity *= 2f;
                }
                fire = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default, 2f));
                if(fire != null)
                    fire.velocity *= 2f;
            }
			base.Kill(timeLeft);
        }
    }
}
