using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Dusts;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
	public class WoMDNPC : GlobalNPC, IPaintable{
		/// <summary>
		/// Whether or not the NPC is currently painted
		/// </summary>
		protected bool _painted = false;
		public bool Painted => _painted;

		/// <summary>
		/// This is for the Painted buff's Update function
		/// </summary>
		public void RefreshPainted() { _painted = true; }

		/// <summary>
		/// The paint data that is currently used for rendering this npc
		/// </summary>
		protected PaintData _paintData = new(npcCyclingTimeScale, -1, null);
		/// <summary>
		/// The paint data that is currently used for rendering this npc
		/// </summary>
		public PaintData PaintData => !_painted ? new PaintData(npcCyclingTimeScale, -1, null) : _paintData;

		public void RemovePaint() {
			_painted = false;
			_paintData = null;
		}

		//Each entity needs their own set of the above variables
		public override bool InstancePerEntity => true;
		//Don't want instances to be cloned
		protected override bool CloneNewInstances => false;

		public override void SetStaticDefaults() {
			int artistType = NPCType<Artist>();
			NPCHappiness.Get(NPCID.Painter).SetNPCAffection(artistType, AffectionLevel.Dislike);
		}

		public bool buffConfetti = false;

		public override void ResetEffects(NPC npc) {
			_painted = false;
			buffConfetti = false;
		}

		/// <summary>
		/// Sets the variables regarding the rendering of the NPC. Called in WeaponsOfMassDecoration.applyPaintedToNPC
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="data"></param>
		public void SetPaintData(NPC npc, PaintData data) {
			if (data == null) {
				_paintData = null;
				_painted = false;
			} else {
				_paintData = data;
				_painted = true;
			}
			if(Multiplayer) {
				SendColorPacket(this, npc);
			}
		}

		/// <summary>
		/// Sends a packet to sync variables regarding the rendering of the NPC
		/// </summary>
		/// <param name="gNpc"></param>
		/// <param name="npc"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void SendColorPacket(WoMDNPC gNpc, NPC npc, int toClient = -1, int ignoreClient = -1) {
			if(!gNpc.Painted)
				return;
			PaintData data = gNpc.PaintData;
			if(Server || Multiplayer) {
				ModPacket packet = gNpc.Mod.GetPacket();
				packet.Write(WoMDMessageTypes.SetNPCColors);
				packet.Write(npc.whoAmI);
				packet.Write(npc.type);
				packet.Write(gNpc.Painted);
				if (gNpc.Painted) {
					packet.Write(data.PaintColor);
					packet.Write(data.CustomPaint == null ? "null" : data.CustomPaint.GetType().Name);
					packet.Write(data.sprayPaint);
					packet.Write((double)data.TimeOffset);
					byte[] enabledBuffs = data.buffConfig.GetEnabledColors();
					packet.Write((byte)enabledBuffs.Length);
					foreach (byte b in enabledBuffs)
						packet.Write(b);
				}
				packet.Send(toClient, ignoreClient);
			}
		}

		/// <summary>
		/// Handles reading a packet and setting variables regarding the rendering of the NPC
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="gNpc"></param>
		/// <param name="npc"></param>
		public static void ReadColorPacket(BinaryReader reader, out WoMDNPC gNpc, out NPC npc) {
			int npcId = reader.ReadInt32();
			int npcType = reader.ReadInt32();
			npc = GetNPC(npcId);
			gNpc = npc.GetGlobalNPC<WoMDNPC>();
			bool painted = reader.ReadBoolean();
			if (painted) {
				int paintColor = reader.ReadInt32();
				string customPaintName = reader.ReadString();
				bool sprayPainted = reader.ReadBoolean();
				float paintedTime = (float)reader.ReadDouble();
				byte buffCount = reader.ReadByte();
				List<byte> enabledBuffs = new();
				for (byte i = 0; i < buffCount; i++)
					enabledBuffs.Add(reader.ReadByte());
				PaintBuffConfig buffConfig = new();
				buffConfig.LoadEnabledColors(enabledBuffs);
				if (npc != null && npc.type == npcType && gNpc != null && npc.active) {
					gNpc._painted = true;
					PaintData data = new() {
						PaintColor = paintColor,
						buffConfig = buffConfig
					};
					if (customPaintName == "null") {
						data.CustomPaint = null;
					} else {
						data.CustomPaint = (CustomPaint)Activator.CreateInstance(Type.GetType("WeaponsOfMassDecoration.Items." + customPaintName));
						data.TimeScale = npcCyclingTimeScale;
					}
					data.sprayPaint = sprayPainted;
					data.TimeOffset = paintedTime;
					gNpc._paintData = data;
				}
			} else {
				gNpc._painted = false;
				gNpc._paintData = null;
			}
		}

		//This is used for controlling certain Chaos Mode functionality
		public override void PostAI(NPC npc) {
			base.PostAI(npc);
			if (buffConfetti) {
				if (Main.rand.NextBool(30))
					SpawnConfetti(npc.Center);
			}
			if((SinglePlayer || Server) && ChaosMode) {
				if(Painted) {
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
									Projectile p = Projectile.NewProjectileDirect(
										npc.GetSource_FromAI(),
										npc.Bottom - new Vector2(0, 8), 
										startVector.RotatedBy(((Math.PI * 2f / 3f) / (numSplatters - 1)) * i), 
										ProjectileType<PaintSplatter>(), 
										0, 
										0
									);
									if(p != null) {
										PaintingProjectile proj = (PaintingProjectile)p.ModProjectile;
										proj.npcOwner = npc.whoAmI;
										p.timeLeft = 60;
										PaintingProjectile.SendProjNPCOwnerPacket(proj);
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
								Vector2 dir = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(0, (float)Math.PI * 2f));
								Vector2 offset = dir * ((npc.width / 2f) * npc.scale);
								float initialSpeed = ((float)Math.Pow(30 - Math.Abs(50 - npc.ai[1]), 2) / 900f) * 4f + 3f;
								for(int i = 0; i < count; i++) {
									if(i == 1) {
										dir = dir.RotatedBy(Math.PI);
										offset *= -1;
									}
									float speed = initialSpeed;
									if(dir.Y > .5f && dir.X <= 0)
										speed += 2;
									else if(dir.Y < 0 && dir.X < 0)
										speed += 1;
									Vector2 vel = dir.RotatedBy((float)Math.PI / 2f) * speed;
									Projectile p = Projectile.NewProjectileDirect(
										npc.GetSource_FromAI(),
										npc.Center + offset, 
										vel, 
										ProjectileType<PaintSplatter>(), 
										0, 
										0
									);
									if(p != null) {
										p.velocity = vel;
										p.timeLeft = 100;
										PaintingProjectile proj = p.ModProjectile as PaintingProjectile;
										if(proj != null) {
											proj.npcOwner = npc.whoAmI;
											if(Server)
												PaintingProjectile.SendProjNPCOwnerPacket(proj);
										}
									}
								}
							}
							break;
					}
				}
			}
		}

		//This is used for controlling certain Chaos Mode functionality
		public override void OnKill(NPC npc) {
			base.OnKill(npc);
			if(ChaosMode) {
				if(Painted) {
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
									Vector2 rotatedDir = dir.RotatedBy((Math.PI / 4f) * i);
									PaintSplatter p = CreatePaintSplatter(npc.whoAmI, npc.Center + rotatedDir * 48, rotatedDir * 10, 100, .5f, 2f);
									if(p != null)
										projectiles.Add(p);
									rotatedDir = rotatedDir.RotatedBy(Math.PI / 8f);
									PaintSplatter p2 = CreatePaintSplatter(npc.whoAmI, npc.Center + rotatedDir * 32, rotatedDir * 6, 100);
									if(p2 != null)
										projectiles.Add(p2);
								}
								if(SinglePlayer) {
									for(int i = 0; i < 20; i++) {
										Dust d = Dust.NewDustDirect(npc.Center - npc.Size / 4f, npc.width / 2, npc.height / 2, DustType<PaintDust>(), 0, 0, 0, GetColor(_paintData), 2);
										if(d != null) {
											d.velocity = (d.position - npc.Center).SafeNormalize(new Vector2(1, 0)) * 5;
											if(d.customData != null) {
												((float[])d.customData)[0] = 0;
											}
										}
									}
								}
								if(Server)
									PaintingProjectile.SendMultiProjNPCOwnerPacket(projectiles);
							}
							break;
					}
				}
				if(npc.townNPC) {
					if(npc.type == NPCID.PartyGirl) {
						SplatterColored(npc.Center, 8, new byte[] { PaintID.DeepRedPaint, PaintID.DeepRedPaint, PaintID.DeepOrangePaint, PaintID.DeepYellowPaint, PaintID.DeepGreenPaint, PaintID.DeepBluePaint, PaintID.DeepPurplePaint }, new PaintData(1, PaintMethods.BlocksAndWalls), true);
						for(int i = 0; i < 10; i++) {
							Vector2 vel = new Vector2(Main.rand.NextFloat(2, 3), 0).RotatedBy(Main.rand.NextFloat((float)Math.PI * 2));
							Dust.NewDust(npc.Center - npc.Size / 4f, npc.width / 2, npc.height / 2, DustID.Confetti + Main.rand.Next(0,4), vel.X, vel.Y, 0);
						}
					} else {
						PaintData data = new(PaintID.DeepRedPaint);
						Splatter(npc.Center, 100f, 8, data, true);
					}
				}
			}
		}

		private static PaintSplatter CreatePaintSplatter(int npcOwner, Vector2 position, Vector2 velocity, int timeLeft = 30, float light = 0, float scale = 1f) {
			Projectile p = Projectile.NewProjectileDirect(
				Main.npc[npcOwner].GetSource_FromAI(),
				position, 
				velocity, 
				ProjectileType<PaintSplatter>(), 
				0, 
				0
			);
			if(p != null) {
				p.scale = scale;
				p.velocity = velocity;
				p.timeLeft = timeLeft;
				p.light = light;
				PaintSplatter proj = p.ModProjectile as PaintSplatter;
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
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if(Painted && !Server) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				ApplyShader(this, PaintData, new DrawData(TextureAssets.Npc[npc.type].Value, npc.position, npc.frame, Color.White));
			}
			return true;
		}

		//This just resets the spritebatch after the NPC is drawn, if necessary
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
			}
			if(Painted) {
				switch(npc.type) {
					case NPCID.Paladin:
						Lighting.AddLight(npc.Center, GetColor(_paintData).ToVector3() * .5f);
						break;
				}
			}
		}

		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			bool rewardsProgram = false;

			WoMDPlayer player = GetModPlayer(Main.myPlayer);
			if(player != null) 
				rewardsProgram = player.accRewardsProgram;
			
			switch(type) {
				case NPCID.Painter:
					if(Multiplayer && ShouldSellPaintingStuff()) {
						shop.item[nextSlot].SetDefaults(ItemType<TeamPaint>());
						nextSlot++;
					}
					break;
			}

			if(rewardsProgram) {
				for(int i = 0; i < shop.item.Length; i++) {
					Item item = shop.item[i];
					if (!item.active)
						continue;
					RewardsProgram.ModifyPrice(ref item);
				}
			}
		}

		/// <summary>
		/// A generic test to check if an npc should sell painting items. Will return true if the painter has moved in, or if the player is currently carrying any paint related items
		/// </summary>
		/// <returns></returns>
		public static bool ShouldSellPaintingStuff() {
			if(NPC.AnyNPCs(NPCID.Painter))
				return true;
			Player p = GetPlayer(Main.myPlayer);
			if(p == null)
				return false;
			Item[] inv = p.inventory;
			for(int c = 0; c < inv.Length; c++) {
				Item i = inv[c];
				if(i.active && i.stack > 0) {
					if(i.ModItem is PaintingItem)
						return true;
					if(IsPaintingTool(i))
						return true;
					if(IsPaint(i))
						return true;
				}
			}
			return false;
		}

		public override void OnSpawn(NPC npc, IEntitySource source) {
			base.OnSpawn(npc, source);
			if (SinglePlayer || Server) {
				if (source is EntitySource_Parent parent) {
					Entity entity = parent.Entity;
					if (entity != null) {
						if (entity is NPC n) {
							WoMDNPC globalNpc = n.GetGlobalNPC<WoMDNPC>();
							if (globalNpc.Painted)
								ApplyPaintedToNPC(npc, globalNpc.PaintData);
						} else if (entity is Projectile proj) {
							WoMDProjectile globalProj = proj.GetGlobalProjectile<WoMDProjectile>();
							if (globalProj.Painted)
								ApplyPaintedToNPC(npc, globalProj.PaintData);
						}
					}
				}
			}
		}
	}
}
