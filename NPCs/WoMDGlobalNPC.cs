using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.Projectiles;
using WeaponsOfMassDecoration.Constants;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.NPCs {
    public class WoMDNPC : GlobalNPC{
        public bool painted = false;
        public int paintColor = -1;
        public CustomPaint customPaint = null;
        public bool sprayPainted = false;
		public float paintedTime = 0;

        public override bool InstancePerEntity { get { return true; } }
		public override bool CloneNewInstances { get { return false; } }

        public override void ResetEffects(NPC npc) {
            painted = false;
        }


        public void setColors(NPC npc, int paintColor, CustomPaint customPaint, bool sprayPainted, float paintedTime) {
            WoMDNPC gNpc = npc.GetGlobalNPC<WoMDNPC>();
            if(gNpc != null) {
                if(paintColor != gNpc.paintColor || sprayPainted != gNpc.sprayPainted || ((customPaint == null) != (gNpc.customPaint == null)) || (customPaint != null && customPaint.displayName != gNpc.customPaint.displayName)) {
                    gNpc.paintColor = paintColor;
                    gNpc.customPaint = customPaint;
                    gNpc.sprayPainted = sprayPainted;
                    if(multiplayer()) {
                        sendColorPacket(gNpc,npc);
                    }
                }
            }
		}

        public static void sendColorPacket(WoMDNPC gNpc, NPC npc, int toClient = -1, int ignoreClient = -1) {
            if(server() || multiplayer()) {
                ModPacket packet = gNpc.mod.GetPacket();
                packet.Write(WoMDMessageTypes.SetNPCColors);
                packet.Write(npc.whoAmI);
                packet.Write(npc.type);
                packet.Write(gNpc.paintColor);
                packet.Write(gNpc.customPaint == null ? "null" : gNpc.customPaint.GetType().Name);
                packet.Write(gNpc.sprayPainted);
                packet.Write((double)gNpc.paintedTime);
                packet.Send(toClient, ignoreClient);
            }
        }

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
                gNpc.paintColor = paintColor;
                if(customPaintName == "null") {
                    gNpc.customPaint = null;
                } else {
                    gNpc.customPaint = (CustomPaint)Activator.CreateInstance(Type.GetType("WeaponsOfMassDecoration.Items." + customPaintName));
                }
                gNpc.sprayPainted = sprayPainted;
                gNpc.paintedTime = paintedTime;
			}
        }

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
                    }
                }
            }
		}

		private bool resetBatchInPost;
		
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(painted && Main.netMode != NetmodeID.Server && (paintColor != -1 || customPaint != null)) {
				resetBatchInPost = true;

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.

				applyShader(this, new DrawData(Main.npcTexture[npc.type], npc.position, npc.frame, Color.White));
			}
			return true;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			if(resetBatchInPost) {
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				resetBatchInPost = false;
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
    
        public void getPaintVars(out int paintColor, out CustomPaint customPaint) {
            paintColor = this.paintColor;
            customPaint = this.customPaint;
		}
    }
}
