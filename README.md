# WeaponsOfMassDecoration
This is a mod for Terraria, made using the tModLoader library

Here is a direct download link to the mod: http://javid.ddns.net/tModLoader/download.php?Down=mods/WeaponsOfMassDecoration.tmod

It's also available through the Mod Browser

Here is my post on the Terraria Community Forums explaining all about the mod: https://forums.terraria.org/index.php?threads/weapons-of-mass-decoration.64331/

Noteworthy files in this repository:
- [CustomPaint.cs](Items/CustomPaint.cs) - Defines all the different types of paint added by the mod. The base CustomPaint class handles the functionality of all of the deriving classes (with a few small exceptions)
- [PaintingProjectile.cs](PaintingProjectile.cs) - The base class for all of the projectiles added by the mod. Handles all the drawing for custom projectiles, as well as a few generic methods that are commonly used in deriving classes
- [PaintUtils.cs](PaintUtils.cs) - All of the painting in the mod is funneled through the functions in this class
- [WoMDNPC.cs](NPCs/WoMDNPC.cs) - Handles applying shaders to npcs. Various chaos mode functionality
- [WoMDProjectile](Projectiles/WoMDProjectile.cs) - Handles applying shaders to projectiles. Various chaos mode functionality
- [WoMDItem](Items/WoMDItem.cs) - Handles replacing the functionality and drawing the vanilla painting tools
- [PaintingItem](Items/PaintingItem.cs) - Handles the drawing for all custom items added by the mod
