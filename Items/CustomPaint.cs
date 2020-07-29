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

namespace WeaponsOfMassDecoration.Items {
	public interface ISprayPaint { }

	public interface IDeepPaint { }

	public interface ICyclingPaint { }

	public struct CustomPaintData {
		public float timeScale;
		public float timeOffset;
		public Player player;
		public bool forceColor;

		public CustomPaintData(bool forceColor, float timeScale, float timeOffset = 0, Player player = null) {
			this.forceColor = forceColor;
			this.timeScale = timeScale;
			this.timeOffset = timeOffset;
			this.player = player;
		}

		public CustomPaintData(bool forceColor, float timeScale, Player player) : this(forceColor, timeScale, default, player) { }
	}

	public abstract class CustomPaint : PaintingItem {
		public float consumptionChance = 1f;

		public int colorCount {
			get {
				return _paintItemIds.Length;
			}
		}
		protected abstract int[] _paintItemIds {
			get;
		}
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
			int index = Array.IndexOf(PaintIDs.itemIds, type);
			if(index == -1)
				return type;
			if(index >= PaintID.Red && index <= PaintID.Pink)
				return PaintIDs.itemIds[index + 12];
			return type;
		}

		protected abstract string _colorName {
			get;
		}
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

		public CustomPaint() : base() {

		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault(displayName);
			SetStaticDefaults("", "");
		}

		public override void SetStaticDefaults(string preToolTip, string postToolTip) {
			Tooltip.SetDefault((preToolTip != "" ? preToolTip + ((consumptionChance < 1f || postToolTip != "") ? "\n" : "") : "") +
				(consumptionChance >= 1f ? postToolTip : ((1f - consumptionChance) * 100).ToString() + "% Chance to not be consumed" + (postToolTip != "" ? "\n" + postToolTip : "")));
		}

		public override void AddRecipes() {
			if(_paintItemIds.Length > 0) {
				ModRecipe recipe = new ModRecipe(mod);
				for(int p = 0; p < paintItemIds.Length; p++)
					recipe.AddIngredient(paintItemIds[p], 1);
				recipe.AddTile(TileID.DyeVat);
				recipe.SetResult(this, paintItemIds.Length);
				recipe.AddRecipe();

				if(this is IDeepPaint) {
					ModRecipe deepRecipe = new ModRecipe(mod);
					for(int p = 0; p < _paintItemIds.Length; p++)
						deepRecipe.AddIngredient(_paintItemIds[p], 2);
					deepRecipe.AddTile(TileID.DyeVat);
					deepRecipe.SetResult(this, _paintItemIds.Length);
					deepRecipe.AddRecipe();

					if(this is ICyclingPaint) {
						ModRecipe deepCycleRecipe = new ModRecipe(mod);
						Type t = GetType().BaseType;
						deepCycleRecipe.AddIngredient(mod.ItemType(t.Name), 2);
						deepCycleRecipe.AddTile(TileID.DyeVat);
						deepCycleRecipe.SetResult(this, 1);
						deepCycleRecipe.AddRecipe();
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

		protected Color getColorFromIndex(int index) => PaintColors.colors[getPaintIDFromIndex(index)];
		protected byte getPaintIDFromIndex(int index) => (byte)Array.IndexOf(PaintIDs.itemIds, paintItemIds[index]);
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
		/// Allows a custom paint to convert itself to a vanilla paint color before applying it to an npc
		/// </summary>
		/// <param name="paintColor"></param>
		/// <param name="customPaint"></param>
		/// <param name="data"></param>
		public virtual void getPaintVarsForNpc(out int paintColor, out CustomPaint customPaint, CustomPaintData data) {
			paintColor = -1;
			customPaint = this;
		}
	}

	public class RainbowPaint : CustomPaint, ICyclingPaint {
		public RainbowPaint() : base() {
			cycleLoops = true;
			consumptionChance = .5f;
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
	public class DeepRainbowSprayPaint : DeepRainbowPaint, ISprayPaint { }

	public class FlamePaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.RedPaint, ItemID.OrangePaint, ItemID.YellowPaint };
		protected override string _colorName => "Flame";
	}
	public class DeepFlamePaint : FlamePaint, IDeepPaint { }
	public class FlameSprayPaint : FlamePaint, ISprayPaint { }
	public class DeepFlameSprayPaint : DeepFlamePaint, ISprayPaint { }

	public class GreenFlamePaint : CustomPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.GreenPaint, ItemID.LimePaint, ItemID.YellowPaint };
		protected override string _colorName => "Green Flame";
	}
	public class DeepGreenFlamePaint : GreenFlamePaint, IDeepPaint { }
	public class GreenFlameSprayPaint : GreenFlamePaint, ISprayPaint { }
	public class DeepGreenFlameSprayPaint : DeepGreenFlamePaint, ISprayPaint { }

	public class BlueFlamePaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.BluePaint, ItemID.SkyBluePaint, ItemID.CyanPaint };
		protected override string _colorName => "Blue Flame";
	}
	public class DeepBlueFlamePaint : BlueFlamePaint, IDeepPaint { }
	public class BlueFlameSprayPaint : BlueFlamePaint, ISprayPaint { }
	public class DeepBlueFlameSprayPaint : DeepBlueFlamePaint, ISprayPaint { }

	public class YellowGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.LimePaint, ItemID.YellowPaint, ItemID.OrangePaint };
		protected override string _colorName => "Yellow Gradient";
	}
	public class DeepYellowGradientPaint : YellowGradientPaint, IDeepPaint { }
	public class YellowGradientSprayPaint : YellowGradientPaint, ISprayPaint { }
	public class DeepYellowGradientSprayPaint : DeepYellowGradientPaint, ISprayPaint { }

	public class CyanGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.TealPaint, ItemID.CyanPaint, ItemID.SkyBluePaint };
		protected override string _colorName => "Cyan Gradient";
	}
	public class DeepCyanGradientPaint : CyanGradientPaint, IDeepPaint { }
	public class CyanGradientSprayPaint : CyanGradientPaint, ISprayPaint { }
	public class DeepCyanGradientSprayPaint : DeepCyanGradientPaint, ISprayPaint { }

	public class VioletGradientPaint : CustomPaint, ICyclingPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.PinkPaint, ItemID.VioletPaint, ItemID.PurplePaint };
		protected override string _colorName => "Violet Gradient";
	}
	public class DeepVioletGradientPaint : VioletGradientPaint, IDeepPaint { }
	public class VioletGradientSprayPaint : VioletGradientPaint, ISprayPaint { }
	public class DeepVioletGradientSprayPaint : DeepVioletGradientPaint, ISprayPaint { }

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

	public class TeamPaint : CustomPaint {
		protected override int[] _paintItemIds => new int[] { ItemID.WhitePaint, ItemID.RedPaint, ItemID.GreenPaint, ItemID.BluePaint, ItemID.YellowPaint,ItemID.PinkPaint};
		protected override string _colorName => "Team";

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
	public class DeepTeamSprayPaint : DeepTeamPaint, ISprayPaint { }
}
