using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaponsOfMassDecoration {

	public delegate bool PaintAnimationDelegate(int index);

	public class PaintAnimation {
		protected PaintAnimationDelegate function;
		protected int index;
		protected int counter;
		protected int maxIndex;
		protected byte delay;

		public PaintAnimation(int maxIndex, byte delay, PaintAnimationDelegate function) {
			this.function = function;
			index = 0;
			this.maxIndex = maxIndex;
			this.delay = delay;
			counter = delay;
		}

		public bool Run() {
			counter--;
			if(counter <= 0) {
				counter = delay;
				bool result = function(index);
				index++;
				if(!result || index > maxIndex)
					return false;
			}
			return true;
		}
	}
}
