using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Dusts;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Projectiles {
	public class WoMDProjectile : GlobalProjectile {
		/// <summary>
		/// Whether or not this projectile has been painted
		/// </summary>
		public bool painted = false;
		/// <summary>
		/// The whoAmI of the NPC that this projectile belongs to
		/// </summary>
		public int npcOwner = -1;

		/// <summary>
		/// The data used for painting with and rendering this projectile
		/// </summary>
		protected PaintData _paintData = new(npcCyclingTimeScale, 0f);
		public PaintData PaintData => !painted ? new PaintData(npcCyclingTimeScale, -1, null) : _paintData;

		/// <summary>
		/// Whether or this projectile has been taken care of in the preAI event
		/// </summary>
		public bool setupPreAi = false;

		//All the above fields need to be stored per entity
		public override bool InstancePerEntity => true;

		//Don't want new instances to be cloned either
		protected override bool CloneNewInstances => false;

		/// <summary>
		/// Sends a packet to sync the variables regarding the rendering of the projectile
		/// </summary>
		/// <param name="gProj"></param>
		/// <param name="proj"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void SendProjectileColorPacket(WoMDProjectile gProj, Projectile proj, int toClient = -1, int ignoreClient = -1) {
			if(!gProj.painted)
				return;
			PaintData data = gProj.PaintData;
			if(Server() || Multiplayer()) {
				ModPacket packet = gProj.Mod.GetPacket();
				packet.Write(WoMDMessageTypes.SetProjectileColor);
				packet.Write(proj.whoAmI);
				packet.Write(proj.type);
				packet.Write(data.PaintColor);
				packet.Write(data.CustomPaint == null ? "null" : data.CustomPaint.GetType().Name);
				packet.Write((double)data.TimeOffset);
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Handles reading a packet to sync the variables regarding the rendering of the projectile
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="gProj"></param>
		/// <param name="proj"></param>
		public static void ReadProjectileColorPacket(BinaryReader reader, out WoMDProjectile gProj, out Projectile proj) {
			int projId = reader.ReadInt32();
			int projType = reader.ReadInt32();
			proj = GetProjectile(projId);
			gProj = proj.GetGlobalProjectile<WoMDProjectile>();
			int paintColor = reader.ReadInt32();
			string customPaintName = reader.ReadString();
			float paintedTime = (float)reader.ReadDouble();
			if(proj != null && proj.type == projType && gProj != null && proj.active) {
				gProj.painted = true;
				PaintData data = new PaintData();
				data.PaintColor = paintColor;
				if(customPaintName == "null") {
					data.CustomPaint = null;
				} else {
					data.CustomPaint = (CustomPaint)Activator.CreateInstance(Type.GetType("WeaponsOfMassDecoration.Items." + customPaintName));
				}
				data.TimeOffset = paintedTime;
				gProj._paintData = data;
			}
		}

		//Handles assigning new projectiles npc owners
		public override bool PreAI(Projectile projectile) {
			if(Server() || SinglePlayer()) {
				if(!projectile.friendly && !setupPreAi) {
					if(GetInstance<WoMDConfig>().chaosModeEnabled) {
						setupPreAi = true;
						switch(projectile.type) {
							case ProjectileID.WoodenArrowHostile:
								if(true) {
									if(Server())
										break; //running into issues making this work in multiplayer
									NPC archer = Main.npc
										.Where(npc => npc.type == NPCID.CultistArcherBlue || npc.type == NPCID.GoblinArcher)
										.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
										.FirstOrDefault();
									if(archer != null) {
										WoMDNPC gNpc = archer.GetGlobalNPC<WoMDNPC>();
										if(gNpc == null || !gNpc.painted)
											break;
										int projId = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity, ProjectileType<PaintArrow>(), projectile.damage, projectile.knockBack);
										Projectile newArrow = GetProjectile(projId);
										if(newArrow == null)
											break;
										PaintArrow arrow = (PaintArrow)newArrow.ModProjectile;
										CloneProperties(projectile, arrow.Projectile);
										arrow.npcOwner = archer.whoAmI;
										arrow.Projectile.GetGlobalProjectile<WoMDProjectile>().setupPreAi = true;
										projectile.timeLeft = 0;
										if(Server())
											PaintingProjectile.SendProjNPCOwnerPacket(arrow);
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
										ApplyPaintedFromNpc(projectile, nimbus);
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
										ApplyPaintedFromNpc(projectile, elemental);
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
											enemyPos = enemy.Center + (projectile.velocity * 2.75f);
											range = 130;
											break;
										case NPCID.ElfCopter:
											range = 30;
											break;
									}
									float dist = Math.Abs(enemyPos.X - projectile.Center.X) + Math.Abs(enemyPos.Y - projectile.Center.Y);
									if(dist < range) {
										ApplyPaintedFromNpc(projectile, enemy);
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
											ApplyPaintedFromNpc(projectile, enemy);
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
											ApplyPaintedFromNpc(projectile, enemy);
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
												ApplyPaintedFromNpc(projectile, enemy);
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
		private static void ApplyPaintedFromNpc(Projectile projectile, NPC npc) {
			WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
			if(gNpc == null || !gNpc.painted)
				return;
			WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
			if(proj == null)
				return;
			proj._paintData = gNpc.PaintData.Clone();
			proj.npcOwner = npc.whoAmI;
			if(Server())
				SendProjectileColorPacket(proj, projectile);
		}

		private static void ApplyPaintedFromProjectile(Projectile dest, Projectile src) {
			WoMDProjectile dProj = dest.GetGlobalProjectile<WoMDProjectile>();
			WoMDProjectile sProj = src.GetGlobalProjectile<WoMDProjectile>();
			if(dProj == null || sProj == null)
				return;
			dProj.painted = true;
			dProj._paintData = sProj.PaintData.Clone();
			dProj.npcOwner = sProj.npcOwner;
			if(Server())
				SendProjectileColorPacket(dProj, dest);
		}

		public static void ApplyPainted(Projectile projectile, PaintData paintData) {
			WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
			if(proj == null)
				return;
			proj.painted = true;
			proj._paintData = paintData.Clone();
			if(Server())
				SendProjectileColorPacket(proj, projectile);
		}

		public override void PostAI(Projectile projectile) {
			if((SinglePlayer() || Server()) && painted) {
				switch(projectile.type) {
					case ProjectileID.SandnadoHostile:
						if(projectile.timeLeft > 930 && Main.rand.NextFloat() < .25f) {
							float y = Main.rand.NextFloat(projectile.height * .6f, projectile.height);
							Vector2 pos = new Vector2(projectile.position.X + projectile.width / 2f, projectile.position.Y + projectile.height - y);
							int dir = Main.rand.Next(0, 2) * 2 - 1;
							Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), pos, new Vector2(6f * dir, -6f - (float)Math.Sqrt(y / 8f)), ProjectileType<PaintSplatter>(), 0, 0);
							if(proj != null) {
								proj.timeLeft = (int)Math.Round(30f + (2f * y));
								PaintingProjectile p = proj.ModProjectile as PaintingProjectile;
								if(p != null) {
									p.npcOwner = projectile.GetGlobalProjectile<WoMDProjectile>().npcOwner;
									if(Server())
										PaintingProjectile.SendProjNPCOwnerPacket(p);
								}
							}
						}
						break;
					case ProjectileID.BulletDeadeye:
						Paint(projectile.Center, _paintData);
						break;
					case ProjectileID.PaladinsHammerHostile:
						if(true) {
							Color c = GetColor(_paintData);
							Lighting.AddLight(projectile.Center, c.ToVector3() * .5f);
							NPC npc = GetNPC(npcOwner);
							if(projectile.timeLeft % 30 == 0 && npc != null && projectile.timeLeft >= 3400) {
								float dSquare = ((GetNPC(npcOwner).Center - projectile.Center) / 16f).LengthSquared();
								//1600 is 40 blocks away from paladin
								//2500 is 50 blocks away from paladin
								if(dSquare >= 1600 && dSquare <= 2500) {
									Vector2 hammerTop = projectile.Center + new Vector2(0, -16 * 9).RotatedBy(projectile.rotation);
									List<Point> paintedTiles = new List<Point>();
									PaintBetweenPoints(projectile.Center, hammerTop, _paintData, paintedTiles: paintedTiles);
									Vector2 shaftOffset = (projectile.Center - hammerTop).SafeNormalize(default);
									Vector2 headOffset = shaftOffset.RotatedBy(Math.PI / 2f) * 64;
									for(int i = 0; i <= 64; i += 8) {
										PaintBetweenPoints(hammerTop + (shaftOffset * i) + headOffset, hammerTop + (shaftOffset * i) - headOffset, _paintData, paintedTiles: paintedTiles);
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
					case ProjectileID.Boulder:
						if(true) {
							Point bl = projectile.BottomLeft.ToTileCoordinates();
							for(byte i = 0; i <= 1; i++) {
								for(byte j = 0; j <= 2; j++) {
									Point p = new Point(bl.X + i, bl.Y - j);
									if(j != 0 || WorldGen.SolidOrSlopedTile(p.X, p.Y))
										Paint(p, PaintData, true);
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
						Explode(projectile.Center, 100f, _paintData);
						break;
					case ProjectileID.InfernoHostileBolt:
						Splatter(projectile.Center, 150f, 7, _paintData);
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
						Paint(projectile.Bottom + new Vector2(0, 16), PaintData);
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
		protected static void CloneProperties(Projectile src, Projectile dest) {
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

		/*
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			if(painted && !server() && (_paintData.PaintColor != -1 || _paintData.CustomPaint != null)) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this,_paintData);
			}
			return true;
		}

		public override void PostDraw(Projectile projectile, Color lightColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
		}*/
	}
}