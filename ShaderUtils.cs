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
		public static MiscShaderData ApplyShader(WoMDProjectile gProjectile, PaintData data) {
			MiscShaderData shader = GetShader(gProjectile, data);
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
		public static MiscShaderData ApplyShader(WoMDNPC globalNpc, PaintData data, DrawData? drawData = null) {
			MiscShaderData shader = GetShader(globalNpc, data, out bool needsDrawData);
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
		public static MiscShaderData ApplyShader(PaintingItem item, Player player) {
			MiscShaderData shader = GetShader(item, player);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Applies a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData ApplyShader(PaintingProjectile projectile, PaintData data) {
			MiscShaderData shader = GetShader(projectile, data);
			if(shader != null)
				shader.Apply();
			return shader;
		}
		/// <summary>
		/// Gets a shader for the provided WoMDProjectile. Possible results are the Painted and PaintedNegative shaders.
		/// </summary>
		/// <param name="gProjectile"></param>
		/// <returns></returns>
		public static MiscShaderData GetShader(WoMDProjectile gProjectile, PaintData data) {
			if(gProjectile == null)
				return null;
			if(!gProjectile.Painted)
				return null;
			if(data.PaintColor == PaintID.NegativePaint || data.CustomPaint is NegativeSprayPaint)
				return GetNegativeShader();
			Color color = GetColor(data);
			return GetPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDNPC. Possible results are the Painted, SprayPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="globalNpc"></param>
		/// <param name="drawData">This is necessary for the SprayPainted shader to work</param>
		/// <returns></returns>
		public static MiscShaderData GetShader(WoMDNPC globalNpc, PaintData data, out bool needsDrawData) {
			needsDrawData = false;
			if(globalNpc == null)
				return null;
			if(!globalNpc.Painted)
				return null;
			if (data == null)
				return null;
			if(data.PaintColor == -1 && data.CustomPaint == null)
				return null;
			if(data.PaintColor == PaintID.NegativePaint || data.CustomPaint is NegativeSprayPaint)
				return GetNegativeShader();
			Color color = GetColor(data);
			if(data.CustomPaint != null && data.sprayPaint) {
				needsDrawData = true;
				return GetSprayPaintedShader(color);
			}
			return GetPaintedShader(color);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingItem. Possible results are the GSPainted and PaintedNegative shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData GetShader(PaintingItem item, Player player) {
			if(player == null)
				return null;
			if(item == null)
				return null;
			WoMDPlayer modPlayer = player.GetModPlayer<WoMDPlayer>();
			if(modPlayer == null)
				return null;
			PaintData data = modPlayer.paintData.Clone();
			data.paintMethod = item.OverridePaintMethod(modPlayer);
			if(data.paintMethod == PaintMethods.RemovePaint)
				return null;
			if(data.PaintColor == PaintID.NegativePaint || data.CustomPaint is NegativeSprayPaint)
				return GetNegativeShader();
			return GetGSShader(data.RenderColor);
		}
		/// <summary>
		/// Gets a shader for the provided PaintingProjectile. Possible results are the Painted, GSPainted, and PaintedNegative shaders.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static MiscShaderData GetShader(PaintingProjectile projectile, PaintData data) {
			if(projectile == null)
				return null;
			if((data.PaintColor == -1 && data.CustomPaint == null) || data.paintMethod == PaintMethods.RemovePaint)
				return null;
			if(data.PaintColor == PaintID.NegativePaint || data.CustomPaint is NegativeSprayPaint)
				return GetNegativeShader();
			if(projectile.usesGSShader)
				return GetGSShader(data.RenderColor);
			return GetPaintedShader(data.RenderColor);
		}
		/// <summary>
		/// Gets a shader for the provided WoMDItem. Possible results are the GSPainted and NegativePainted shaders.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static MiscShaderData GetShader(WoMDItem item, PaintData data) {
			if(data.PaintColor == PaintID.NegativePaint || data.CustomPaint is NegativeSprayPaint)
				return GetNegativeShader();
			return GetGSShader(data.RenderColor);
		}
		/// <summary>
		/// Gets the data for the PaintedNegative shader
		/// </summary>
		/// <returns></returns>
		private static MiscShaderData GetNegativeShader() {
			MiscShaderData data = GameShaders.Misc["PaintedNegative"];
			return data;
		}
		/// <summary>
		/// Gets the data for the Painted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData GetPaintedShader(Color color) {
			MiscShaderData data = GameShaders.Misc["Painted"].UseColor(color).UseOpacity(1f);
			return data;
		}
		/// <summary>
		/// Gets the data for the SprayPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <param name="drawData"></param>
		/// <returns></returns>
		private static MiscShaderData GetSprayPaintedShader(Color color) {
			MiscShaderData data = GameShaders.Misc["SprayPainted"];
			data.UseColor(color);
			data.UseOpacity(1f);
			data.UseImage1("Images/Misc/noise");
			return data;
		}
		/// <summary>
		/// Gets the data for the GSPainted shader using the provided color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static MiscShaderData GetGSShader(Color color) {
			MiscShaderData data = GameShaders.Misc["GSPainted"].UseColor(color).UseOpacity(1f);
			return data;
		}

		public static Color GetColor(PaintData data) {
			if(data == null)
				return Color.White;
			return data.RenderColor;
			/*if((data.PaintColor == -1 && data.CustomPaint == null) || data.paintMethod == PaintMethods.RemovePaint)
				return Color.White;
			if(data.CustomPaint == null)
				return PaintColors.list[data.PaintColor];
			return data.CustomPaint.getColor(data);*/
		}
	}
}
