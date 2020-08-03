namespace WeaponsOfMassDecoration {
	/// <summary>
	/// The different methods that can be used for painting
	/// </summary>
	public enum PaintMethods {
		/// <summary>
		/// The method when the player has no painting tools in their inventory
		/// </summary>
		None,
		/// <summary>
		/// The method used by paint scrapers
		/// </summary>
		RemovePaint,
		/// <summary>
		/// The method used by paintbrushes
		/// </summary>
		Blocks,
		/// <summary>
		/// The method used by paint rollers
		/// </summary>
		Walls,
		/// <summary>
		/// The method used by painting multi-tools
		/// </summary>
		BlocksAndWalls
	}
}
