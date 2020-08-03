﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using WeaponsOfMassDecoration.NPCs;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	/// <summary>
	/// Handles all of the painting that occurs within this mod. Using these functions is crucial for ensuring that players consume paint properly, switch colors mid-operation if a player runs out of the paint they're using, and having custom paints output proper colors.
	/// </summary>
	public static class PaintUtils {

		/// <summary>
		/// The speed that custom paints cycle through colors for painting tiles. Also applies to the colors for projectile shaders to line up with the color they are painting.
		/// </summary>
		public const float paintCyclingTimeScale = .25f;

		/// <summary>
		/// The speed that custom paints cycle through colors for npc shaders
		/// </summary>
		public const float npcCyclingTimeScale = 1f;

		/// <summary>
		/// Paints tiles between the 2 provided world coordinates
		/// </summary>
		/// <param name="start">The starting position of the line to paint. Expects values in world coordinates</param>
		/// <param name="end">The ending position of the line to paint. Expects values in world coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <param name="paintedTiles">A list of all the tiles that were updated in this function</param>
		public static void paintBetweenPoints(Vector2 start, Vector2 end, PaintData data, List<Point> paintedTiles = null) {
			if(!(data.blocksAllowed || data.wallsAllowed))
				return;
			Vector2 unitVector = end - start;
			float distance = unitVector.Length();
			unitVector.Normalize();
			int iterations = (int)Math.Ceiling(distance / 8f);
			int count = 0;
			for(int i = 0; i < iterations; i++) {
				Vector2 pos = start + (unitVector * i * 8);
				if(paint(pos, data)) {
					count++;
					if(paintedTiles != null) {
						Point tPos = pos.ToTileCoordinates();
						if(!paintedTiles.Contains(tPos))
							paintedTiles.Add(tPos);
					}
				}
			}
		}

		/// <summary>
		/// Creates a circle of paint
		/// </summary>
		/// <param name="pos">The position of the center of the circle. Expects values in world coordinates</param>
		/// <param name="radius">The radiues of the circle. 16 for each tile</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		public static void explode(Vector2 pos, float radius, PaintData data) {
			for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
				if(currentLevel == 0) {
					paint(pos, data);
				} else {
					for(int i = 0; i <= currentLevel * 2; i++) {
						float xOffset;
						float yOffset;
						if(i <= currentLevel) {
							xOffset = currentLevel;
							yOffset = i;
						} else {
							xOffset = (currentLevel * 2 - i + 1);
							yOffset = (currentLevel + 1);
						}
						Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
						if(offsetVector.Length() <= radius) {
							for(int dir = 0; dir < 4; dir++) {
								paint(pos + offsetVector.RotatedBy(dir * (Math.PI / 2)), data);
							}
						}
					}
				}
			}
		}

		public static void explodeColored(Vector2 pos, IEnumerable<int> colors, PaintData data) {
			for(int currentLevel = 0; currentLevel < colors.Count(); currentLevel++) {
				if(currentLevel == 0) {
					data.paintColor = colors.ElementAt(currentLevel);
					paint(pos, data);
				} else {
					for(int i = 0; i <= currentLevel * 2; i++) {
						float xOffset;
						float yOffset;
						if(i <= currentLevel) {
							xOffset = currentLevel;
							yOffset = i;
						} else {
							xOffset = (currentLevel * 2 - i + 1);
							yOffset = (currentLevel + 1);
						}
						Vector2 offsetVector = new Vector2(xOffset * 16f, yOffset * 16f);
						if(offsetVector.Length() <= colors.Count() * 16f) {
							for(int dir = 0; dir < 4; dir++) {
								data.paintColor = colors.ElementAt(currentLevel);
								paint(pos + offsetVector.RotatedBy(dir * (Math.PI / 2)), data);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a splatter of paint
		/// </summary>
		/// <param name="pos">The position of the center of the splatter. Expects values in world coordinates</param>
		/// <param name="radius">The length of the spokes coming out of the center of the splatter. Uses world distance</param>
		/// <param name="spokes">The number of spokes to create</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		public static void splatter(Vector2 pos, float radius, int spokes, PaintData data) {
			explode(pos, 48f, data);
			float angle = Main.rand.NextFloat((float)Math.PI);
			float[] angles = new float[spokes];
			float[] radii = new float[spokes];
			for(int s = 0; s < spokes; s++) {
				angles[s] = angle;
				angle += Main.rand.NextFloat((float)Math.PI / 6, (float)(Math.PI * 2) / 3);
				radii[s] = radius - (Main.rand.NextFloat(4) * 8);
			}
			for(int offset = 0; offset < radius; offset += 8) {
				for(int s = 0; s < spokes; s++) {
					if(offset <= radii[s]) {
						Point newPos = new Point(
							(int)Math.Floor((pos.X + Math.Cos(angles[s]) * offset) / 16f),
							(int)Math.Floor((pos.Y + Math.Sin(angles[s]) * offset) / 16f)
						);
						paint(newPos.X, newPos.Y, data);
					}
				}
			}
		}

		public static void splatterColored(Vector2 pos, int spokes, IEnumerable<int> colors, PaintData data) {
			explodeColored(pos, new List<int> { colors.ElementAt(0), colors.ElementAt(1), colors.ElementAt(2) }, data);
			float radius = 16 * colors.Count();
			float angle = Main.rand.NextFloat((float)Math.PI);
			float[] angles = new float[spokes];
			float[] radii = new float[spokes];
			for(int s = 0; s < spokes; s++) {
				angles[s] = angle;
				angle += Main.rand.NextFloat((float)Math.PI / 6, (float)(Math.PI * 2) / 3);
				radii[s] = radius - (Main.rand.NextFloat(4) * 8);
			}
			for(int offset = 0; offset < radius; offset += 8) {
				for(int s = 0; s < spokes; s++) {
					if(offset <= radii[s]) {
						Point newPos = new Point(
							(int)Math.Floor((pos.X + Math.Cos(angles[s]) * offset) / 16f),
							(int)Math.Floor((pos.Y + Math.Sin(angles[s]) * offset) / 16f)
						);
						data.paintColor = colors.ElementAt((int)Math.Floor(offset / 16f));
						paint(newPos.X, newPos.Y, data);
					}
				}
			}
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="pos">The position of the tile. Expects values in world coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(Vector2 pos, PaintData data) {
			Point p = pos.ToTileCoordinates();
			return paint(p.X, p.Y, data);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool paint(int x, int y, PaintData data) {
			if(!WorldGen.InWorld(x, y, 10))
				return false;
			if(data.paintColor == -1 && data.customPaint == null)
				return false;
			byte targetColor;
			if(data.customPaint != null) {
				targetColor = data.customPaint.getPaintID(data);
			} else {
				targetColor = (byte)data.paintColor;
			}
			return paint(x, y, targetColor, data);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="color">The PaintID of the color to use</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <returns>Whether or not the tile was updated</returns>
		private static bool paint(int x, int y, byte color, PaintData data) {
			if(data.paintMethod == PaintMethods.None)
				return false;
			if(!WorldGen.InWorld(x, y, 10))
				return false;
			Tile t = Main.tile[x, y];
			if(t == null)
				return false;

			if(data.paintMethod == PaintMethods.RemovePaint)
				color = 0;

			bool updated = false;

			if(data.paintMethod != PaintMethods.Walls && data.blocksAllowed && t.active() && t.color() != color && (color != 0 || data.paintMethod == PaintMethods.RemovePaint)) {
				if(data.useWorldGen)
					WorldGen.paintTile(x, y, color, false);
				else
					t.color(color);
				updated = true;
			}
			if(data.paintMethod != PaintMethods.Blocks && data.wallsAllowed && t.wall > 0 && t.wallColor() != color && (color != 0 || data.paintMethod == PaintMethods.RemovePaint)) {
				if(data.useWorldGen)
					WorldGen.paintWall(x, y, color, false);
				else
					t.wallColor(color);
				updated = true;
			}
			if(updated) {
				if(data.paintMethod != PaintMethods.RemovePaint && data.player != null && data.consumePaint) {
					WoMDPlayer player = data.player.GetModPlayer<WoMDPlayer>();
					if(player != null)
						player.consumePaint(data);
				}
				if(server())
					sendTileFrame(x, y);
			}
			return updated;
		}

		/// <summary>
		/// Sends a net message to update the tile at the given position
		/// </summary>
		/// <param name="x">The tile's x coordinate. Expects values in tile coordinates</param>
		/// <param name="y">The tile's y coordinate. Expects values in tile coordinates</param>
		public static void sendTileFrame(int x, int y) {
			NetMessage.SendTileSquare(-1, x, y, 1);
		}
	}
}
