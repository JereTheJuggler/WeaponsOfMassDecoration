using Microsoft.Xna.Framework;
using Terraria;
using WeaponsOfMassDecoration.Items;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	public class PaintData {
		protected int _paintColor = -1;
		public int paintColor {
			get { return _paintColor; }
			set {
				if(value == _paintColor)
					return;
				_renderColor = null;
				_paintColor = value;
			}
		}
		protected CustomPaint _customPaint = null;
		public CustomPaint customPaint {
			get { return _customPaint; }
			set {
				if((_customPaint == null) == (value == null) && (_customPaint == null || _customPaint.GetType().Equals(value.GetType())))
					return;
				_renderColor = null;
				_customPaint = value;
			}
		}

		public float timeOffset = 0;
		public float timeScale = 1f;
		public Player player = null;
		public PaintMethods paintMethod = PaintMethods.BlocksAndWalls;
		public bool blocksAllowed = true;
		public bool wallsAllowed = true;
		public bool consumePaint = true;
		public bool useWorldGen = false;
		public bool sprayPaint = false;
		private Color? _renderColor;
		public Color renderColor {
			get {
				if(_renderColor == null) {
					if((paintColor == -1 && customPaint == null) || paintMethod == PaintMethods.RemovePaint) {
						_renderColor = Color.White;
					} else if(paintColor != -1) {
						_renderColor = PaintColors.list[paintColor];
					} else {
						_renderColor = customPaint.getColor(this);
					}
				}
				return (Color)_renderColor;
			}
		}

		public PaintData(float timeScale = 1f, int paintColor = -1, CustomPaint customPaint = null, bool sprayPaint = false, float timeOffset = 0, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool useWorldGen = false, bool consumePaint = true) {
			this.paintColor = paintColor;
			this.player = player;
			this.customPaint = customPaint;
			this.timeOffset = timeOffset;
			this.timeScale = timeScale;
			this.player = player;
			this.blocksAllowed = blocksAllowed;
			this.wallsAllowed = wallsAllowed;
			this.consumePaint = consumePaint;
			this.useWorldGen = useWorldGen;
			this.paintMethod = paintMethod;
			this.sprayPaint = sprayPaint;
			_renderColor = null;
		}

		public PaintData(int paintColor, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool useWorldGen = false, bool consumePaint = true) :
					this(1f, paintColor, paintMethod: paintMethod, blocksAllowed: blocksAllowed, wallsAllowed: wallsAllowed, player: player, useWorldGen: useWorldGen, consumePaint: consumePaint) { }

		public PaintData(float timeScale, float timeOffset, CustomPaint customPaint, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool useWorldGen = false, bool consumePaint = true) :
					this(timeScale, default, customPaint, customPaint is ISprayPaint, timeOffset, paintMethod, blocksAllowed, wallsAllowed, player, useWorldGen, consumePaint) { }

		public PaintData(float timeScale, float timeOffset = 0) :
					this(timeScale, -1, null, timeOffset: timeOffset) { }

		public PaintData(PaintData data) :
					this(data.timeScale, data.paintColor, data.customPaint == null ? null : (CustomPaint)data.customPaint.Clone(), data.sprayPaint, data.timeOffset, data.paintMethod, data.blocksAllowed, data.wallsAllowed, data.player == null ? null : getPlayer(data.player.whoAmI), data.useWorldGen, data.consumePaint) { }
	}
}
