using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using WeaponsOfMassDecoration.Items;
using WeaponsOfMassDecoration.NPCs;
using WeaponsOfMassDecoration.Projectiles;

namespace WeaponsOfMassDecoration {
	public static class ShaderUtils {
		/// <summary>
		/// Applies a shader for the provided WoMDProjectile. Possible results are the Painted and PaintedNegative shaders.
		/// </summary>
		/// <param name="gProjectile"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(WoMDProjectile gProjectile, PaintData data) {
			MiscShaderData shader = getShader(gProjectile, data);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided WoMDNPC. Possible results are the Painted, SprayPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="drawData">This is necessary for the SprayPainted shader to work</param>
		/// <returns></returns>
		public static MiscShaderData applyShader(WoMDNPC globalNpc, PaintData data, DrawData? drawData = null) {
			MiscShaderData shader = getShader(globalNpc, data, out bool needsDrawData);
			if(shader != null)
				shader.Apply(needsDrawData ? drawData : null);
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided PaintingItem. Possible results are the GSPainted and PaintedNegative shader.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(PaintingItem item, Player player) {
			MiscShaderData shader = getShader(item, player);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData applyShader(PaintingProjectile projectile, PaintData data) {
			MiscShaderData shader = getShader(projectile, data);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Gets a shader for the provided WoMDProjectile. Possible results are the Painted and PaintedNegative shaders.
		/// </summary>
		/// <param name="gProjectile"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDProjectile gProjectile, PaintData data) {
			if(gProjectile == null)
				return null;
			if(!gProjectile.painted)
				return null;
			if(data.paintColor == PaintID.Negative || data.customPaint is NegativeSprayPaint)
				return getNegativeShader();
			Color color = getColor(data);
			return getPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDNPC. Possible results are the Painted, SprayPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="drawData">This is necessary for the SprayPainted shader to work</param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDNPC globalNpc, PaintData data, out bool needsDrawData) {
			needsDrawData = false;
			if(globalNpc == null)
				return null;
			if(!globalNpc.painted)
				return null;
			if(data.paintColor == -1 && data.customPaint == null)
				return null;
			if(data.paintColor == PaintID.Negative || data.customPaint is NegativeSprayPaint)
				return getNegativeShader();
			Color color = getColor(data);
			if(data.customPaint != null && data.sprayPaint) {
				needsDrawData = true;
				return getSprayPaintedShader(color);
			}
			return getPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingItem. Possible results are the GSPainted and PaintedNegative shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(PaintingItem item, Player player) {
			if(player == null)
				return null;
			if(item == null)
				return null;
			WoMDPlayer modPlayer = player.GetModPlayer<WoMDPlayer>();
			if(modPlayer == null)
				return null;
			PaintData data = modPlayer.paintData.clone();
			data.paintMethod = item.overridePaintMethod(modPlayer);
			if(data.paintMethod == PaintMethods.RemovePaint)
				return null;
			if(data.paintColor == PaintID.Negative || data.customPaint is NegativeSprayPaint)
				return getNegativeShader();
			return getGSShader(data.renderColor);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(PaintingProjectile projectile, PaintData data) {
			if(projectile == null)
				return null;
			if((data.paintColor == -1 && data.customPaint == null) || data.paintMethod == PaintMethods.RemovePaint)
				return null;
			if(data.paintColor == PaintID.Negative || data.customPaint is NegativeSprayPaint)
				return getNegativeShader();
			if(projectile.usesGSShader)
				return getGSShader(data.renderColor);
			return getPaintedShader(data.renderColor);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDItem. Possible results are the GSPainted and NegativePainted shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData getShader(WoMDItem item, PaintData data) {
			if(data.paintColor == PaintID.Negative || data.customPaint is NegativeSprayPaint)
				return getNegativeShader();
			return getGSShader(data.renderColor);
		}
		/// <summary>
		/// Gets the data for the PaintedNegative shader
		/// </summary>
		/// <returns></returns>
		private static MiscShaderData getNegativeShader() {
			MiscShaderData data = GameShaders.Misc["PaintedNegative"];
			return data;
		}
		/// <summary>
		/// Gets the data for the Painted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData getPaintedShader(Color color) {
			MiscShaderData data = GameShaders.Misc["Painted"].UseColor(color).UseOpacity(1f);
			return data;
		}
		/// <summary>
		/// Gets the data for the SprayPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <param name="drawData"></param>
		/// <returns></returns>
		private static MiscShaderData getSprayPaintedShader(Color color) {
			MiscShaderData data = GameShaders.Misc["SprayPainted"].UseColor(color).UseImage("Images/Misc/noise").UseOpacity(1f);
			return data;
		}
		/// <summary>
		/// Gets the data for the GSPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData getGSShader(Color color) {
			MiscShaderData data = GameShaders.Misc["GSPainted"].UseColor(color).UseOpacity(1f);
			return data;
		}

		public static Color getColor(PaintData data) {
			if(data == null)
				return Color.White;
			return data.renderColor;
			/*if((data.paintColor == -1 && data.customPaint == null) || data.paintMethod == PaintMethods.RemovePaint)
				return Color.White;
			if(data.customPaint == null)
				return PaintColors.list[data.paintColor];
			return data.customPaint.getColor(data);*/
		}
	}
}
