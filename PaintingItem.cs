using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Buffs;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.PaintUtils;
using static WeaponsOfMassDecoration.ShaderUtils;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	public abstract class PaintingItem : ModItem {
		/// <summary>
		/// This text is applied to all painting weapons
		/// </summary>
		public const string halfDamageText = "Damage is halved if you don't have any paint.";

		/// <summary>
		/// Whether or not this item uses the "green screen" shader during rendering in the player's inventory
		/// </summary>
		public abstract bool UsesGSShader {
			get;
		}
		/// <summary>
		/// The number of textures this item uses for rendering in the player's inventory
		/// </summary>
		public abstract int TextureCount {
			get;
		}

		public override void SetStaticDefaults() {
			SetStaticDefaults("", "", true);
		}

		public virtual void SetStaticDefaults(string preToolTip, string postToolTip = "", bool dealsDamage = true) {
			List<string> lines = new();

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
			if(postToolTip != "") {
				lines.AddRange(postToolTip.Split('\n'));
			}
			if(this is not CustomPaint) {
				lines.AddRange(new string[] {
					"Current Tool: ",
					"Current Paint: "
				});
			}
			Tooltip.SetDefault(string.Join("\n", lines));
		}

		//This is what fills in the "Current Tool" and "Current Paint" lines of the tooltip
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			Player p = GetPlayer(Item.playerIndexTheItemIsReservedFor);
			if(p == null)
				return;
			WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
			if(player == null)
				return;
			for(int i = 0; i < tooltips.Count; i++) {
				if(tooltips[i].Text.StartsWith("Current Tool: ")) {
					tooltips[i].Text = "Current Tool: " + GetPaintToolName(player.paintData.paintMethod);
				} else if(tooltips[i].Text.StartsWith("Current Paint: ")) {
					tooltips[i].Text = "Current Paint: " + GetPaintColorName(player.paintData);
				}
			}
			base.ModifyTooltips(tooltips);
		}

		//This is where shaders are applied to items to make them reflect the current paint and tool the player is using
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Player p = GetPlayer(Main.myPlayer);
			if(p == null)
				return true;
			WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
			if(player == null)
				return true;
			MiscShaderData shader = GetShader(this, p);
			if (shader == null) 
				return true;
			if((UsesGSShader || ((player.paintData.PaintColor == PaintID.NegativePaint || player.paintData.CustomPaint is NegativeSprayPaint) && this is not CustomPaint))) {
				if(shader != null) {
					spriteBatch.End();
					spriteBatch.Begin(
						SpriteSortMode.Immediate, 
						BlendState.AlphaBlend, 
						(player.paintData.PaintColor == PaintID.NegativePaint || player.paintData.CustomPaint is NegativeSprayPaint) ? 
							SamplerState.LinearClamp : //linear clamp works for the negative paint shader
							SamplerState.PointClamp, //point clamp is needed for green screen shader because linear messes with the chroma keying
						DepthStencilState.Default, 
						RasterizerState.CullNone, 
						null, 
						Main.UIScaleMatrix
					); // SpriteSortMode needs to be set to Immediate for shaders to work.

					shader.Apply();
				}

				Texture2D texture = GetTexture(player);
				if(texture == null)
					texture = TextureAssets.Item[Item.type].Value;

				spriteBatch.Draw(texture, position, frame, drawColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

				if(shader != null) {
					spriteBatch.End();
					spriteBatch.Begin(
						SpriteSortMode.Deferred, 
						BlendState.AlphaBlend, 
						SamplerState.LinearClamp, 
						DepthStencilState.Default, 
						RasterizerState.CullNone, 
						null, 
						Main.UIScaleMatrix
					);
				}
				return false;
			}
			return true;
		}

		protected virtual Texture2D GetTexture(WoMDPlayer player) {
			//default handling based on conventions with texture counts and texture names
			switch(TextureCount) {
				case 2: //expects a default version with no paint, and a version with paint
					if((player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null) || player.paintData.paintMethod == PaintMethods.RemovePaint)
						return null;
					return GetExtraTexture(GetType().Name + "Painted");
				case 3: //expects a default version with no paint, and versions with paint and as a paint scraper
					if(player.paintData.paintMethod == PaintMethods.RemovePaint)
						return GetExtraTexture(GetType().Name + "Scraper");
					if(player.paintData.PaintColor == -1 && player.paintData.CustomPaint == null)
						return null;
					return GetExtraTexture(GetType().Name + "Painted");
			}
			return null;
		}

		//This is where damage is cut in half if the player's current tool is a paint scraper or the player doesn't have any paint or tools in their inventory
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			WoMDPlayer p = player.GetModPlayer<WoMDPlayer>();
			if(p == null)
				return;
			if(!p.CanPaint()) {
				damage.Scale(.5f);
			} else if(p.paintData.paintMethod == PaintMethods.RemovePaint) {
				damage.Scale(.5f);
			}
		}

		//This is where the painted buff is applied to NPCs that the item hits with melee
		public override void OnHitNPC(Player p, NPC target, int damage, float knockBack, bool crit) {
			WoMDNPC npc = target.GetGlobalNPC<WoMDNPC>();
			if(npc != null && p != null && Item.playerIndexTheItemIsReservedFor == Main.myPlayer) {
				WoMDPlayer player = p.GetModPlayer<WoMDPlayer>();
				PaintMethods method = OverridePaintMethod(player);
				if(method != PaintMethods.None) {
					if(method == PaintMethods.RemovePaint) {
						npc.painted = false;
						int index = target.FindBuffIndex(BuffType<Painted>());
						if(index >= 0)
							target.DelBuff(index);
					} else {
						ApplyPaintedToNPC(target, new PaintData(npcCyclingTimeScale, player.paintData.PaintColor, player.paintData.CustomPaint, player.paintData.CustomPaint is ISprayPaint, Main.GlobalTimeWrappedHourly, player: player.Player));
					}
				}
			}
		}

		/// <summary>
		/// By default has no effect. Allows the item to override the paintMethod used for painting tiles. This is required for the Painting Multi-Tools to still work even if a different painting tool is found earlier in the player's inventory
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual PaintMethods OverridePaintMethod(WoMDPlayer player) => player.paintData.paintMethod;
	}
}