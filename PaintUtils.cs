﻿using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
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
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		/// <param name="paintedTiles">A list of all the tiles that were updated in this function</param>
		public static void PaintBetweenPoints(Vector2 start, Vector2 end, PaintData data, List<Point> paintedTiles = null, bool useWorldGen = false) {
			if(!(data.blocksAllowed || data.wallsAllowed))
				return;
			Vector2 unitVector = end - start;
			float distance = unitVector.Length();
			unitVector.Normalize();
			int iterations = (int)Math.Ceiling(distance / 8f);
			int count = 0;
			for(int i = 0; i < iterations; i++) {
				Vector2 pos = start + (unitVector * i * 8);
				if(Paint(pos, data, useWorldGen)) {
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
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		public static void Explode(Vector2 pos, float radius, PaintData data, bool useWorldGen = false) {
			for(int currentLevel = 0; currentLevel < Math.Ceiling(radius / 16f); currentLevel++) {
				if(currentLevel == 0) {
					Paint(pos, data, useWorldGen);
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
								Vector2 transform;
								switch(dir) {
									case 0:
									default:
										transform = offsetVector;
										break;
									case 1:
										transform = new Vector2(offsetVector.Y, offsetVector.X * -1);
										break;
									case 2:
										transform = offsetVector * -1;
										break;
									case 3:
										transform = new Vector2(offsetVector.Y * -1, offsetVector.X);
										break;
								}
								Paint(pos + transform, data, useWorldGen);
							}
						}
					}
				}
			}
		}

		public static void ExplodeColored(Vector2 pos, IEnumerable<byte> colors, PaintData data, bool useWorldGen = false) {
			for(int currentLevel = 0; currentLevel < colors.Count(); currentLevel++) {
				if(currentLevel == 0) {
					data.PaintColor = colors.ElementAt(currentLevel);
					Paint(pos, data, useWorldGen);
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
								Vector2 transform;
								switch(dir) {
									case 0:
									default:
										transform = offsetVector;
										break;
									case 1:
										transform = new Vector2(offsetVector.Y, offsetVector.X * -1);
										break;
									case 2:
										transform = offsetVector * -1;
										break;
									case 3:
										transform = new Vector2(offsetVector.Y * -1, offsetVector.X);
										break;
								}
								Point p = (pos + transform).ToTileCoordinates();
								Paint(p.X,p.Y, colors.ElementAt(currentLevel), data, useWorldGen);
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
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		public static void Splatter(Vector2 pos, float radius, int spokes, PaintData data, bool useWorldGen = false) {
			Explode(pos, 48f, data, useWorldGen);
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
						Paint(newPos.X, newPos.Y, data, useWorldGen);
					}
				}
			}
		}

		public static void SplatterColored(Vector2 pos, int spokes, IEnumerable<byte> colors, PaintData data, bool useWorldGen = false) {
			ExplodeColored(pos, new List<byte> { colors.ElementAt(0), colors.ElementAt(1), colors.ElementAt(2) }, data, useWorldGen);
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
						Paint(newPos.X, newPos.Y, colors.ElementAt((int)Math.Floor(offset / 16f)), data, useWorldGen);
					}
				}
			}
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="pos">The position of the tile. Expects values in world coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool Paint(Vector2 pos, PaintData data, bool useWorldGen = false) {
			Point p = pos.ToTileCoordinates();
			return Paint(p.X, p.Y, data, useWorldGen);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="pos">The position of the tile. Expects values in tile coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool Paint(Point pos, PaintData data, bool useWorldGen = false) => Paint(pos.X, pos.Y, data, useWorldGen);

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		/// <returns>Whether or not the tile was updated</returns>
		public static bool Paint(int x, int y, PaintData data, bool useWorldGen = false) {
			if(!WorldGen.InWorld(x, y, 10))
				return false;
			if(data.PaintColor == -1 && data.CustomPaint == null)
				return false;
			return Paint(x, y, data.TruePaintColor, data, useWorldGen);
		}

		/// <summary>
		/// Paints the tile at the given position
		/// </summary>
		/// <param name="x">The x coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="y">The y coordinate of the tile. Expects values in tile coordinates</param>
		/// <param name="color">The PaintID of the color to use</param>
		/// <param name="data">An instance of PaintData used to specify various settings for how painting should function</param>
		/// <param name="useWorldGen">Whether or not WorldGen.paintTile and WorldGen.paintWall should be used, or if the tile should just be modified directly. Using WorldGen causes additional visual effects</param>
		/// <returns>Whether or not the tile was updated</returns>
		private static bool Paint(int x, int y, byte color, PaintData data, bool useWorldGen = false) {
			if(data.paintMethod == PaintMethods.None)
				return false;

			if(data.sprayPaint) {
				if(WorldGen.genRand.NextFloat() < .6f)
					return false;
			}

			if(!WorldGen.InWorld(x, y, 10))
				return false;
			Tile t = Main.tile[x, y];
			if(t == null)
				return false;

			if(data.paintMethod == PaintMethods.RemovePaint)
				color = 0;

			bool updated = false;

			if(data.paintMethod != PaintMethods.Walls && data.blocksAllowed && t.HasTile && t.TileColor != color && (color != 0 || data.paintMethod == PaintMethods.RemovePaint)) {
				if(useWorldGen)
					WorldGen.paintTile(x, y, color, false);
				else
					t.TileColor = color;
				updated = true;
			}
			if(data.paintMethod != PaintMethods.Blocks && data.wallsAllowed && t.WallType > 0 && t.WallColor != color && (color != 0 || data.paintMethod == PaintMethods.RemovePaint)) {
				if(useWorldGen)
					WorldGen.paintWall(x, y, color, false);
				else
					t.WallColor = color;
				updated = true;
			}
			if(updated) {
				if(data.paintMethod != PaintMethods.RemovePaint && data.player != null && data.consumePaint) {
					WoMDPlayer player = data.player.GetModPlayer<WoMDPlayer>();
					if(player != null)
						player.ConsumePaint(data);
				}
				if(Server || Multiplayer)
					SendTileFrame(x, y);
			}
			return updated;
		}

		/// <summary>
		/// Sends a net message to update the tile at the given position
		/// </summary>
		/// <param name="x">The tile's x coordinate. Expects values in tile coordinates</param>
		/// <param name="y">The tile's y coordinate. Expects values in tile coordinates</param>
		public static void SendTileFrame(int x, int y) {
			NetMessage.SendTileSquare(-1, x, y, 1);
		}

		public static byte GetBasePaintID(byte id) {
			return id switch {
				PaintID.DeepRedPaint => PaintID.RedPaint,
				PaintID.DeepOrangePaint => PaintID.OrangePaint,
				PaintID.DeepYellowPaint => PaintID.YellowPaint,
				PaintID.DeepLimePaint => PaintID.LimePaint,
				PaintID.DeepGreenPaint => PaintID.GreenPaint,
				PaintID.DeepTealPaint => PaintID.TealPaint,
				PaintID.DeepCyanPaint => PaintID.CyanPaint,
				PaintID.DeepSkyBluePaint => PaintID.SkyBluePaint,
				PaintID.DeepBluePaint => PaintID.BluePaint,
				PaintID.DeepPurplePaint => PaintID.PurplePaint,
				PaintID.DeepVioletPaint => PaintID.VioletPaint,
				PaintID.DeepPinkPaint => PaintID.PinkPaint,
				_ => id,
			};
		}
	}
}
