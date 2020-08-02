﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using MonoMod.Utils;
using System.Diagnostics;
using WeaponsOfMassDecoration.Dusts;

namespace WeaponsOfMassDecoration.Projectiles {
	public class WoMDProjectile : GlobalProjectile {
		/// <summary>
		/// Whether or not this projectile has been painted
		/// </summary>
		public bool painted = false;
		/// <summary>
		/// The PaintID of the paint that is being used to paint with and render this projectile. -1 if projectile is using a custom paint
		/// </summary>
		public int paintColor = -1;
		/// <summary>
		/// The custom paint that is being used to paint with and render this projectile. null if projectile is using a vanilla paint
		/// </summary>
		public CustomPaint customPaint = null;
		/// <summary>
		/// The time that this projectile was painted. Used for applying shaders for color changing paints
		/// </summary>
		public float paintedTime = 0;
		/// <summary>
		/// The whoAmI of the NPC that this projectile belongs to
		/// </summary>
		public int npcOwner = -1;

		/// <summary>
		/// Whether or this projectile has been taken care of in the preAI event
		/// </summary>
		public bool setupPreAi = false;

		//All the above fields need to be stored per entity
		public override bool InstancePerEntity => true;

		//Don't want new instances to be cloned either
		public override bool CloneNewInstances => false;

		/// <summary>
		/// Sends a packet to sync the variables regarding the rendering of the projectile
		/// </summary>
		/// <param name="gProj"></param>
		/// <param name="proj"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void sendProjectileColorPacket(WoMDProjectile gProj,Projectile proj,int toClient=-1,int ignoreClient=-1) {
			if(server() || multiplayer()) {
				ModPacket packet = gProj.mod.GetPacket();
				packet.Write(WoMDMessageTypes.SetProjectileColor);
				packet.Write(proj.whoAmI);
				packet.Write(proj.type);
				packet.Write(gProj.paintColor);
				packet.Write(gProj.customPaint == null ? "null" : gProj.customPaint.GetType().Name);
				packet.Write((double)gProj.paintedTime);
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Handles reading a packet to sync the variables regarding the rendering of the projectile
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="gProj"></param>
		/// <param name="proj"></param>
		public static void readProjectileColorPacket(BinaryReader reader, out WoMDProjectile gProj, out Projectile proj) {
			int projId = reader.ReadInt32();
			int projType = reader.ReadInt32();
			proj = getProjectile(projId);
			gProj = proj.GetGlobalProjectile<WoMDProjectile>();
			int paintColor = reader.ReadInt32();
			string customPaintName = reader.ReadString();
			float paintedTime = (float)reader.ReadDouble();
			if(proj != null && proj.type == projType && gProj != null && proj.active) {
				gProj.painted = true;
				gProj.paintColor = paintColor;
				if(customPaintName == "null") {
					gProj.customPaint = null;
				} else {
					gProj.customPaint = (CustomPaint)Activator.CreateInstance(Type.GetType("WeaponsOfMassDecoration.Items." + customPaintName));
				}
				gProj.paintedTime = paintedTime;
			}
		}

		//Handles assigning new projectiles npc owners
		public override bool PreAI(Projectile projectile) {
			if(server() || singlePlayer()) {
				if(!projectile.friendly && !setupPreAi) {
					if(GetInstance<WoMDConfig>().chaosModeEnabled) {
						setupPreAi = true;
						switch(projectile.type) {
							case ProjectileID.WoodenArrowHostile:
								if(true) {
									if(server())
										break; //running into issues making this work in multiplayer
									NPC archer = Main.npc
										.Where(npc => npc.type == NPCID.CultistArcherBlue || npc.type == NPCID.GoblinArcher)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(archer != null) {
										WoMDNPC gNpc = archer.GetGlobalNPC<WoMDNPC>();
										if(gNpc == null || !gNpc.painted)
											break;
										int projId = Projectile.NewProjectile(projectile.Center, projectile.velocity, ProjectileType<PaintArrow>(), projectile.damage, projectile.knockBack);
										Projectile newArrow = getProjectile(projId);
										if(newArrow == null)
											break;
										PaintArrow arrow = (PaintArrow)newArrow.modProjectile;
										cloneProperties(projectile, arrow.projectile);
										arrow.npcOwner = archer.whoAmI;
										arrow.projectile.GetGlobalProjectile<WoMDProjectile>().setupPreAi = true;
										projectile.timeLeft = 0;
										if(server())
											PaintingProjectile.sendProjNPCOwnerPacket(arrow);
									}
								}
								break;
							case ProjectileID.RainNimbus:
								if(true) {
									NPC nimbus = Main.npc
										.Where(npc => npc.type == NPCID.AngryNimbus)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(nimbus != null) {
										applyPaintedFromNpc(projectile, nimbus);
									}
								}
								break;
							case ProjectileID.SandnadoHostileMark:
							case ProjectileID.SandnadoHostile:
								if(true) {
									NPC elemental = Main.npc
										.Where(npc => npc.type == NPCID.SandElemental)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(elemental != null) {
										applyPaintedFromNpc(projectile, elemental);
									}
								}
								break;
							case ProjectileID.BulletDeadeye:
								if(true) {
									NPC enemy = Main.npc
										.Where(npc => new int[] { NPCID.TacticalSkeleton, NPCID.SantaNK1, NPCID.ElfCopter, NPCID.PirateDeadeye, NPCID.PirateCaptain }.Contains(npc.type))
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									Vector2 enemyPos = enemy.Center;
									float range = 20;
									switch(enemy.type) {
										case NPCID.SantaNK1:
											enemyPos = enemy.Center+(projectile.velocity * 2.75f);
											range = 130;
											break;
										case NPCID.ElfCopter:
											range = 30;
											break;
									}
									float dist = Math.Abs(enemyPos.X - projectile.Center.X) + Math.Abs(enemyPos.Y - projectile.Center.Y);
									if(dist < range) {
										applyPaintedFromNpc(projectile, enemy);
									}
								}
								break;
							case ProjectileID.HappyBomb:
								if(true) {
									NPC enemy = Main.npc
										.Where(npc => npc.type == NPCID.Clown)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(enemy != null) {
										float dist = Math.Abs(enemy.Center.X - projectile.Center.X) + Math.Abs(enemy.Center.Y - projectile.Center.Y);
										if(dist < 64) 
											applyPaintedFromNpc(projectile, enemy);
									}
								}
								break;
							case ProjectileID.InfernoHostileBolt:
								if(true) {
									NPC enemy = Main.npc
										.Where(npc => npc.type == NPCID.DiabolistRed || npc.type == NPCID.DiabolistWhite)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(enemy != null) {
										float dist = Math.Abs(enemy.Center.X - projectile.Center.X) + Math.Abs(enemy.Center.Y - projectile.Center.Y);
										if(dist <= 20)
											applyPaintedFromNpc(projectile, enemy);
									}
								}
								break;
							case ProjectileID.PaladinsHammerHostile:
								if(true) {
									NPC enemy = Main.npc
										.Where(npc => npc.type == NPCID.Paladin)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(enemy != null) {
										WoMDNPC gNpc = enemy.GetGlobalNPC<WoMDNPC>();
										if(gNpc != null && gNpc.painted) {
											float dist = Math.Abs(enemy.Center.X - projectile.Center.X) + Math.Abs(enemy.Center.Y - projectile.Center.Y);
											if(dist <= 25) {
												applyPaintedFromNpc(projectile, enemy);
												projectile.Opacity = 0;
												projectile.alpha = 0;
											}
										}
									}
								}
								break;
						}
					}
				}
			}
			return base.PreAI(projectile);
		}
		
		/// <summary>
		/// Copies the painted settings from the provided npc to the provided projectile
		/// </summary>
		/// <param name="projectile">The projectile to inherit the painted settings</param>
		/// <param name="npc">The npc to copy the painted settings from</param>
		private static void applyPaintedFromNpc(Projectile projectile,NPC npc) {
			WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
			if(gNpc == null || !gNpc.painted)
				return;
			WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
			if(proj == null)
				return;
			proj.painted = true;
			proj.paintColor = gNpc.paintColor;
			proj.paintedTime = gNpc.paintedTime;
			proj.customPaint = gNpc.customPaint;
			proj.npcOwner = npc.whoAmI;
			if(server())
				sendProjectileColorPacket(proj, projectile);
		}

		private static void applyPaintedFromProjectile(Projectile dest, Projectile src) {
			WoMDProjectile dProj = dest.GetGlobalProjectile<WoMDProjectile>();
			WoMDProjectile sProj = src.GetGlobalProjectile<WoMDProjectile>();
			if(dProj == null || sProj == null)
				return;
			dProj.painted = true;
			dProj.paintColor = sProj.paintColor;
			dProj.paintedTime = sProj.paintedTime;
			dProj.customPaint = sProj.customPaint;
			dProj.npcOwner = sProj.npcOwner;
			if(server())
				sendProjectileColorPacket(dProj, dest);
		}

		public override void PostAI(Projectile projectile) {
			if(!projectile.friendly && (singlePlayer() || server()) && painted) {
				switch(projectile.type) {
					case ProjectileID.SandnadoHostile:
						if(projectile.timeLeft > 930 && Main.rand.NextFloat() < .25f) {
							float y = Main.rand.NextFloat(projectile.height * .6f, projectile.height);
							Vector2 pos = new Vector2(projectile.position.X + projectile.width / 2f, projectile.position.Y + projectile.height - y);
							int dir = Main.rand.Next(0, 2) * 2 - 1;
							//Projectile proj = Projectile.NewProjectileDirect(pos, new Vector2((4f + (float)Math.Sqrt(y / 16f)) * dir, -6f), ProjectileType<PaintSplatter>(), 0, 0);
							Projectile proj = Projectile.NewProjectileDirect(pos, new Vector2(6f * dir, -6f - (float)Math.Sqrt(y/8f)), ProjectileType<PaintSplatter>(), 0, 0);
							if(proj != null) {
								proj.timeLeft = (int)Math.Round(30f + (2f * y));
								PaintingProjectile p = proj.modProjectile as PaintingProjectile;
								if(p != null) {
									p.npcOwner = projectile.GetGlobalProjectile<WoMDProjectile>().npcOwner;
									if(server())
										PaintingProjectile.sendProjNPCOwnerPacket(p);
								}
							}
						}
						break;
					case ProjectileID.BulletDeadeye:
						paint(projectile.Center, paintColor, customPaint, new CustomPaintData(false, npcCyclingTimeScale, paintedTime, null));
						break;
					case ProjectileID.PaladinsHammerHostile:
						if(true) {
							Color c = getColor(this);
							Lighting.AddLight(projectile.Center, c.ToVector3() * .5f);
							NPC npc = getNPC(npcOwner);
							if(projectile.timeLeft % 30 == 0 && npc != null && projectile.timeLeft >= 3400) {
								float dSquare = ((getNPC(npcOwner).Center - projectile.Center) / 16f).LengthSquared();
								//1600 is 40 blocks away from paladin
								//2500 is 50 blocks away from paladin
								if(dSquare >= 1600 && dSquare <= 2500) {
									CustomPaintData data = new CustomPaintData(false, npcCyclingTimeScale, paintedTime);
									Vector2 hammerTop = projectile.Center + new Vector2(0, -16 * 9).RotatedBy(projectile.rotation);
									List<Point> paintedTiles = new List<Point>();
									paintBetweenPoints(projectile.Center, hammerTop, paintColor, customPaint, data, paintedTiles: paintedTiles);
									Vector2 shaftOffset = (projectile.Center - hammerTop).SafeNormalize(default);
									Vector2 headOffset = shaftOffset.RotatedBy(Math.PI / 2f) * 64;
									for(int i = 0; i <= 64; i += 8) {
										paintBetweenPoints(hammerTop + (shaftOffset * i) + headOffset, hammerTop + (shaftOffset * i) - headOffset, paintColor, customPaint, data, paintedTiles: paintedTiles);
									}
									foreach(Point p in paintedTiles) {
										Dust d = Dust.NewDustPerfect(p.ToWorldCoordinates(), DustType<PaintDust>(), new Vector2(0, 0), default, c, 2f);
										if(d != null && d.customData != null) {
											((float[])d.customData)[0] = 0;
										}
									}
								}
							}
						}
						break;
				}
			}
			base.PostAI(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft) {
			if(painted) {
				switch(projectile.type) {
					case ProjectileID.HappyBomb:
						explode(projectile.Center,100f,paintColor,customPaint,new CustomPaintData(false,npcCyclingTimeScale,paintedTime));
						break;
					case ProjectileID.InfernoHostileBolt:
						splatter(projectile.Center, 150f, 7, paintColor, customPaint, new CustomPaintData(false, npcCyclingTimeScale, paintedTime));
						break;
				}
			}
			return base.PreKill(projectile, timeLeft);
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) {
			WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
			if(proj.painted && GetInstance<WoMDConfig>().chaosModeEnabled) {
				switch(projectile.type) {
					case ProjectileID.RainNimbus:
						paint(projectile.Bottom + new Vector2(0, 16), paintColor, customPaint, new CustomPaintData(false, npcCyclingTimeScale, paintedTime));
						break;
				}
			}
			return base.OnTileCollide(projectile, oldVelocity);
		}

		/// <summary>
		/// Uses Reflection to copy most properties and field from one projectile to another
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dest"></param>
		protected static void cloneProperties(Projectile src,Projectile dest) {
			PropertyInfo[] properties = dest.GetType().GetProperties();
			foreach(PropertyInfo property in properties) {
				switch(property.Name) {
					case "modProjectile":
						continue;
				}
				if(property.CanWrite && property.CanRead) {
					property.SetValue(dest, property.GetValue(src));
					//System.Diagnostics.Debug.WriteLine("Set property " + property.Name);
				}
			}
			FieldInfo[] fields = dest.GetType().GetFields();
			foreach(FieldInfo field in fields) {
				switch(field.Name) {
					case "whoAmI":
					case "type":
					case "projUUID":
					case "identity":
						continue;
				}
				field.SetValue(dest, field.GetValue(src));
				//System.Diagnostics.Debug.WriteLine("Set field " + field.Name);
			}
		}

		/// <summary>
		/// Will be set to true if the spritebatch needs to be reset after the projectile is drawn. 
		/// </summary>
		protected bool resetBatchInPost = false;

		public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			if(painted && Main.netMode != NetmodeID.Server && (paintColor != -1 || customPaint != null)) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this);
			}
			return true;
		}

		public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
		}
	}
}