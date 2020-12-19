using Microsoft.Xna.Framework;
using Terraria;
using WeaponsOfMassDecoration.Items;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration {
	public class PaintData {
		protected int _paintColor = -1;
		public int PaintColor {
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
		public CustomPaint CustomPaint {
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
		public float TimeOffset {
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
		public float TimeScale {
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
		public Color RenderColor {
			get {
				validateCachedProperties();
				if(_renderColor == null) {
					if((_paintColor == -1 && _customPaint == null) || paintMethod == PaintMethods.RemovePaint) {
						_renderColor = Color.White;
					} else if(_paintColor != -1) {
						_renderColor = PaintColors.list[_paintColor];
					} else {
						_renderColor = _customPaint.getColor(this);
					}
				}
				return (Color)_renderColor;
			}
		}
		private int? _customPaintColor = null;
		public int CustomPaintColor {
			get {
				validateCachedProperties();
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
		
		private uint lastUpdateTick = 0;
		private void validateCachedProperties() {
			if(Main.GameUpdateCount != lastUpdateTick) {
				_customPaintColor = null;
				_renderColor = null;
				lastUpdateTick = Main.GameUpdateCount;
			}
		}

		/// <summary>
		/// The current paint color for this set of data, whether it comes from a vanilla paint or a custom paint. Paint method and spray paint do not factor in here.
		/// </summary>
		public byte TruePaintColor {
			get {
				if(_paintColor == -1 && _customPaint == null)
					return 0;
				if(_paintColor != -1)
					return (byte)_paintColor;
				return (byte)CustomPaintColor;
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
