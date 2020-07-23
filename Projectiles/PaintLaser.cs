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
    class PaintLaser : PaintingProjectile {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Beam");
        }
        
        public override void SetDefaults() {
            projectile.width = 6;
            projectile.height = 6;
            projectile.aiStyle = 0;
            //projectile.CloneDefaults(ProjectileID.ShadowBeamFriendly);
            //aiType = ProjectileID.ShadowBeamFriendly;
            projectile.friendly = true;
            projectile.timeLeft = 3;
            projectile.alpha = 255;
            projectile.magic = true;
            light = .5f;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            paintConsumptionChance = .25f;
        }

        public override bool PreAI() {
            return base.PreAI();
        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer) {
                if(projectile.timeLeft == 1) {
                    Vector2 playerPos = Main.player[Main.myPlayer].position;
                    Vector2 myPos = projectile.Center;
                    paintTileAndWall(projectile.Center.ToTileCoordinates());
                    if(projectile.ai[1] > 0)
                        explode(projectile.Center, 16, true, false);
                    Vector2 displacement = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;

					createLight(projectile.Center, light);

                    for(int p = 0; p < 10; p++) {
                        //int dustId = Dust.NewDust(projectile.Center, 0, 0, mod.DustType<Dusts.LightDust>(), 0, 0, 200, getLightColor(), .75f);
                        int dustId = Dust.NewDust(projectile.TopLeft + displacement * (p / 10f) + new Vector2(-3,-3), 7, 7, ModContent.DustType<Dusts.LightDust>(), 0, 0, 200, WeaponsOfMassDecoration.getColor(this), 1f);
						Dust dust = Main.dust[dustId];
						dust.velocity = new Vector2(3, 0).RotatedByRandom(Math.PI * 2);
						dust.fadeIn = 2f;
						//dust.scale = 
						
						//int dustId = Dust.NewDust(projectile.Center + displacement * Main.rand.NextFloat(1f), 0, 0, mod.DustType<Dusts.LightDust>(), 0, 0, 200, getLightColor(), .75f);
                    }
                    //projectile.timeLeft = 0;
                    /*if(Math.Sqrt(Math.Pow(myPos.X - playerPos.X, 2) + Math.Pow(myPos.Y - playerPos.Y, 2)) > 32) {
                        Convert((int)(projectile.position.X + (float)(projectile.width / 2)) / 16, (int)(projectile.position.Y + (float)(projectile.height / 2)) / 16, 0);
                    }*/
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            explode(projectile.Center, 16, true, false);
            return base.OnTileCollide(oldVelocity);
        }

        public void Convert(int i, int j, int size = 4) {
            for(int k = i - size; k <= i + size; k++) {
                for(int l = j - size; l <= j + size; l++) {
                    if(WorldGen.InWorld(k, l, 1) && new Vector2(k - i, l - j).Length() < new Vector2(size * 2, size * 2).Length()) {
                        paintTileAndWall(k, l);
                        //int type = (int)Main.tile[k, l].type;
                        //int wall = (int)Main.tile[k, l].wall;
                        //if(wall != 0) {
                        //    Main.tile[k, l].wall = (ushort)mod.WallType("ExampleWall");
                        //    WorldGen.SquareWallFrame(k, l, true);
                        //    NetMessage.SendTileSquare(-1, k, l, 1);
                        //}
                        //if(TileID.Sets.Conversion.Stone[type]) {
                        //    Main.tile[k, l].type = (ushort)mod.TileType("ExampleBlock");
                        //    WorldGen.SquareTileFrame(k, l, true);
                        //    NetMessage.SendTileSquare(-1, k, l, 1);
                        //} else if(type == TileID.Chairs && Main.tile[k, l - 1].type == TileID.Chairs) {
                        //    Main.tile[k, l].type = (ushort)mod.TileType("ExampleChair");
                        //    Main.tile[k, l - 1].type = (ushort)mod.TileType("ExampleChair");
                        //    WorldGen.SquareTileFrame(k, l, true);
                        //    NetMessage.SendTileSquare(-1, k, l, 1);
                        //} else if(type == TileID.WorkBenches && Main.tile[k - 1, l].type == TileID.WorkBenches) {
                        //    Main.tile[k, l].type = (ushort)mod.TileType("ExampleWorkbench");
                        //    Main.tile[k - 1, l].type = (ushort)mod.TileType("ExampleWorkbench");
                        //    WorldGen.SquareTileFrame(k, l, true);
                        //    NetMessage.SendTileSquare(-1, k, l, 1);
                        //}
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //Redraw the projectile with the color not influenced by light
            createLight(projectile.Center, .5f);
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = (Main.projectileTexture[projectile.type].Height - (2 * (Main.projFrames[projectile.type] - 1))) / Main.projFrames[projectile.type];
            int startY = (frameHeight + 2) * projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            for(int k = 0; k < projectile.oldPos.Length; k++) {
                Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + origin + new Vector2(0f, projectile.gfxOffY);
                Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, sourceRectangle, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
