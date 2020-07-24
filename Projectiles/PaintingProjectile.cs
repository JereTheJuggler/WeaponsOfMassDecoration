using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Items;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using System.Xml.Schema;

namespace WeaponsOfMassDecoration.Projectiles {
    

    public abstract class PaintingProjectile : ModProjectile {

		public bool explodesOnDeath = false;
		public float explosionRadius = 5f;

		public bool dropsOnDeath = false;
		public int dropCount = 5;
		public float dropCone = (float)(Math.PI / 2f);

		public bool usesShader = false;

        public int color = -2;
        public int numFrames = 31;
        public PaintMethods paintMethod = PaintMethods.NotSet;
        public int currentPaintIndex = -1;
        public List<Point> paintedTiles = new List<Point>();
        public double lastUpdateCheck = 0;
        public Vector2 startPosition = new Vector2(0,0);
        public float paintConsumptionChance = 1f;
        public float light = 0;
        public int animationFrames = 1;
		
		protected bool resetBatchInPost = false;

		public PaintingProjectile() : base() { }

        public override bool PreAI() {
            //if(projectile.owner == Main.myPlayer) {
                if(color == -2 || paintMethod == PaintMethods.NotSet || Main.GlobalTime - lastUpdateCheck >= .25) {
                    assignPaintColorAndMethod();
                    lastUpdateCheck = Main.GlobalTime;
                }
                if(startPosition.X == 0 && startPosition.Y == 0)
                    startPosition = projectile.position;
			//}
			if(!usesShader) {
				projectile.frame = WeaponsOfMassDecoration.getCurrentColorID(this, true);
			}

			//projectile.position = projectile.position + (new Vector2(1, 0).RotatedBy(Math.PI / 2) * (maxDistanceFromCenter * Math.Sin(time)));

			return true;
        }
		
		public Player getOwner() {
            return Main.player[projectile.owner];
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			WoMDGlobalNPC npc = target.GetGlobalNPC<WoMDGlobalNPC>();
			if(npc != null) {
				Player p = getOwner();
				if(currentPaintIndex >= 0 && p != null) {
					if(currentPaintIndex >= 0) {
						if(p.inventory[currentPaintIndex].modItem is CustomPaint) {
							WeaponsOfMassDecoration.applyPaintedToNPC(target, -1, (CustomPaint)p.inventory[currentPaintIndex].modItem);
						} else {
							for(int i = 0; i < PaintIDs.itemIds.Length; i++) {
								if(p.inventory[currentPaintIndex].type == PaintIDs.itemIds[i]) {
									WeaponsOfMassDecoration.applyPaintedToNPC(target, (byte)i, null);
									break;
								}
							}
						}
					}
				} else {
					npc.customPaint = null;
					npc.paintColor = -1;
					npc.paintedTime = 0;
				}
			}
			if(projectile.penetrate == 1)
				onKillOnNPC(target);
        }

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			assignPaintColorAndMethod();
			if(currentPaintIndex < 0 || paintMethod == PaintMethods.RemovePaint)
				damage = (int)Math.Round(damage * .5f);
		}

		public virtual void onKillOnNPC(NPC target) {

		}
		
        public override void OnHitPvp(Player target, int damage, bool crit) {
            base.OnHitPvp(target, damage, crit);
            target.AddBuff(BuffType<Buffs.Painted>(), 600);
            Player p = getOwner();
            if(currentPaintIndex >= 0 && p != null) {
                if(currentPaintIndex >= 0) {
                    if(p.inventory[currentPaintIndex].modItem is CustomPaint) {
                        //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = -1;
                        //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = (Items.CustomPaint)p.inventory[currentPaintIndex].modItem.Clone();
                    }
                    for(int i = 0; i < PaintIDs.itemIds.Length; i++) {
                        if(p.inventory[currentPaintIndex].type == PaintIDs.itemIds[i]) {
                            //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = (byte)i;
                            //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = null;
                            break;
                        }
                    }
                }
            } else {
                //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).customPaint = null;
                //target.GetGlobalNPC<NPCs.WoMDGlobalNPC>(mod).paintColor = -1;
            }
        }

        public void paintAlongOldVelocity(Vector2 oldVelocity,bool blocks = true,bool walls = true) {
            if(!(blocks || walls))
                return;
            if(oldVelocity.Length() > 0 && !(startPosition.X == 0 && startPosition.Y == 0) && (startPosition-projectile.position).Length() > oldVelocity.Length()) {
                Vector2 unitVector = new Vector2(oldVelocity.X, oldVelocity.Y);
                unitVector.Normalize();
                for(int o = 0; o < Math.Ceiling(oldVelocity.Length()); o += 8) {
                    Vector2 currentOffset = projectile.Center - oldVelocity + (unitVector * o);
                    if(blocks && walls)
                        paintTileAndWall(currentOffset);
                    else if(blocks)
                        paintTile(currentOffset);
                    else if(walls)
                        paintWall(currentOffset);
                }
            }
        }
        public void paintBetweenPoints(Vector2 start, Vector2 end, bool blocks = true, bool walls = true) {
            if(!(blocks || walls))
                return;
            Vector2 unitVector = end - start;
            float distance = unitVector.Length();
            unitVector.Normalize();
            int iterations = (int)Math.Ceiling(distance / 8f);
            for(int i=0; i < iterations; i++) {
                Vector2 offset = start + (unitVector * i * 8);
                if(blocks && walls) {
                    paintTileAndWall(offset);
				} else if(blocks) {
                    paintTile(offset);
				} else {
                    paintWall(offset);
				}
			}

		}

        public Point convertPositionToTile(Vector2 position) {
            return new Point((int)Math.Floor(position.X / 16f), (int)Math.Floor(position.Y / 16f));
        }
        public Point convertPositionToTile(Point position) {
            return new Point((int)Math.Floor(position.X / 16f), (int)Math.Floor(position.Y / 16f));
        }

        public void assignPaintColorAndMethod() {
            try {
                Player p = getOwner();
                color = -2;
                paintMethod = PaintMethods.NotSet;
                for(int i = 0; i <= p.inventory.Length && (color == -2 || paintMethod == PaintMethods.NotSet); i++) {
                    if(i == p.inventory.Length) {
                        paintMethod = PaintMethods.None;
                        color = -1;
                    } else {
                        if(p.inventory[i].type != ItemID.None && p.inventory[i].stack > 0) {
                            if(color == -2) {
                                if(PaintIDs.itemIds.Contains(p.inventory[i].type)) {
                                    for(int c = 0; c < PaintIDs.itemIds.Length; c++) {
                                        if(PaintIDs.itemIds[c] == p.inventory[i].type) {
                                            color = c;
                                            break;
                                        }
                                    }
                                } else if(p.inventory[i].modItem is CustomPaint) {
                                    color = PaintIDs.Custom;
                                }
                                if(color != -2)
                                    currentPaintIndex = i;
                            }
                            if(paintMethod == PaintMethods.NotSet) {
                                if(p.inventory[i].type == ItemID.Paintbrush || p.inventory[i].type == ItemID.SpectrePaintbrush) {
                                    paintMethod = PaintMethods.Tiles;
                                }else if(p.inventory[i].type == ItemID.PaintRoller || p.inventory[i].type == ItemID.SpectrePaintRoller) {
                                    paintMethod = PaintMethods.Walls;
                                }else if(p.inventory[i].type == ItemID.PaintScraper || p.inventory[i].type == ItemID.SpectrePaintScraper) {
                                    paintMethod = PaintMethods.RemovePaint;
                                    color = 0;
                                }else if(p.inventory[i].type == ModContent.ItemType<PaintingMultiTool>() || p.inventory[i].type == ModContent.ItemType<SpectrePaintingMultiTool>()) {
                                    paintMethod = PaintMethods.TilesAndWalls;
                                }
                            }
                        }
                    }
                }
                if(color == -2) {
                    color = -1;
                    currentPaintIndex = -1;
                }
                if(paintMethod == PaintMethods.NotSet)
                    paintMethod = PaintMethods.None;
                if(paintMethod == PaintMethods.RemovePaint) {
                    currentPaintIndex = -1;
                }
            } catch {
                color = -1;
                paintMethod = PaintMethods.None;
            }
        }
		
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
            int startY = frameHeight * projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Main.spriteBatch.Draw(texture,
                projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY),
                sourceRectangle, projectile.GetAlpha(lightColor), projectile.rotation, origin,projectile.scale,SpriteEffects.None,0f);
            return false;
        }

		public override bool PreKill(int timeLeft) {
			if(dropsOnDeath)
				createDrops(dropCount, projectile.Center, projectile.velocity * -1, dropCone, 5f);
			if(explodesOnDeath)
				explode(projectile.Center, explosionRadius);
			return base.PreKill(timeLeft);
		}

		public void createDrops(int count,Vector2 position, Vector2 direction, float spreadAngle, float speed = 1f, int timeLeft = 30) {
			direction.Normalize();
			for(int i = 0; i < count; i++) {
				Vector2 vel = direction.RotatedBy((spreadAngle * Main.rand.NextFloat()) - (spreadAngle / 2));
				int projId = Projectile.NewProjectile(position + vel * 2f, vel * speed, ProjectileType<PaintSplatter>(), 0, 0, projectile.owner, 1, ProjectileID.IchorSplash);
				Main.projectile[projId].timeLeft = timeLeft;
				Main.projectile[projId].alpha = 125;
			}
		}
		
        public void explode(Vector2 pos,float radius,bool blocks = true,bool walls = true) {
            for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
                if(currentLevel == 0) {
                    if(blocks && walls)
                        paintTileAndWall(pos);
                    else if(blocks)
                        paintTile(pos);
                    else if(walls)
                        paintWall(pos);
                }else {
                    for(int i = 0; i <= currentLevel * 2; i++) {
                        float xOffset = 0;// (float)Math.Cos(dir * (Math.PI / 2f)) * currentLevel - (float)Math.Sin(dir * (Math.PI / 2f)) * (i <= currentLevel + 1 ? 0 : i - currentLevel - 1);
                        float yOffset = 0;// (i <= currentLevel ? i : currentLevel + 1) * (float)Math.Sin((float)dir * (Math.PI / 2f));
                        if(i <= currentLevel) {
                            xOffset = currentLevel;// * (float)Math.Cos(dir * (Math.PI / 2f));
                            yOffset = i;// * (float)Math.Sin(dir * (Math.PI / 2f));
                        }else {
                            xOffset = (currentLevel*2 - i + 1);// * (float)Math.Cos(dir * (Math.PI / 2f));
                            yOffset = (currentLevel + 1);// * (float)Math.Sin(dir * (Math.PI / 2f));
                        }
                        Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
                        if(offsetVector.Length() <= radius) {
                            for(int dir = 0; dir < 4; dir++) {
                                //Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f).RotatedBy(dir * (Math.PI / 2));
                                Vector2 currentPos = pos + offsetVector.RotatedBy(dir * (Math.PI/2));
                                if(blocks && walls)
                                    paintTileAndWall(currentPos);
                                else if(blocks)
                                    paintTile(currentPos);
                                else if(walls)
                                    paintWall(currentPos);
                            }
                        }
                    }
                }
            }
        }

        public void splatter(Vector2 pos,float radius,int spokes = 5,bool blocks = true,bool walls = true) {
            explode(pos, 48f, blocks, walls);
            float angle = Main.rand.NextFloat((float)Math.PI);
            float[] angles = new float[spokes];
            float[] radii = new float[spokes];
            for(int s = 0; s < spokes; s++) {
                angles[s] = angle;
                angle += Main.rand.NextFloat((float)Math.PI/6,(float)(Math.PI*2)/3);
                radii[s] = radius - (Main.rand.NextFloat(4) * 8);
            }
            for(int offset = 0;offset < radius; offset+=8) {
                for(int s = 0; s < spokes; s++) {
                    if(offset <= radii[s]) {
                        Point newPos = new Point(
                            (int)Math.Round(pos.X + Math.Cos(angles[s]) * offset),
                            (int)Math.Round(pos.Y + Math.Sin(angles[s]) * offset)
                        );
                        Point tilePoint = convertPositionToTile(newPos);
                        if(blocks && walls) {
                            paintTileAndWall(tilePoint);
                        } else if(blocks) {
                            paintTile(tilePoint);
                        } else if(walls) {
                            paintWall(tilePoint);
                        }
                    }
                }
            }
        }

        public void paintTileAndWall(Vector2 p) {
            paintTileAndWall(convertPositionToTile(p));
        }
        public void paintTile(Vector2 p) {
            paintTile(convertPositionToTile(p));
        }
        public void paintWall(Vector2 p) {
            paintTile(convertPositionToTile(p));
        }

        public void paintTileAndWall(int c = -1) {
            paintTileAndWall(convertPositionToTile(projectile.Center));
        }
        public void paintTile(int c = -1) {
            paintTile(convertPositionToTile(projectile.Center));
        }
        public void paintWall(int c = -1) {
            paintWall(convertPositionToTile(projectile.Center));
        }

        public void paintTileAndWall(Point p) { paintTileAndWall(p.X, p.Y); }
        public void paintTile(Point p) { paintTile(p.X, p.Y); }
        public void paintWall(Point p) { paintWall(p.X, p.Y); }

        public void paintTileAndWall(int x,int y) {
            if(projectile.owner == Main.myPlayer) {
                if(!paintedTiles.Contains(new Point(x, y))) {
                    bool updated = false;
                    assignPaintColorAndMethod();
                    if((paintMethod == PaintMethods.RemovePaint || paintMethod == PaintMethods.Tiles || paintMethod == PaintMethods.TilesAndWalls)
                            && color >= 0 && x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY) {
                        byte trueColor = WeaponsOfMassDecoration.getCurrentColorID(this, false);
                        if(trueColor == 0 && paintMethod != PaintMethods.RemovePaint) {
                            updated = true;
                        } else {
                            if(Main.tile[x, y].color() != trueColor) {
                                if(WorldGen.paintTile(x, y, trueColor)) {
                                    updated = true;
                                }
                            }
                        }
                    }
                    if(currentPaintIndex != -1 && getOwner().inventory[currentPaintIndex].stack <= 0)
                        assignPaintColorAndMethod();
                    if((paintMethod == PaintMethods.RemovePaint || paintMethod == PaintMethods.Walls || paintMethod == PaintMethods.TilesAndWalls)
                            && color >= 0 && x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY) {
                        byte trueColor = WeaponsOfMassDecoration.getCurrentColorID(this, false);
                        if(trueColor == 0 && paintMethod != PaintMethods.RemovePaint) {
                            updated = true;
                        } else {
                            if(Main.tile[x, y].wallColor() != trueColor) {
                                if(WorldGen.paintWall(x, y, trueColor)) {
                                    updated = true;
                                }
                            }
                        }
                    }
                    paintedTiles.Add(new Point(x, y));
                    if(updated) {
                        if(currentPaintIndex != -1 && shouldConsumePaint() && color != 0)
                            getOwner().inventory[currentPaintIndex].stack--;
                        sendTileFrame(x, y);
                    }
                }
            }
        }
        public void paintTile(int x,int y) {
            if(projectile.owner == Main.myPlayer) {
                if(!paintedTiles.Contains(new Point(x, y))) {
                    assignPaintColorAndMethod();
                    if((paintMethod == PaintMethods.RemovePaint || paintMethod == PaintMethods.Tiles || paintMethod == PaintMethods.TilesAndWalls)
                            && color >= 0 && x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY) {
                        byte trueColor = WeaponsOfMassDecoration.getCurrentColorID(this, false);
                        if(trueColor != 0 || paintMethod == PaintMethods.RemovePaint) {
                            if(Main.tile[x, y].color() != trueColor) {
                                if(WorldGen.paintTile(x, y, trueColor)) {
                                    sendTileFrame(x, y);
                                    if(currentPaintIndex != -1 && shouldConsumePaint())
                                        getOwner().inventory[currentPaintIndex].stack--;
                                }
                            }
                        }
                    }
                    paintedTiles.Add(new Point(x, y));
                }
            }
        }
        public void paintWall(int x, int y) {
            if(projectile.owner == Main.myPlayer) {
                if(!paintedTiles.Contains(new Point(x, y))) {
                    assignPaintColorAndMethod();
                    if((paintMethod == PaintMethods.RemovePaint || paintMethod == PaintMethods.Walls || paintMethod == PaintMethods.TilesAndWalls)
                            && color >= 0 && x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY) {
                        byte trueColor = WeaponsOfMassDecoration.getCurrentColorID(this, false);
                        if(trueColor != 0 || paintMethod == PaintMethods.RemovePaint) {
                            if(Main.tile[x, y].wallColor() != trueColor) {
                                if(WorldGen.paintWall(x, y, trueColor)) {
                                    sendTileFrame(x, y);
                                    if(currentPaintIndex != -1 && shouldConsumePaint())
                                        getOwner().inventory[currentPaintIndex].stack--;
                                }
                            }
                        }
                    }
                    paintedTiles.Add(new Point(x, y));
                }
            }
        }

        public bool shouldConsumePaint() {
            Player p = getOwner();
            for(int i = 0; i < p.armor.Length/2; i++) {
                if(p.armor[i].type == ModContent.ItemType<Items.ArtistPalette>())
                    return false;
            }
            if(Main.rand.NextFloat() <= paintConsumptionChance) {
                if(p.inventory[currentPaintIndex].modItem is Items.CustomPaint)
                    return Main.rand.NextFloat(0, 1) <= ((Items.CustomPaint)p.inventory[currentPaintIndex].modItem).consumptionChance;
                return true;
            }
            return false;
        }

        public void sendTileFrame(int x,int y) {
            WorldGen.SquareTileFrame(x, y);
            WorldGen.SquareWallFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y, 1);
        }

        public void createLight(Vector2 pos,float brightness) {
			Color c = WeaponsOfMassDecoration.getColor(this);
            Lighting.AddLight(pos, (c.R / 255f) * brightness, (c.G / 255f) * brightness, (c.B / 255f) * brightness);
        }

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			}
		}
	}
}
