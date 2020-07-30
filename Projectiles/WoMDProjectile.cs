using System;
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

namespace WeaponsOfMassDecoration.Projectiles {
	class WoMDProjectile : GlobalProjectile {
		public bool painted = false;
		public int paintColor = -1;
		public CustomPaint customPaint = null;
		public bool sprayPainted = false;
		public float paintedTime = 0;
		public int npcOwner = -1;

		public bool setupPreAi = false;

		public override bool InstancePerEntity {
			get {
				return true;
			}
		}

		public override bool CloneNewInstances {
			get {
				return false;
			}
		}

		public static void sendProjectileColorPacket(WoMDProjectile gProj,Projectile proj,int toClient=-1,int ignoreClient=-1) {
			Console.WriteLine("sending projectile color packet");
			ModPacket packet = gProj.mod.GetPacket();
			packet.Write(WoMDMessageTypes.SetProjectileColor);
			packet.Write(proj.whoAmI);
			packet.Write(proj.type);
			packet.Write(gProj.paintColor);
			packet.Write(gProj.customPaint == null ? "null" : gProj.customPaint.GetType().Name);
			packet.Write((double)gProj.paintedTime);
			packet.Send(toClient, ignoreClient);
		}

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

		public override bool PreAI(Projectile projectile) {
			if(server() || singlePlayer()) {
				if(!projectile.friendly && !setupPreAi) {
					if(GetInstance<WoMDConfig>().chaosModeEnabled) {
						setupPreAi = true;
						switch(projectile.type) {
							case ProjectileID.WoodenArrowHostile:
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
								break;
							case ProjectileID.RainNimbus:
								NPC nimbus = Main.npc
									.Where(npc => npc.type == NPCID.AngryNimbus)
									.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
									.FirstOrDefault();
								if(nimbus != null) {
									WoMDNPC gNpc = nimbus.GetGlobalNPC<WoMDNPC>();
									if(gNpc == null || !gNpc.painted)
										break;
									WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
									proj.painted = true;
									proj.paintColor = gNpc.paintColor;
									proj.paintedTime = gNpc.paintedTime;
									proj.customPaint = gNpc.customPaint;
									proj.npcOwner = nimbus.whoAmI;
									if(server())
										sendProjectileColorPacket(proj, projectile);
								}
								break;
							case ProjectileID.SandnadoHostileMark:
							case ProjectileID.SandnadoHostile:
								NPC elemental = Main.npc
									.Where(npc => npc.type == NPCID.SandElemental)
									.OrderBy(npc => Math.Abs(npc.Center.X - projectile.Center.X) + Math.Abs(npc.Center.Y - projectile.Center.Y))
									.FirstOrDefault();
								if(elemental != null) {
									WoMDNPC gNpc = elemental.GetGlobalNPC<WoMDNPC>();
									if(gNpc == null || !gNpc.painted)
										break;
									WoMDProjectile proj = projectile.GetGlobalProjectile<WoMDProjectile>();
									proj.painted = true;
									proj.paintColor = gNpc.paintColor;
									proj.paintedTime = gNpc.paintedTime;
									proj.customPaint = gNpc.customPaint;
									proj.npcOwner = elemental.whoAmI;
									if(server())
										sendProjectileColorPacket(proj, projectile);
								}
								break;
						}
					}
				}
			}
			return base.PreAI(projectile);
		}
		
		public override void PostAI(Projectile projectile) {
			if(singlePlayer() || server()) {
				if(painted) {
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
					}
				}
			}
			base.PostAI(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft) {
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

		public static void cloneProperties(Projectile src,Projectile dest) {
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

		public bool resetBatchInPost = false;

		public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			if(painted && Main.netMode != NetmodeID.Server && (paintColor != -1 || customPaint != null)) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this, projectile);
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
