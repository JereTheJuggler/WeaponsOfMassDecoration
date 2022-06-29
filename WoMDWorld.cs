using IL.Terraria;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace WeaponsOfMassDecoration {
	public class WoMDWorld : ModSystem{
		protected List<PaintAnimation> animations;

		public WoMDWorld() : base(){
			animations = new List<PaintAnimation>();
		}

		public override void PostUpdateWorld() {
			int i = 0;
			while(i < animations.Count) {
				if(!animations[i].run()) {
					animations.RemoveAt(i);
				} else {
					i++;
				}
			}
		}

		public void addAnimation(PaintAnimation animation) {
			animations.Add(animation);
		}
	}
}
