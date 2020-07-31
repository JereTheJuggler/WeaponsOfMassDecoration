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
        public int yFrameCount = 1;

		public override void SetStaticDefaults() {
            SetStaticDefaults("", "");
        }

        public virtual void SetStaticDefaults(string preToolTip,string postToolTip = "") {
            Tooltip.SetDefault((preToolTip != "" ? preToolTip + "\n" : "") +
                "Paints blocks and walls!\nBrushing, rolling, or scraping is determined by the first painting tool in your inventory" +
                (postToolTip != "" ? "\n" + postToolTip : ""));
        }

        public Player getOwner() {
            return getPlayer(item.owner);
		}

        protected bool resetBatchInPost = false;

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if(usesGSShader) {
                MiscShaderData shader = getShader(this,out int paintColor, out CustomPaint customPaint, out WoMDPlayer player);
                if(shader != null) {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix); // SpriteSortMode needs to be set to Immediate for shaders to work.
                    
                    shader.Apply();

                    Texture2D texture = getTexture(paintColor, customPaint, player.getPaintMethod());
                    if(texture == null)
                        texture = Main.itemTexture[item.type];

                    spriteBatch.Draw(texture, position, frame, drawColor,0,new Vector2(0,0),scale,SpriteEffects.None,0);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);//, Main.GameViewMatrix.ZoomMatrix);
                    return false;
                }
            }
            return true;
		}

        protected virtual Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
            return null;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(resetBatchInPost) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                resetBatchInPost = false;
            }
        }

		public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
            WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			if(!p.canPaint()) {
                mult *= .5f;
			} else if(p.getPaintMethod() ==  PaintMethods.RemovePaint){
                mult *= .5f;
			}
		}

		public override void OnHitNPC(Player p, NPC target, int damage, float knockBack, bool crit) {
            WoMDNPC npc = target.GetGlobalNPC<WoMDNPC>();
            if(npc != null && p != null && item.owner == Main.myPlayer) {
                WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
                PaintMethods method = player.getPaintMethod();
                if(method != PaintMethods.None) {
                    if(method == PaintMethods.RemovePaint) {
                        npc.painted = false;
                    } else {
                        player.getPaintVars(out int paintColor, out CustomPaint customPaint);
                        applyPaintedToNPC(target, paintColor, customPaint, new CustomPaintData() { player = p });
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
