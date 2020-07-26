using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
    class PaintDynamite : PaintingProjectile {
		public PaintDynamite() : base() {
			explosionRadius = 10;

            manualRotation = true;
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Dynamite");     //The English name of the projectile
        }

        public override void SetDefaults() {
            base.SetDefaults();
            projectile.width = 8;               //The width of projectile hitbox
            projectile.height = 8;              //The height of projectile hitbox
            projectile.aiStyle = 1;             //The ai style of the projectile, please reference the source code of Terraria
            projectile.friendly = true;         //Can the projectile deal damage to enemies?
            projectile.hostile = false;         //Can the projectile deal damage to the player?
            projectile.penetrate = -1;           //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            projectile.timeLeft = 420;          //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            projectile.alpha = 0;             //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in)
            projectile.ignoreWater = true;          //Does the projectile's speed be influenced by water?
            projectile.tileCollide = true;          //Can the projectile collide with tiles?
            projectile.extraUpdates = 1;            //Set to above 0 if you want the projectile to update multiple time in a frame
            aiType = ProjectileID.Dynamite;
            paintConsumptionChance = .5f;
            projectile.gfxOffY = 19;
            projectile.light = .5f;
        }

        public override bool PreAI() {
            base.PreAI();
            if(projectile.velocity.Y == 0 && Math.Abs(projectile.velocity.X) > 0)
                projectile.velocity.X *= .98f;
            rotation += projectile.velocity.X * .05f;
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
            projectile.soundDelay = 20;
            if(projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
                projectile.velocity.X = oldVelocity.X * -.4f;
            if(projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
                projectile.velocity.Y = oldVelocity.Y * -.35f;
            projectile.velocity.X *= .9f;
            return false;
        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer && projectile.timeLeft <= 3) {
                projectile.tileCollide = false;
                projectile.alpha = 255;
                projectile.position.X = projectile.position.X + (projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (projectile.height / 2);
                projectile.width = (int)Math.Round(32 * explosionRadius);
                projectile.height = (int)Math.Round(32 * explosionRadius);
                projectile.position.X = projectile.position.X - (projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (projectile.height / 2);
                projectile.damage = 120;
            } else {
                if(Main.rand.Next(2) == 0) {
                    Dust smoke = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f));
                    if(smoke != null) {
                        smoke.scale = .1f + Main.rand.Next(5) * .1f;
                        smoke.fadeIn = 1.5f + Main.rand.Next(5) * .1f;
                        smoke.noGravity = true;
                        smoke.position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2 - 12)).RotatedBy(rotation, default) * 1.1f;
                    }
                    Dust fire = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Fire, 0f, 0f, 100, default, 1f));
                    if(fire != null) {
                        fire.scale = 1f + Main.rand.Next(5) * .1f;
                        fire.noGravity = true;
                        fire.position = projectile.Center + new Vector2(0f, (-(float)projectile.height / 2 - 12)).RotatedBy(rotation, default) * 1.1f;
                    }
                }
            }
            base.AI();
        }

        public override void Kill(int timeLeft) {
            Main.PlaySound(SoundID.Item14, projectile.position);
            //smoke dust
            for(int i = 0; i < 15; i++) {
                Dust smoke = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f));
                if(smoke != null)
                smoke.velocity *= 1.4f;
            }
            //fire dust
            for(int i = 0; i < 30; i++) {
                Dust dust = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 3, 0f, 0f, 100, default, 1f));
                if(dust != null) {
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                }
                Dust fire = getDust(Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Fire, 0f, 0f, 100, default, 2f));
                if(fire != null)
                    fire.velocity *= 2f;
            }
            //reset size
            projectile.position.X = projectile.position.X + (projectile.width / 2);
            projectile.position.Y = projectile.position.Y + (projectile.height / 2);
            projectile.width = 18;
            projectile.height = 18;
            projectile.position.X = projectile.position.X - (projectile.width / 2);
            projectile.position.Y = projectile.position.Y - (projectile.height / 2);

            if(canPaint()) {
                explode(projectile.Center, explosionRadius*16, true, true);
            }
        }
    }
}
