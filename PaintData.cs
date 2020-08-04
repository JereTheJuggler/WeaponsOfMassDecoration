using Microsoft.Xna.Framework;
using Terraria;
using WeaponsOfMassDecoration.Items;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	public class PaintData {
		protected int _paintColor = -1;
		public int paintColor {
			get => _paintColor;
			set {
				if(value == _paintColor)
					return;
				_renderColor = null;
				_paintColor = value;
				_customPaintColor = null;
			}
		}
		protected CustomPaint _customPaint = null;
		public CustomPaint customPaint {
			get => _customPaint;
			set {
				if((_customPaint == null) == (value == null) && (_customPaint == null || _customPaint.GetType().Equals(value.GetType())))
					return;
				_renderColor = null;
				_customPaint = value;
				_customPaintColor = null;
			}
		}

		protected float _timeOffset = 0;
		public float timeOffset {
			get => _timeOffset;
			set {
				if(value == _timeOffset)
					return;
				if(_customPaint != null) {
					//timeOffset has no effect on vanilla paints, so save some performance and don't make these recalculate
					_renderColor = null;
					_customPaintColor = null;
				}
				_timeOffset = value;
			}
		}
		protected float _timeScale = 1f;
		public float timeScale {
			get => _timeScale; 
			set {
				if(value == _timeScale)
					return;
				if(_customPaint != null) { 
					//timeScale has no effect on vanilla paints, so save some performance and don't make these recalculate
					_renderColor = null;
					_customPaintColor = null;
				}
				_timeScale = value;
			}
		}
		public Player player = null;
		public PaintMethods paintMethod = PaintMethods.BlocksAndWalls;
		public bool blocksAllowed = true;
		public bool wallsAllowed = true;
		public bool consumePaint = true;
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
		private int? _customPaintColor = null;
		public int customPaintColor {
			get {
				if(_customPaintColor == null) {
					if(_customPaint == null) {
						_customPaintColor = -1;
					} else {
						_customPaintColor = _customPaint.getPaintID(this);
					}
				}
				return (int)_customPaintColor;
			}
		}

		public PaintData(float timeScale = 1f, int paintColor = -1, CustomPaint customPaint = null, bool sprayPaint = false, float timeOffset = 0, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool consumePaint = true) {
			_paintColor = paintColor;
			_customPaint = customPaint;
			_timeOffset = timeOffset;
			_timeScale = timeScale;
			this.player = player;
			this.blocksAllowed = blocksAllowed;
			this.wallsAllowed = wallsAllowed;
			this.consumePaint = consumePaint;
			this.paintMethod = paintMethod;
			this.sprayPaint = sprayPaint;
			_renderColor = null;
			_customPaintColor = null;
		}

		public PaintData(int paintColor, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool consumePaint = true) :
					this(1f, paintColor, paintMethod: paintMethod, blocksAllowed: blocksAllowed, wallsAllowed: wallsAllowed, player: player, consumePaint: consumePaint) { }

		public PaintData(float timeScale, float timeOffset, CustomPaint customPaint, PaintMethods paintMethod = PaintMethods.BlocksAndWalls, bool blocksAllowed = true, bool wallsAllowed = true, Player player = null, bool consumePaint = true) :
					this(timeScale, default, customPaint, customPaint is ISprayPaint, timeOffset, paintMethod, blocksAllowed, wallsAllowed, player, consumePaint) { }

		public PaintData(float timeScale, float timeOffset = 0) :
					this(timeScale, -1, null, timeOffset: timeOffset) { }

		public PaintData clone() {
			PaintData data = new PaintData();
			data._paintColor = _paintColor;
			data._customPaint = _customPaint;
			data._renderColor = _renderColor;
			data._customPaintColor = _customPaintColor;
			data._timeScale = _timeScale;
			data._timeOffset = _timeOffset;
			data.player = player;
			data.paintMethod = paintMethod;
			data.blocksAllowed = blocksAllowed;
			data.wallsAllowed = wallsAllowed;
			data.consumePaint = consumePaint;
			data.sprayPaint = sprayPaint;
			return data;
		}
	}
}
