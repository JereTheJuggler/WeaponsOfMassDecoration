using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.NPCs;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System.Runtime.Remoting.Messaging;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using System.Runtime.InteropServices;

namespace WeaponsOfMassDecoration.Items {
    public abstract class PaintingItem : ModItem{

		public const string halfDamageText = "Damage is halved if you don't have any paint.";

        public bool usesGSShader = false;
        public int textureCount = 1;
        public float paintConsumptionChance = 1f;

		public override void SetStaticDefaults() {
            SetStaticDefaults("", "", true);
        }

        public virtual void SetStaticDefaults(string preToolTip,string postToolTip = "", bool dealsDamage = true) {
            List<string> lines = new List<string>();

			if(dealsDamage) {
                lines.Add(halfDamageText);
			}
            if(preToolTip != "") {
                lines.AddRange(preToolTip.Split('\n'));
			}
            lines.AddRange(new string[] {
                "Paints blocks and walls!",
                "The first paint and tool found in your inventory will be used"
            });
            if(paintConsumptionChance < 1f) {
                lines.Add(Math.Round((1 - paintConsumptionChance) * 100f).ToString() + "% chance to not consume paint");
			}
            if(postToolTip != "") {
                lines.AddRange(postToolTip.Split('\n'));
			}
            if(!(this is CustomPaint)) {
                lines.AddRange(new string[] {
                    "Current Tool: ",
                    "Current Paint: "
                });
            }
            Tooltip.SetDefault(string.Join("\n", lines));
        }

        public Player getOwner() {
            return getPlayer(item.owner);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
            Player p = getOwner();
            if(p == null)
                return;
            WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
            if(player == null)
                return;
            for(int i = 0; i < tooltips.Count; i++) {
                if(tooltips[i].text.StartsWith("Current Tool: ")) {
                    tooltips[i].text = "Current Tool: "+getPaintToolName(player.paintMethod);
				}else if(tooltips[i].text.StartsWith("Current Paint: ")) {
                    tooltips[i].text = "Current Paint: " + getPaintColorName(player.paintColor, player.customPaint);
				}
			}
			base.ModifyTooltips(tooltips);
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			MiscShaderData shader = getShader(this, out WoMDPlayer player);
            if((usesGSShader || ((player.paintColor == PaintID.Negative || player.customPaint is NegativeSprayPaint) && !(this is CustomPaint))) && shader != null) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (player.paintColor == PaintID.Negative || player.customPaint is NegativeSprayPaint) ? SamplerState.LinearClamp : SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
                    
                shader.Apply();

                Texture2D texture = getTexture(player);
                if(texture == null)
                    texture = Main.itemTexture[item.type];

                spriteBatch.Draw(texture, position, frame, drawColor,0,new Vector2(0,0),scale,SpriteEffects.None,0);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                return false;
            }
            return true;
		}

		protected virtual Texture2D getTexture(WoMDPlayer player) {
            //default handling based on conventions with texture counts and texture names
			switch(textureCount) {
                case 2: //expects a default version with no paint, and a version with paint
                    if((player.paintColor == -1 && player.customPaint == null) || player.paintMethod == PaintMethods.RemovePaint)
                        return null;
                    return getExtraTexture(GetType().Name + "Painted");
                case 3: //expects a default version with no paint, and versions with paint and as a paint scraper
                    if(player.paintMethod == PaintMethods.RemovePaint)
                        return getExtraTexture(GetType().Name + "Scraper");
                    if(player.paintColor == -1 && player.customPaint == null)
                        return null;
                    return getExtraTexture(GetType().Name + "Painted");
            }
            return null;
		}

		public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
            WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			if(!p.canPaint()) {
                mult *= .5f;
			} else if(p.paintMethod ==  PaintMethods.RemovePaint){
                mult *= .5f;
			}
		}

		public override void OnHitNPC(Player p, NPC target, int damage, float knockBack, bool crit) {
            WoMDNPC npc = target.GetGlobalNPC<WoMDNPC>();
            if(npc != null && p != null && item.owner == Main.myPlayer) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                PaintMethods method = player.paintMethod;
                if(method != PaintMethods.None) {
                    if(method == PaintMethods.RemovePaint) {
                        npc.painted = false;
                    } else {
                        applyPaintedToNPC(target, player.paintColor, player.customPaint, new CustomPaintData() { player = p });
                    }
                }
            }
        }

        /// <summary>
        /// Used to paint blocks and walls. blocksAllowed and wallsAllowed can be used to disable painting blocks and walls regardless of the player's current painting method.
        /// </summary>
        /// <param name="x">Tile x coordinate</param>
        /// <param name="y">Tile y coordinate</param>
        /// <param name="blocksAllowed">Can be used to disable painting blocks regardless of the player's current painting method</param>
        /// <param name="wallsAllowed">Can be used to disable painting walls regardless of the player's current painting method</param>
        public void paint(int x, int y, bool blocksAllowed = true, bool wallsAllowed = true) {
            if(!(blocksAllowed || wallsAllowed))
                return;
            if(x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;
            Player p = getOwner();
            if(item.owner == Main.myPlayer && p != null) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                player.paint(x, y, blocksAllowed, wallsAllowed);
            }
        }
    }
}
