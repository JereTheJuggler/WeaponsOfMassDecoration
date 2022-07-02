using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaponsOfMassDecoration {
	internal interface IPaintable {
		/// <summary>
		/// Whether or not the entity is currently painted
		/// </summary>
		public bool Painted { get; }

		/// <summary>
		/// The data used for painting with and rendering this entity
		/// </summary>
		public PaintData PaintData { get; }

		/// <summary>
		/// Removes paint from this entity
		/// </summary>
		public void RemovePaint();
	}
}
