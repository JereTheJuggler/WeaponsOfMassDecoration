using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Dusts;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
	public class WoMDNPC : GlobalNPC {
		/// <summary>
		/// Whether or not the NPC is currently painted
		/// </summary>
		public bool painted = false;

		protected PaintData _paintData = new PaintData(npcCyclingTimeScale, -1, null);
		public PaintData paintData => !painted ? new PaintData(npcCyclingTimeScale, -1, null) : _paintData;

		//Each entity needs their own set of the above variables
		public override bool InstancePerEntity => true;
		//Don't want instances to be cloned
		public override bool CloneNewInstances => false;

		public override void ResetEffects(NPC npc) {
			painted = false;
		}

		/// <summary>
		/// Sets the variables regarding the rendering of the NPC. Called in WeaponsOfMassDecoration.applyPaintedToNPC
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="paintColor"></param>
		/// <param name="customPaint"></param>
		/// <param name="sprayPainted"></param>
		/// <param name="paintedTime"></param>
		public void setPaintData(NPC npc, PaintData data) {
			_paintData = data;
			painted = true;
			if(multiplayer()) {
				sendColorPacket(this, npc);
			}
		}

		/// <summary>
		/// Sends a packet to sync variables regarding the rendering of the NPC
		/// </summary>
		/// <param name="gNpc"></param>
		/// <param name="npc"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void sendColorPacket(WoMDNPC gNpc, NPC npc, int toClient = -1, int ignoreClient = -1) {
			if(!gNpc.painted)
				return;
			PaintData data = gNpc.paintData;
			if(server() || multiplayer()) {
				ModPacket packet = gNpc.mod.GetPacket();
				packet.Write(WoMDMessageTypes.SetNPCColors);
				packet.Write(npc.whoAmI);
				packet.Write(npc.type);
				packet.Write(data.paintColor);
				packet.Write(data.customPaint == null ? "null" : data.customPaint.GetType().Name);
				packet.Write(data.sprayPaint);
				packet.Write((double)data.timeOffset);
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Handles reading a packet and setting variables regarding the rendering of the NPC
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="gNpc"></param>
		/// <param name="npc"></param>
		public static void readColorPacket(BinaryReader reader, out WoMDNPC gNpc, out NPC npc) {
			int npcId = reader.ReadInt32();
			int npcType = reader.ReadInt32();
			npc = getNPC(npcId);
			gNpc = npc.GetGlobalNPC<WoMDNPC>();
			int paintColor = reader.ReadInt32();
			string customPaintName = reader.ReadString();
			bool sprayPainted = reader.ReadBoolean();
			float paintedTime = (float)reader.ReadDouble();
			if(npc != null && npc.type == npcType && gNpc != null && npc.active) {
				gNpc.painted = true;
				PaintData data = new PaintData();
				data.paintColor = paintColor;
				if(customPaintName == "null") {
					data.customPaint = null;
				} else {
					data.customPaint = (CustomPaint)Activator.CreateInstance(Type.GetType("WeaponsOfMassDecoration.Items." + customPaintName));
					data.timeScale = npcCyclingTimeScale;
				}
				data.sprayPaint = sprayPainted;
				data.timeOffset = paintedTime;
			}
		}

		//This is used for controlling certain Chaos Mode functionality
		public override void PostAI(NPC npc) {
			base.PostAI(npc);
			if(Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server) {
				if(GetInstance<WoMDConfig>().chaosModeEnabled) {
					if(painted) {
						switch(npc.aiStyle) {
							case 1: //slime
								if(npc.oldVelocity.Y > 2 && npc.velocity.Y == 0 && Main.rand.NextFloat() < .5f) {
									Point minTile = npc.BottomLeft.ToTileCoordinates();
									Point maxTile = npc.BottomRight.ToTileCoordinates();
									if(!(WorldGen.InWorld(minTile.X, minTile.Y + 1, 10) && WorldGen.InWorld(maxTile.X, maxTile.Y + 1, 10)))
										break;
									bool foundGround = false;
									for(int i = minTile.X; i <= maxTile.X && !foundGround; i++) {
										if(WorldGen.SolidOrSlopedTile(i, minTile.Y + 1))
											foundGround = true;
									}
									if(!foundGround)
										break;
									Vector2 startVector = new Vector2(0, -6).RotatedBy(Math.PI / -3);
									int numSplatters = 7;
									for(int i = 0; i < numSplatters; i++) {
										Projectile p = Projectile.NewProjectileDirect(npc.Bottom - new Vector2(0, 8), startVector.RotatedBy(((Math.PI * 2f / 3f) / (numSplatters - 1)) * i), ProjectileType<PaintSplatter>(), 0, 0);
										if(p != null) {
											PaintingProjectile proj = (PaintingProjectile)p.modProjectile;
											proj.npcOwner = npc.whoAmI;
											p.timeLeft = 60;
											PaintingProjectile.sendProjNPCOwnerPacket(proj);
										}
									}
								}
								break;
						}
						switch(npc.type) {
							case NPCID.EyeofCthulhu:
								//the eye starts spinning when ai[0] == 1
								if(npc.ai[0] == 1 && npc.ai[1] >= 20 && npc.ai[1] % 2 == 0) {
									//the eye stops spinning when ai[1] > 99 (ai[1] never == 100. it is immediately reset back to 0)
									int count = npc.ai[1] >= 40 && npc.ai[1] <= 60 ? 2 : 1;
									Vector2 dir = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(0, PI * 2));
									Vector2 offset = dir * ((npc.width / 2f) * npc.scale);
									float initialSpeed = ((float)Math.Pow(30 - Math.Abs(50 - npc.ai[1]), 2) / 900f) * 4f + 3f;
									for(int i = 0; i < count; i++) {
										if(i == 1) {
											dir = dir.RotatedBy(PI);
											offset *= -1;
										}
										float speed = initialSpeed;
										if(dir.Y > .5f && dir.X <= 0)
											speed += 2;
										else if(dir.Y < 0 && dir.X < 0)
											speed += 1;
										Vector2 vel = dir.RotatedBy(PI / 2f) * speed;
										Projectile p = Projectile.NewProjectileDirect(npc.Center + offset, vel, ProjectileType<PaintSplatter>(), 0, 0);
										if(p != null) {
											p.velocity = vel;
											p.timeLeft = 100;
											PaintingProjectile proj = p.modProjectile as PaintingProjectile;
											if(proj != null) {
												proj.npcOwner = npc.whoAmI;
												if(server())
													PaintingProjectile.sendProjNPCOwnerPacket(proj);
											}
										}
									}
								}
								break;
						}
					}
				}
			}
		}

		//This is used for controlling certain Chaos Mode functionality
		public override void NPCLoot(NPC npc) {
			base.NPCLoot(npc);
			if(painted && GetInstance<WoMDConfig>().chaosModeEnabled) {
				switch(npc.type) {
					case NPCID.EyeofCthulhu:
					case NPCID.Spazmatism:
					case NPCID.Retinazer:
					case NPCID.Skeleton:
					case NPCID.SkeletronHand:
					case NPCID.SkeletronPrime:
					case NPCID.PrimeCannon:
					case NPCID.PrimeLaser:
					case NPCID.PrimeSaw:
					case NPCID.PrimeVice:
					case NPCID.BrainofCthulhu:
					case NPCID.KingSlime:
					case NPCID.Everscream:
					case NPCID.SantaNK1:
					case NPCID.IceQueen:
					case NPCID.Plantera:
					case NPCID.MourningWood:
					case NPCID.WallofFlesh:
					case NPCID.WallofFleshEye:
					case NPCID.DukeFishron:
					case NPCID.MoonLordCore:
					case NPCID.QueenBee:
					case NPCID.DD2OgreT2:
					case NPCID.DD2OgreT3:
					case NPCID.DD2Betsy:
					case NPCID.DD2DarkMageT1:
					case NPCID.DD2DarkMageT3:
						if(true) {
							Vector2 dir = new Vector2(1, 0);
							List<PaintingProjectile> projectiles = new List<PaintingProjectile>();
							for(int i = 0; i < 8; i++) {
								Vector2 rotatedDir = dir.RotatedBy((PI / 4f) * i);
								PaintSplatter p = createPaintSplatter(npc.whoAmI, npc.Center + rotatedDir * 48, rotatedDir * 10, 100, .5f, 2f);
								if(p != null)
									projectiles.Add(p);
								rotatedDir = rotatedDir.RotatedBy(PI / 8f);
								PaintSplatter p2 = createPaintSplatter(npc.whoAmI, npc.Center + rotatedDir * 32, rotatedDir * 6, 100);
								if(p2 != null)
									projectiles.Add(p2);
							}
							if(singlePlayer()) {
								for(int i = 0; i < 20; i++) {
									Dust d = Dust.NewDustDirect(npc.Center - npc.Size / 4f, npc.width / 2, npc.height / 2, DustType<PaintDust>(), 0, 0, 0, getColor(_paintData), 2);
									if(d != null) {
										d.velocity = (d.position - npc.Center).SafeNormalize(new Vector2(1, 0)) * 5;
										if(d.customData != null) {
											((float[])d.customData)[0] = 0;
										}
									}
								}
							}
							if(server())
								PaintingProjectile.sendMultiProjNPCOwnerPacket(projectiles);
						}
						break;
				}
			}
			if(npc.townNPC && GetInstance<WoMDConfig>().chaosModeEnabled) {
				if(npc.type == NPCID.PartyGirl) {
					splatterColored(npc.Center, 8, new int[] { PaintID.DeepRed, PaintID.DeepRed, PaintID.DeepOrange, PaintID.DeepYellow, PaintID.DeepGreen, PaintID.DeepBlue, PaintID.DeepPurple }, new PaintData(0, useWorldGen: true));
					List<Color> colors = new List<Color> {
						PaintColors.Green,
						PaintColors.SkyBlue,
						PaintColors.Yellow,
						PaintColors.Pink
					};
					for(int i = 0; i < 10; i++) {
						Vector2 vel = new Vector2(Main.rand.NextFloat(2, 3), 0).RotatedBy(Main.rand.NextFloat(PI * 2));
						Dust.NewDust(npc.Center - npc.Size / 4f, npc.width / 2, npc.height / 2, DustID.Confetti, vel.X, vel.Y, 0, colors[Main.rand.Next(0, colors.Count)]);
					}
				} else {
					PaintData data = new PaintData(PaintID.DeepRed, useWorldGen: true);
					splatter(npc.Center, 100f, 8, data);
				}
			}
		}

		private PaintSplatter createPaintSplatter(int npcOwner, Vector2 position, Vector2 velocity, int timeLeft = 30, float light = 0, float scale = 1f) {
			Projectile p = Projectile.NewProjectileDirect(position, velocity, ProjectileType<PaintSplatter>(), 0, 0);
			if(p != null) {
				p.scale = scale;
				p.velocity = velocity;
				p.timeLeft = timeLeft;
				p.light = light;
				PaintSplatter proj = p.modProjectile as PaintSplatter;
				if(proj != null) {
					proj.npcOwner = npcOwner;
					return proj;
				}
			}
			return null;
		}

		/// <summary>
		/// Whether or not the spritebatch needs to be reset after drawing an NPC
		/// </summary>
		private bool resetBatchInPost;

		//This is where the shaders are applied to the NPCs to make them appear painted
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(painted && !server()) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this, paintData, new DrawData(Main.npcTexture[npc.type], npc.position, npc.frame, Color.White));
			}
			return true;
		}

		//This just resets the spritebatch after the NPC is drawn, if necessary
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
			if(painted) {
				switch(npc.type) {
					case NPCID.Paladin:
						Lighting.AddLight(npc.Center, getColor(_paintData).ToVector3() * .5f);
						break;
				}
			}
		}

		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			switch(type) {
				case NPCID.Steampunker:
					if(shouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<Items.PaintSolution>());
						nextSlot++;
					}
					break;
				case NPCID.Merchant:
					if(shouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<Items.PaintArrow>());
						nextSlot++;

						shop.item[nextSlot].SetDefaults(ItemType<Items.ThrowingPaintbrush>());
						nextSlot++;

						shop.item[nextSlot].SetDefaults(ItemType<Items.PaintShuriken>());
						nextSlot++;
					}
					break;
				case NPCID.Demolitionist:
					if(shouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<Items.PaintBomb>());
						nextSlot++;

						shop.item[nextSlot].SetDefaults(ItemType<Items.PaintDynamite>());
						nextSlot++;
					}
					break;
				case NPCID.ArmsDealer:
					if(shouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<Items.Paintball>());
						nextSlot++;
					}
					break;
				case NPCID.Painter:
					if(multiplayer() && shouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<TeamPaint>());
						nextSlot++;
					}
					break;
			}
		}

		/// <summary>
		/// A generic test to check if an npc should sell painting items. Will return true if the painter has moved in, or if the player is currently carrying any paint related items
		/// </summary>
		/// <returns></returns>
		public bool shouldSellPaintingStuff() {
			if(NPC.AnyNPCs(NPCID.Painter))
				return true;
			Player p = getPlayer(Main.myPlayer);
			if(p == null)
				return false;
			Item[] inv = p.inventory;
			for(int c = 0; c < inv.Length; c++) {
				Item i = inv[c];
				if(i.active && i.stack > 0) {
					if(i.modItem is PaintingItem)
						return true;
					if(isPaintingTool(i))
						return true;
					if(isPaint(i))
						return true;
				}
			}
			return false;
		}
	}
}
