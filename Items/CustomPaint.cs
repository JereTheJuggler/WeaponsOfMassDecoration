using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Steamworks;
using Microsoft.Xna.Framework;
using WeaponsOfMassDecoration.Constants;

namespace WeaponsOfMassDecoration.Items {
	/// <summary>
	/// An empty interface used to specify that a CustomPaint is a type of Spray Paint
	/// </summary>
	public interface ISprayPaint { }

	/// <summary>
	/// An empty interface used to specify that a CustomPaint is a Deep Paint
	/// </summary>
	public interface IDeepPaint { }

	/// <summary>
	/// An empty interface used to specify that a CustomPaint will cycle through different colors
	/// </summary>
	public interface ICyclingPaint { }

	/// <summary>
	/// A class that contains data that will be used to determine what color a custom paint will result in
	/// </summary>
	public struct CustomPaintData {
		/// <summary>
		/// How fast the paint will cycle through colors. Should be either WeaponsOfMassDecoration.paintCyclingTimeScale or WeaponsOfMassDecoration.npcCyclingTimeScale
		/// </summary>
		public float timeScale;
		/// <summary>
		/// An offset to use when calculating what color the paint will be. This allows the same custom paint to result in different colors at the same time
		/// </summary>
		public float timeOffset;
		/// <summary>
		/// The player to use when calculating what color the paint will be
		/// </summary>
		public Player player;
		/// <summary>
		/// Whether or not color should be forced. Color should be forced for anything that is not painting the world, including selecting frames, rendering shaders, and lights
		/// </summary>
		public bool forceColor;

		/// <summary>
		/// Creates an instance of CustomPaintData
		/// </summary>
		/// <param name="forceColor">Whether or not color should be forced. Color should be forced for anything that is not painting the world, including selecting frames, rendering shaders, and lights</param>
		/// <param name="timeScale">How fast the paint will cycle through colors. Should be either WeaponsOfMassDecoration.paintCyclingTimeScale or WeaponsOfMassDecoration.npcCyclingTimeScale</param>
		/// <param name="timeOffset">An offset to use when calculating what color the paint will be. This allows the same custom paint to result in different colors at the same time</param>
		/// <param name="player">The player to use when calculating what color the paint will be</param>
		public CustomPaintData(bool forceColor, float timeScale, float timeOffset = 0, Player player = null) {
			this.forceColor = forceColor;
			this.timeScale = timeScale;
			this.timeOffset = timeOffset;
			this.player = player;
		}

		/// <summary>
		/// Creates an instance of CustomPaintData
		/// </summary>
		/// <param name="forceColor">Whether or not color should be forced. Color should be forced for anything that is not painting the world, including selecting frames, rendering shaders, and lights.</param>
		/// <param name="timeScale">How fast the paint will cycle through colors. Should be either WeaponsOfMassDecoration.paintCyclingTimeScale or WeaponsOfMassDecoration.npcCyclingTimeScale</param>
		/// <param name="player">The player to use when calculating what color the paint will be</param>
		public CustomPaintData(bool forceColor, float timeScale, Player player) : this(forceColor, timeScale, default, player) { }
	}

	public abstract class CustomPaint : PaintingItem {
		/// <summary>
		/// The value assigned to item.paint for every instance of a CustomPaint
		/// </summary>
		public const byte paintValue = 64;

		/// <summary>
		/// The chance that this paint will be consumed when used
		/// </summary>
		public virtual float paintConsumptionChance { get { return 1f; } }
		/// <summary>
		/// Whether or not the paint can be crafted from the items in paintItemIds
		/// </summary>
		protected virtual bool _includeVanillaRecipes { get { return true; } }

		/// <summary>
		/// The number of different colors a custom paint can produce. This is preferable to using paintItemIds.Length because it will not go through the process of converting base paint item ids to deep paint item ids first
		/// </summary>
		public int colorCount { get { return _paintItemIds.Length; } }
		/// <summary>
		/// The item ids of the paints that the custom paint will use
		/// </summary>
		protected abstract int[] _paintItemIds { get; }
		/// <summary>
		/// The item ids of the paints that the custom paint will use
		/// </summary>
		public int[] paintItemIds {
			get {
				if(this is IDeepPaint) {
					int[] deepIds = new int[_paintItemIds.Length];
					for(int i = 0; i < deepIds.Length; i++)
						deepIds[i] = getDeepItemId(_paintItemIds[i]);
					return deepIds;
				} else {
					return (int[])_paintItemIds.Clone();
				}
			}
		}
		/// <summary>
		/// Gets the item id of the deep version of the provided paint item id
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		protected static int getDeepItemId(int type) {
			int index = Array.IndexOf(PaintItemID.list, type);
			if(index == -1)
				return type;
			if(index >= PaintID.Red && index <= PaintID.Pink)
				return PaintItemID.list[index + 12];
			return type;
		}

		/// <summary>
		/// The base color name for the custom paint. Additional keywords for Spray, Deep, and Paint will automatically be added in the public CustomPaint.displayName
		/// </summary>
		protected abstract string _colorName { get; }
		/// <summary>
		/// The display name to use for the custom paint
		/// </summary>
		public string displayName {
			get {
				string name = _colorName;
				if(this is ISprayPaint)
					name += " Spray";
				name += " Paint";
				if(this is IDeepPaint)
					name = "Deep " + name;
				return name;
			}
		}

		/// <summary>
		/// If true, the paint will cycle through colors going 1 2 3 4 1 2 3 4 etc. If false, the paint will cycle through colors going 1 2 3 4 3 2 1 2 3 4 3 etc.
		/// </summary>
		public bool cycleLoops = false;

		public CustomPaint() : base() { }
		public override void SetDefaults() {
			item.paint = paintValue;
		}
		public override void SetStaticDefaults() {
			DisplayName.SetDefault(displayName);
			string paintConsumptionText = "";
			if(paintConsumptionChance < 1)
				paintConsumptionText = Math.Round(100 * (1f - paintConsumptionChance)).ToString() + "% chance to not be consumed";
			SetStaticDefaults(paintConsumptionText, "",false);
		}

		/* Required recipes:
		* The number in parenthesis at the beginning of each description refers to which if statement is responsible for adding the recipe
		* 
		* For cycling paints:
		* - (1) using 1 of each base item to create 1 * the number of base items used.				Ex: 1 red + 1 orange + 1 yellow = 3 flame
		* 
		* For deep cycling paints:
		* - (2) using 2 of each base item to create .5 * the number of base items used.				Ex: 2 red + 2 orange + 2 yellow = 3 deep flame
		* - (1) using 1 of each deep item to create 1 * the number of deep items used.				Ex: 1 deep red + 1 deep orange + 1 deep yellow = 3 deep flame
		* - (3) using 2 of the base cycling paint to create 1 item.									Ex: 2 flame paint = 1 deep flame paint
		* 
		* For vanilla spray paints:
		* - (1) using 1 of the base item to create 1 item.											Ex: 1 red paint = 1 red spray paint
		* 
		* For deep vanilla spray paints:
		* - (2) using 2 of the base item to create 1 item.											Ex: 2 red = 1 deep red spray paint
		* - (1) using 1 of the deep item to create 1 item.											Ex: 1 deep red = 1 deep red spray paint
		* - (3) using 2 of the base spray paint to create 1 item.									Ex: 2 red spray paint = 1 deep red spray paint
		* 
		* For cycling spray paints:
		* - (1) using 1 of each base item to create 1 * the number of base items used.				Ex: 1 red + 1 orange + 1 yellow = 3 flame spray paint
		* - (4) using 1 of the base cycling paint to create 1 item.									Ex: 1 flame paint = 1 flame spray paint
		* - (5) using 1 of each base spray paint to create 1 * the number of base items used.		Ex: 1 red spray + 1 orange spray + 1 yellow spray = 3 flame spray paint
		* 
		* For deep cycling spray paints:
		* - (2) using 2 of each base item to create .5 * the number of base items used.				Ex: 2 red + 2 orange + 2 yellow = 3 deep flame spray paint
		* - (1) using 1 of each deep item to create 1 * the number of deep items used.				Ex: 1 deep red + 1 deep orange + 1 deep yellow = 3 deep flame spray paint
		* - (6a) using 2 of the base cycling paint to create 1 item.								Ex: 2 flame paint = 1 deep flame spray paint
		* - (6b) using 1 of the deep cycling paint to create 1 item.								Ex: 1 deep flame paint = 1 deep flame spray paint
		* - (6c) using 2 of each base spray paint to create .5 * the number of base items used.		Ex: 2 red spray + 2 orange spray + 2 yellow spray = 3 deep flame spray paint
		* - (5) using 1 of each deep spray paint to create 1 * the number of deep items used.		Ex: 1 deep red spray + 1 deep orange spray + 1 deep yellow spray = 3 deep flame spray paint
		* - (3) using 2 of the base cycling spray paint to create 1 item.							Ex: 2 flame spray paint = 1 deep flame spray paint
		*/
		public override void AddRecipes() {
			//any paint type that has _includeVanillaRecipes set to false should ignore recipes that use paintItemIds (public or protected)
			if(_paintItemIds.Length > 0) {
				//case (1)
				//using 1 of each base/deep item to create 1 * the number of items used.
				//matches all paints
				if(_includeVanillaRecipes) {
					//using the public paintItemIds so base custom paints use base items and deep custom paints use deep items
					ModRecipe recipe = new ModRecipe(mod);
					int[] itemIds = paintItemIds; //store it so it doesn't need to convert multiple times
					for(int p = 0; p < colorCount; p++)
						recipe.AddIngredient(itemIds[p], 1);
					recipe.AddTile(TileID.DyeVat);
					recipe.SetResult(this, colorCount);
					recipe.AddRecipe();
				}

				//case (2)
				//using 2 of each base item to create .5 * the number of items used.
				//matchs all deep paints
				if(_includeVanillaRecipes && this is IDeepPaint) {
					//using the protected _paintItemIds so only base items are used
					ModRecipe recipe = new ModRecipe(mod);
					for(int p = 0; p < _paintItemIds.Length; p++)
						recipe.AddIngredient(_paintItemIds[p], 2);
					recipe.AddTile(TileID.DyeVat);
					recipe.SetResult(this, colorCount);
					recipe.AddRecipe();
				}

				//case (3)
				//using 2 of the base custom paint to create 1 item
				//matches deep cycling, deep vanilla spray paints, and deep cycling spray paints
				if(this is IDeepPaint && (this is ICyclingPaint || this is ISprayPaint)) {
					ModRecipe recipe = new ModRecipe(mod); 
					Type t = GetType().BaseType;
					recipe.AddIngredient(mod.ItemType(t.Name), 2);
					recipe.AddTile(TileID.DyeVat);
					recipe.SetResult(this, 1);
					recipe.AddRecipe();
				}

				//case (4)
				//using 1 of the base cycling paint to create 1 item
				//matches cycling spray paints
				//same as case (3), but only takes 1 of the base item
				if(this is ISprayPaint && this is ICyclingPaint && !(this is IDeepPaint)) {
					ModRecipe recipe = new ModRecipe(mod);
					Type t = GetType().BaseType;
					recipe.AddIngredient(mod.ItemType(t.Name), 1);
					recipe.AddTile(TileID.DyeVat);
					recipe.SetResult(this, 1);
					recipe.AddRecipe();
				}

				//case (5)
				//using 1 of each base/deep spray paint to create 1 * the number of items used
				//matches cycling spray paints and deep cycling spray paints
				if(_includeVanillaRecipes && this is ISprayPaint && this is ICyclingPaint) {
					ModRecipe recipe = new ModRecipe(mod);
					int[] itemIds = paintItemIds; //store it so it doesn't need to convert multiple times for deep paints
					for(int p = 0; p < colorCount; p++)
						recipe.AddIngredient(mod.ItemType(ColorNames.list[Array.IndexOf(PaintItemID.list, itemIds[p])].Replace(" ", "") + "SprayPaint"), 1);
					recipe.AddTile(TileID.DyeVat);
					recipe.SetResult(this, colorCount);
					recipe.AddRecipe();
				}

				//case (6)
				//since the rest of the needed recipes all apply to only deep cycling spray paints, this case will be broken into separate parts
				//matches deep cycling spray paints
				if(this is IDeepPaint && this is ICyclingPaint && this is ISprayPaint) {
					//case (6a)
					//using 2 of the base cycling paint to create 1 deep cycling spray paint
					ModRecipe recipeA = new ModRecipe(mod);
					Type sprayType = GetType().BaseType;
					Type t = sprayType.BaseType;
					recipeA.AddIngredient(mod.ItemType(t.Name), 2);
					recipeA.AddTile(TileID.DyeVat);
					recipeA.SetResult(this, 1);
					recipeA.AddRecipe();

					//case (6b)
					//using 1 of the deep cycling paint to create 1 deep cycling spray paint
					ModRecipe recipeB = new ModRecipe(mod);
					recipeB.AddIngredient(mod.ItemType("Deep" + t.Name), 1);
					recipeB.AddTile(TileID.DyeVat);
					recipeB.SetResult(this, 1);
					recipeB.AddRecipe();

					//case (6c)
					//using 2 of the base spray paints to create .5 * the number of items used
					//using protected _paintItemIds to get only base paints
					if(_includeVanillaRecipes) {
						ModRecipe recipeC = new ModRecipe(mod);
						for(int p = 0; p < colorCount; p++)
							recipeC.AddIngredient(mod.ItemType("Deep" + ColorNames.list[Array.IndexOf(PaintItemID.list, _paintItemIds[p])].Replace(" ", "") + "SprayPaint"), 2);
						recipeC.AddTile(TileID.DyeVat);
						recipeC.SetResult(this, colorCount);
						recipeC.AddRecipe();
					}
				}
			}
		}

		/// <summary>
		/// Gets the color to use for lights and shaders based on the data provided.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public Color getColor(CustomPaintData data) {
			if(colorCount == 1)
				return getColorFromIndex(0);
			int index1 = getPaintIndex(data);
			int index2 = getPaintIndex(data, 1);
			if(index1 == index2)
				return getColorFromIndex(index1);
			float lerpAmount = ((Main.GlobalTime - data.timeOffset) / data.timeScale) % 1;
			return Color.Lerp(getColorFromIndex(index1), getColorFromIndex(index2), lerpAmount);
		}
		/// <summary>
		/// Gets the PaintID for painting tiles based on the data provided.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public byte getPaintID(CustomPaintData data) {
			if(!data.forceColor && this is ISprayPaint && Main.rand.NextFloat() <= .66f)
				return 0;
			if(colorCount == 1)
				return getPaintIDFromIndex(0);
			return getPaintIDFromIndex(getPaintIndex(data));
		}

		protected Color getColorFromIndex(int index) => PaintColors.list[getPaintIDFromIndex(index)];
		protected byte getPaintIDFromIndex(int index) => (byte)Array.IndexOf(PaintItemID.list, paintItemIds[index]);
		/// <summary>
		/// Gets the index of the paint item id that the custom paint is currently using. Base implementation is dependant on Main.global time with a time scale and offset factored in
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset">This can be used to offset the result. For example, to get the next index the paint will use the offset should be 1. This is useful for interpolating between the current and next color for the custom paint</param>
		/// <returns></returns>
		protected virtual int getPaintIndex(CustomPaintData data, int offset = 0) {
			int index;
			if(cycleLoops) {
				index = ((int)Math.Floor((Main.GlobalTime - data.timeOffset) / data.timeScale) + offset) % colorCount;
				if(index < 0)
					index += colorCount;
			} else {
				index = ((int)Math.Floor((Main.GlobalTime - data.timeOffset) / data.timeScale) + offset) % (colorCount * 2 - 2);
				if(index < 0)
					index *= -1;
				if(index >= colorCount)
					index = colorCount * 2 - 2 - index;
			}
			if(index < 0 || index >= colorCount) {
				throw new Exception("Error getting paint index for custom paint");
			}
			return index;
		}
	
		/// <summary>
		/// Allows a custom paint to convert itself to a vanilla paint color before applying it to an npc. This is useful for a custom paint that is dependent on the current player, where the color of the paint could change after the buff is applied in a way that is not desirable
		/// </summary>
		/// <param name="paintColor">If the custom paint converts itself to a vanilla paint, this will be the color's PaintID. Will be -1 if the paint is not converted</param>
		/// <param name="customPaint">If the custom paint is not converted, this will just be the same custom paint object</param>
		/// <param name="data"></param>
		public virtual void getPaintVarsForNpc(out int paintColor, out CustomPaint customPaint, CustomPaintData data) {
			paintColor = -1;
			customPaint = this;
		}
	}

	public class RainbowPaint : CustomPaint, ICyclingPaint {
		public override float paintConsumptionChance => .5f;
		public RainbowPaint() : base() {
			cycleLoops = true;
		}
		protected override int[] _paintItemIds => new int[] {
			ItemID.RedPaint,
			ItemID.OrangePaint,
			ItemID.YellowPaint,
			ItemID.LimePaint,
			ItemID.GreenPaint,
			ItemID.TealPaint,
			ItemID.CyanPaint,
			ItemID.SkyBluePaint,
			ItemID.BluePaint,
			ItemID.PurplePaint,
			ItemID.VioletPaint,
			ItemID.PinkPaint
		};
		protected override string _colorName => "Rainbow";
	}
	public class DeepRainbowPaint : RainbowPaint, IDeepPaint { }
	public class RainbowSprayPaint : RainbowPaint, ISprayPaint { }
	public class DeepRainbowSprayPaint : RainbowSprayPaint, IDeepPaint { }

	public class FlamePaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.RedPaint, ItemID.OrangePaint, ItemID.YellowPaint };
		protected override string _colorName => "Flame";
	}
	public class DeepFlamePaint : FlamePaint, IDeepPaint { }
	public class FlameSprayPaint : FlamePaint, ISprayPaint { }
	public class DeepFlameSprayPaint : FlameSprayPaint, IDeepPaint { }

	public class GreenFlamePaint : CustomPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.GreenPaint, ItemID.LimePaint, ItemID.YellowPaint };
		protected override string _colorName => "Green Flame";
	}
	public class DeepGreenFlamePaint : GreenFlamePaint, IDeepPaint { }
	public class GreenFlameSprayPaint : GreenFlamePaint, ISprayPaint { }
	public class DeepGreenFlameSprayPaint : GreenFlameSprayPaint, IDeepPaint { }

	public class BlueFlamePaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.BluePaint, ItemID.SkyBluePaint, ItemID.CyanPaint };
		protected override string _colorName => "Blue Flame";
	}
	public class DeepBlueFlamePaint : BlueFlamePaint, IDeepPaint { }
	public class BlueFlameSprayPaint : BlueFlamePaint, ISprayPaint { }
	public class DeepBlueFlameSprayPaint : BlueFlameSprayPaint, IDeepPaint { }

	public class YellowGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.LimePaint, ItemID.YellowPaint, ItemID.OrangePaint };
		protected override string _colorName => "Yellow Gradient";
	}
	public class DeepYellowGradientPaint : YellowGradientPaint, IDeepPaint { }
	public class YellowGradientSprayPaint : YellowGradientPaint, ISprayPaint { }
	public class DeepYellowGradientSprayPaint : YellowGradientSprayPaint, IDeepPaint { }

	public class CyanGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.TealPaint, ItemID.CyanPaint, ItemID.SkyBluePaint };
		protected override string _colorName => "Cyan Gradient";
	}
	public class DeepCyanGradientPaint : CyanGradientPaint, IDeepPaint { }
	public class CyanGradientSprayPaint : CyanGradientPaint, ISprayPaint { }
	public class DeepCyanGradientSprayPaint : CyanGradientSprayPaint, IDeepPaint { }

	public class VioletGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.PinkPaint, ItemID.VioletPaint, ItemID.PurplePaint };
		protected override string _colorName => "Violet Gradient";
	}
	public class DeepVioletGradientPaint : VioletGradientPaint, IDeepPaint { }
	public class VioletGradientSprayPaint : VioletGradientPaint, ISprayPaint { }
	public class DeepVioletGradientSprayPaint : VioletGradientSprayPaint, IDeepPaint { }

	public class GrayscalePaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.ShadowPaint, ItemID.BlackPaint, ItemID.GrayPaint, ItemID.WhitePaint };
		protected override string _colorName => "Grayscale";
	}
	public class GrayscaleSprayPaint : GrayscalePaint, ISprayPaint { }

	public class RedSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.RedPaint };
		protected override string _colorName => "Red";
	}
	public class DeepRedSprayPaint : RedSprayPaint, IDeepPaint { }

	public class OrangeSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.OrangePaint };
		protected override string _colorName => "Orange";
	}
	public class DeepOrangeSprayPaint : OrangeSprayPaint, IDeepPaint { }

	public class YellowSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.YellowPaint };
		protected override string _colorName => "Yellow";
	}
	public class DeepYellowSprayPaint : YellowSprayPaint, IDeepPaint { }

	public class LimeSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.LimePaint };
		protected override string _colorName => "Lime";
	}
	public class DeepLimeSprayPaint : LimeSprayPaint, IDeepPaint { }

	public class GreenSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.GreenPaint };
		protected override string _colorName => "Green";
	}
	public class DeepGreenSprayPaint : GreenSprayPaint, IDeepPaint { }

	public class TealSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.TealPaint };
		protected override string _colorName => "Teal";
	}
	public class DeepTealSprayPaint : TealSprayPaint, IDeepPaint { }

	public class CyanSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.CyanPaint };
		protected override string _colorName => "Cyan";
	}
	public class DeepCyanSprayPaint : CyanSprayPaint, IDeepPaint { }

	public class SkyBlueSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.SkyBluePaint };
		protected override string _colorName => "Sky Blue";
	}
	public class DeepSkyBlueSprayPaint : SkyBlueSprayPaint, IDeepPaint { }

	public class BlueSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.BluePaint };
		protected override string _colorName => "Blue";
	}
	public class DeepBlueSprayPaint : BlueSprayPaint, IDeepPaint { }

	public class PurpleSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.PurplePaint };
		protected override string _colorName => "Purple";
	}
	public class DeepPurpleSprayPaint : PurpleSprayPaint, IDeepPaint { }

	public class VioletSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.VioletPaint };
		protected override string _colorName => "Violet";
	}
	public class DeepVioletSprayPaint : VioletSprayPaint, IDeepPaint { }

	public class PinkSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.PinkPaint };
		protected override string _colorName => "Pink";
	}
	public class DeepPinkSprayPaint : PinkSprayPaint, IDeepPaint { }

	public class WhiteSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.WhitePaint };
		protected override string _colorName => "White";
	}
	public class GraySprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.GrayPaint };
		protected override string _colorName => "Gray";
	}
	public class BlackSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.BlackPaint };
		protected override string _colorName => "Black";
	}
	public class ShadowSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.ShadowPaint };
		protected override string _colorName => "Shadow";
	}
	public class BrownSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.BrownPaint };
		protected override string _colorName => "Brown";
	}
	public class NegativeSprayPaint : CustomPaint, ISprayPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.NegativePaint };
		protected override string _colorName => "Negative";
	}

	public class TeamPaint : CustomPaint, ICyclingPaint{
		protected override int[] _paintItemIds => new int[] { ItemID.WhitePaint, ItemID.RedPaint, ItemID.GreenPaint, ItemID.BluePaint, ItemID.YellowPaint,ItemID.PinkPaint};
		protected override string _colorName => "Team";
		protected override bool _includeVanillaRecipes => false;

		protected override int getPaintIndex(CustomPaintData data, int offset = 0) {
			if(data.player == null)
				return 0;
			int team = data.player.team;
			return team;
		}

		public override void getPaintVarsForNpc(out int paintColor, out CustomPaint customPaint, CustomPaintData data) {
			customPaint = null;
			paintColor = getPaintIDFromIndex(getPaintIndex(data));
		}
	}
	public class DeepTeamPaint : TeamPaint, IDeepPaint { }
	public class TeamSprayPaint : TeamPaint, ISprayPaint { }
	public class DeepTeamSprayPaint : TeamSprayPaint, IDeepPaint { }
}