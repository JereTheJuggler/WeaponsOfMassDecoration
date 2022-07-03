using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaponsOfMassDecoration.NPCs;

namespace WeaponsOfMassDecoration.Buffs {
	public class PaintBuffConfig {
		protected Dictionary<byte, bool> _enabledColors;

		public PaintBuffConfig() {
			_enabledColors = new();
		}
		public PaintBuffConfig(IEnumerable<byte> enabledColors) {
			LoadEnabledColors(enabledColors);
		}

		public bool IsColorEnabled(byte paintId) {
			return _enabledColors.ContainsKey(paintId);
		}

		public void SetColorEnabled(byte paintId, bool enabled) {
			if (enabled) {
				if (!_enabledColors.ContainsKey(paintId))
					_enabledColors.Add(paintId, true);
			} else {
				if (_enabledColors.ContainsKey(paintId))
					_enabledColors.Remove(paintId);
			}
		}
	
		public byte[] GetEnabledColors() {
			return _enabledColors.Keys.ToArray();
		}

		public void LoadEnabledColors(IEnumerable<byte> enabledColors) {
			_enabledColors = new();
			foreach (byte paintId in enabledColors)
				_enabledColors.Add(paintId, true);
		}
	}
}
