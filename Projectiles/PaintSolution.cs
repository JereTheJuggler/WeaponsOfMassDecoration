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
    class PaintSolution : PaintingProjectile{

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Spray Paint");
        }

        public override void SetDefaults() {
            projectile.width = 6;
            projectile.height = 6;
            projectile.aiStyle = 41;
            aiType = ProjectileID.PureSpray;
            projectile.friendly = true;
            projectile.timeLeft = 50;
            projectile.alpha = 255;
            light = .5f;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            paintConsumptionChance = .25f;

        }

        public override void AI() {
            if(projectile.owner == Main.myPlayer) {
                Vector2 playerPos = Main.player[Main.myPlayer].position;
                Vector2 myPos = projectile.Center;
                if(Math.Sqrt(Math.Pow(myPos.X-playerPos.X,2)+Math.Pow(myPos.Y-playerPos.Y,2)) > 32){
                    Convert((int)(projectile.position.X + (float)(projectile.width / 2)) / 16, (int)(projectile.position.Y + (float)(projectile.height / 2)) / 16, 2);
                }
            }
            int dustType = DustID.Smoke;
            if(projectile.timeLeft > 100) {
                projectile.timeLeft = 100;
            }
            if(projectile.ai[0] > 7f) {
                float dustScale = 1f;
                if(projectile.ai[0] == 8f) {
                    dustScale = 0.2f;
                } else if(projectile.ai[0] == 9f) {
                    dustScale = 0.4f;
                } else if(projectile.ai[0] == 10f) {
                    dustScale = 0.6f;
                } else if(projectile.ai[0] == 11f) {
                    dustScale = 0.8f;
                }
                projectile.ai[0] += 1f;
                for(int i = 0; i < 1; i++) {
                    int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 1f);
                    Dust dust = Main.dust[dustIndex];
                    dust.noGravity = true;
                    dust.scale *= 1.75f;
                    dust.velocity.X = dust.velocity.X * 2f;
                    dust.velocity.Y = dust.velocity.Y * 2f;
                    dust.scale *= dustScale;
                    dust.color = WeaponsOfMassDecoration.getColor(this);
                }
            } else {
                projectile.ai[0] += 1f;
            }
            projectile.rotation += 0.3f * (float)projectile.direction;
        }

        public void Convert(int i, int j, int size = 4) {
            for(int k = i - size; k <= i + size; k++) {
                for(int l = j - size; l <= j + size; l++) {
                    if(WorldGen.InWorld(k, l, 1) && new Vector2(k-i,l-j).Length() < new Vector2(size*2, size * 2).Length()) {
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
            createLight(projectile.Center,.5f);
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
