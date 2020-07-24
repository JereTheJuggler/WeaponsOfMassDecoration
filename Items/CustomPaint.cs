using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Items {
    public class PaintMethods {
        public const byte Loop = 0;
        public const byte Reverse = 1;
        public const byte Spray = 2;
        public const byte LoopSpray = 2;
        public const byte ReverseSpray = 3;
    }

    public abstract class CustomPaint : PaintingItem {
        public float consumptionChance;
        public abstract int[] paintIds {
            get;
        }
        public abstract byte paintMethod {
            get;
        }
        public abstract string colorName {
            get;
        }
        public virtual string displayName {
            get { return colorName + " Paint"; }
        }

        public virtual byte getColor(float timeScale, bool forceColor = false, float timeDelta = 0) {
            if(paintIds.Length == 0)
                return 0;
            int index = 0;
			if(!forceColor) {
				if(paintMethod == PaintMethods.ReverseSpray || paintMethod == PaintMethods.LoopSpray) {
					if(Main.rand.NextFloat(3f) < 2) {
						return 0;
					}
				}
			}
            switch(paintMethod) {
                case PaintMethods.Loop:
                case PaintMethods.LoopSpray:
                    index = (int)Math.Floor((Main.GlobalTime - timeDelta) / timeScale) % paintIds.Length;
                    break;
                case PaintMethods.Reverse:
                case PaintMethods.ReverseSpray:
                    index = (int)Math.Floor((Main.GlobalTime - timeDelta) / timeScale) % (paintIds.Length*2 - 2);
                    if(index >= paintIds.Length)
                        index = paintIds.Length * 2 - 2 - index;
                    break;
            }
            return getColorFromId(paintIds[index]);
        }
		public virtual byte getNextColor(float timeScale, bool forceColor = false, float timeDelta = 0) {
			if(paintIds.Length == 0)
				return 0;
			int index = 0;
			if(!forceColor) {
				if(paintMethod == PaintMethods.ReverseSpray || paintMethod == PaintMethods.LoopSpray) {
					if(Main.rand.NextFloat(3f) < 2) {
						return 0;
					}
				}
			}
			switch(paintMethod) {
				case PaintMethods.Loop:
				case PaintMethods.LoopSpray:
					index = (int)(Math.Floor((Main.GlobalTime - timeDelta) / timeScale)+1) % paintIds.Length;
					break;
				case PaintMethods.Reverse:
				case PaintMethods.ReverseSpray:
					index = (int)(Math.Floor((Main.GlobalTime - timeDelta) / timeScale)+1) % (paintIds.Length * 2 - 2);
					if(index >= paintIds.Length)
						index = paintIds.Length * 2 - 2 - index;
					break;
			}
			return getColorFromId(paintIds[index]);
		}

        public byte getColorFromId(int paintId) {
            for(byte i = 0; i < PaintIDs.itemIds.Length; i++) {
                if(PaintIDs.itemIds[i] == paintId)
                    return i;
            }
            return 0;
        }

        public CustomPaint() : base(){
            setConsumptionChance();
        }

        public virtual void setConsumptionChance() {
            consumptionChance = 1f;
        }
        
        public override void SetStaticDefaults() {
            DisplayName.SetDefault(displayName);
            SetStaticDefaults("", "");
        }

        public override void SetStaticDefaults(string preToolTip, string postToolTip) {
            Tooltip.SetDefault((preToolTip != "" ? preToolTip + ((consumptionChance < 1f || postToolTip != "") ? "\n" : "") : "") +
                (consumptionChance >= 1f ? postToolTip :((1f-consumptionChance)*100).ToString()+"% Chance to not be consumed" + (postToolTip != "" ? "\n" + postToolTip : "")));
        }

        public override void AddRecipes() {
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }
        }

        public override void SetDefaults() {
            item.noMelee = true;
            item.noUseGraphic = true;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 0, 0, 10);
            item.maxStack = 999;
            item.width = 20;
            item.height = 18;
        }
    }
    public abstract class DeepCustomPaint : CustomPaint {
        public abstract CustomPaint basePaint {
            get;
        }
        public override byte paintMethod {
            get {
                return basePaint.paintMethod;
            }
        }
        public override string colorName {
            get { return basePaint.colorName; }
        }
        public override string displayName {
            get { return "Deep " + colorName + " Paint"; }
        }

        public DeepCustomPaint() : base() {
        }
        
        public override void AddRecipes() {
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }

            if(basePaint != null) {
                ModRecipe recipe3 = new ModRecipe(mod);
                recipe3.AddIngredient(mod.ItemType(basePaint.GetType().Name), 2);
                recipe3.AddTile(TileID.DyeVat);
                recipe3.SetResult(this, 1);
                recipe3.AddRecipe();

                if(basePaint.paintIds.Length > 0) {
                    ModRecipe recipe2 = new ModRecipe(mod);
                    for(int p = 0; p < basePaint.paintIds.Length; p++) {
                        recipe2.AddIngredient(basePaint.paintIds[p], 2);
                    }
                    recipe2.AddTile(TileID.DyeVat);
                    recipe2.SetResult(this, basePaint.paintIds.Length);
                    recipe2.AddRecipe();
                }
            }
        }
    }
    public abstract class CustomSprayPaint : CustomPaint{
        public abstract CustomPaint basePaint {
            get;
        }
        public override byte paintMethod {
            get { return PaintMethods.ReverseSpray; }
        }
        public override int[] paintIds {
            get { return basePaint.paintIds; }
        }
        public override string colorName {
            get { return basePaint.colorName; }
        }
        public override string displayName {
            get { return colorName + " Spray Paint"; }
        }

        public CustomSprayPaint() {
        }

		public override void SetDefaults() {
			base.SetDefaults();
            item.width = 16;
            item.height = 32;
		}

		public override void AddRecipes() {
            //add recipe using the vanilla paints (ex. 3 flame spray paint = 1 red, orange yellow paint)
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }
            //add recipe using the base custom paint (ex. 1 flame spray paint = 1 flame paint)
            if(basePaint != null) {
                ModRecipe recipe2 = new ModRecipe(mod);
                recipe2.AddIngredient(mod.ItemType(basePaint.GetType().Name), 1);
                recipe2.AddTile(TileID.DyeVat);
                recipe2.SetResult(this, 1);
                recipe2.AddRecipe();
            }
        }
    }
    public abstract class DeepCustomSprayPaint : CustomSprayPaint {
        public abstract CustomSprayPaint baseSprayPaint {
            get;
        }
        public abstract DeepCustomPaint baseDeepPaint {
            get;
        }
        public override CustomPaint basePaint {
            get { return baseDeepPaint; }
        }
        public override string colorName {
            get { return basePaint.colorName; }
        }
        public override string displayName {
            get { return "Deep " + colorName + " Spray Paint"; }
        }
        public override int[] paintIds {
            get { return baseDeepPaint.paintIds; }
        }

        public DeepCustomSprayPaint(){
        }
        
        public override void AddRecipes() {
            //add recipe using 1 of each deep paint (ex. 3 Deep flame spray paint = 1 Deep red, orange, yellow paint)
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }

            //add recipe using 1 of the base deep custom paint (ex. 1 Deep flame spray paint = 1 Deep flame paint)
            if(baseDeepPaint != null) {
                ModRecipe recipe2 = new ModRecipe(mod);
                recipe2.AddIngredient(mod.ItemType(baseDeepPaint.GetType().Name), 1);
                recipe2.AddTile(TileID.DyeVat);
                recipe2.SetResult(this, 1);
                recipe2.AddRecipe();
                
                //add recipe using 2 of the deep paint's base (ex. 1 Deep flame spray paint = 2 flame paint)
                if(baseDeepPaint.basePaint != null) {
                    ModRecipe recipe4 = new ModRecipe(mod);
                    recipe4.AddIngredient(mod.ItemType(baseDeepPaint.basePaint.GetType().Name), 2);
                    recipe4.AddTile(TileID.DyeVat);
                    recipe4.SetResult(this, 1);
                    recipe4.AddRecipe();

                    //add recipe using the most basic ingredients (ex. 3 Deep flame spray paint = 2 red, orange, yellow paints)
                    if(baseDeepPaint.basePaint.paintIds.Length > 0) {
                        ModRecipe recipe5 = new ModRecipe(mod);
                        for(int p = 0; p < baseDeepPaint.basePaint.paintIds.Length; p++) {
                            recipe5.AddIngredient(baseDeepPaint.basePaint.paintIds[p], 2);
                        }
                        recipe5.AddTile(TileID.DyeVat);
                        recipe5.SetResult(this, baseDeepPaint.basePaint.paintIds.Length);
                        recipe5.AddRecipe();
                    }
                }
            }
            
            //add recipe using 2 of the base spray paints (ex. 1 Deep flame spray paint = 2 Flame spray paint
            if(baseSprayPaint != null) {
                ModRecipe recipe3 = new ModRecipe(mod);
                recipe3.AddIngredient(mod.ItemType(baseSprayPaint.GetType().Name), 2);
                recipe3.AddTile(TileID.DyeVat);
                recipe3.SetResult(this, 1);
                recipe3.AddRecipe();
            }

        }
    }
    public abstract class VanillaSprayPaint : CustomPaint {
        public override string displayName {
            get { return colorName + " Spray Paint"; }
        }
        public override byte paintMethod {
            get { return PaintMethods.LoopSpray; }
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.width = 16;
            item.height = 32;
        }

        public override void AddRecipes() {
            //add recipe using the most basic ingredients (ex. 1 orange spray paint = 1 orange paint)
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }
        }
    }
    public abstract class DeepVanillaSprayPaint : VanillaSprayPaint {
        public override string displayName {
            get { return "Deep " + colorName + " Spray Paint"; }
        }
        public override string colorName {
            get { return baseSprayPaint.colorName; }
        }
        public abstract VanillaSprayPaint baseSprayPaint {
            get;
        }

        public override void AddRecipes() {
            //add recipe using the vanilla deep paints (ex. 1 deep yellow spray paint = 1 deep yellow paint)
            if(paintIds.Length > 0) {
                ModRecipe recipe = new ModRecipe(mod);
                for(int p = 0; p < paintIds.Length; p++) {
                    recipe.AddIngredient(paintIds[p], 1);
                }
                recipe.AddTile(TileID.DyeVat);
                recipe.SetResult(this, paintIds.Length);
                recipe.AddRecipe();
            }

            //add recipe using 2 of the base spray paint (ex. 1 deep yellow spray paint = 2 yellow spray paint)
            if(baseSprayPaint != null) {
                ModRecipe recipe2 = new ModRecipe(mod);
                recipe2.AddIngredient(mod.ItemType(baseSprayPaint.GetType().Name), 2);
                recipe2.AddTile(TileID.DyeVat);
                recipe2.SetResult(this, 1);
                recipe2.AddRecipe();

                //add recipe using the most basic ingredients (ex. 1 deep yellow spray paint = 2 yellow paint)
                if(baseSprayPaint.paintIds.Length > 0) {
                    ModRecipe recipe3 = new ModRecipe(mod);
                    for(int p = 0; p < baseSprayPaint.paintIds.Length; p++) {
                        recipe3.AddIngredient(baseSprayPaint.paintIds[p], 2);
                    }
                    recipe3.AddTile(TileID.DyeVat);
                    recipe3.SetResult(this, baseSprayPaint.paintIds.Length);
                    recipe3.AddRecipe();
                }
            }
        }
    }

    public class RainbowPaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
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
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Loop; }
        }
        public override string colorName {
            get { return "Rainbow"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
        
        public override void setConsumptionChance() {
            consumptionChance = .5f;
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.value = Item.buyPrice(0, 0, 0, 10);
        }
    }
    public class DeepRainbowPaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepRedPaint,
                    ItemID.DeepOrangePaint,
                    ItemID.DeepYellowPaint,
                    ItemID.DeepLimePaint,
                    ItemID.DeepGreenPaint,
                    ItemID.DeepTealPaint,
                    ItemID.DeepCyanPaint,
                    ItemID.DeepSkyBluePaint,
                    ItemID.DeepBluePaint,
                    ItemID.DeepPurplePaint,
                    ItemID.DeepVioletPaint,
                    ItemID.DeepPinkPaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new RainbowPaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.value = Item.buyPrice(0, 0, 0, 20);
        }
    }
    public class RainbowSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new RainbowPaint(); }
        }
        public override byte paintMethod {
            get { return PaintMethods.LoopSpray; }
        }
    }
    public class DeepRainbowSprayPaint : DeepCustomSprayPaint {
        public override CustomSprayPaint baseSprayPaint {
            get { return new RainbowSprayPaint(); }
        }
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepRainbowPaint(); }
        }
        public override byte paintMethod {
            get { return PaintMethods.LoopSpray; }
        }
    }

    public class FlamePaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.RedPaint,
                    ItemID.OrangePaint,
                    ItemID.YellowPaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Flame"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("","");
        }
    }
    public class DeepFlamePaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepRedPaint,
                    ItemID.DeepOrangePaint,
                    ItemID.DeepYellowPaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new FlamePaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class FlameSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new FlamePaint(); }
        }

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flame Spray Paint");
            base.SetStaticDefaults("", "");
        }
    }
    public class DeepFlameSprayPaint : DeepCustomSprayPaint {
        public override CustomSprayPaint baseSprayPaint {
            get { return new FlameSprayPaint(); }
        }
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepFlamePaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }

    public class GreenFlamePaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.GreenPaint,
                    ItemID.LimePaint,
                    ItemID.YellowPaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Green Flame"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class DeepGreenFlamePaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepGreenPaint,
                    ItemID.DeepLimePaint,
                    ItemID.DeepYellowPaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new GreenFlamePaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("","");
        }
    }
    public class GreenFlameSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new GreenFlamePaint(); }
        }
    }
    public class DeepGreenFlameSprayPaint : DeepCustomSprayPaint {
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepGreenFlamePaint(); }
        }
        public override CustomSprayPaint baseSprayPaint {
            get { return new GreenFlameSprayPaint(); }
        }
    }

    public class BlueFlamePaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.BluePaint,
                    ItemID.SkyBluePaint,
                    ItemID.CyanPaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Blue Flame"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("","");
        }
    }
    public class DeepBlueFlamePaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepBluePaint,
                    ItemID.DeepSkyBluePaint,
                    ItemID.DeepCyanPaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new BlueFlamePaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("","");
        }
    }
    public class BlueFlameSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new BlueFlamePaint(); }
        }
    }
    public class DeepBlueFlameSprayPaint : DeepCustomSprayPaint {
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepBlueFlamePaint(); }
        }
        public override CustomSprayPaint baseSprayPaint {
            get { return new BlueFlameSprayPaint(); }
        }
    }

    public class YellowGradientPaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.LimePaint,
                    ItemID.YellowPaint,
                    ItemID.OrangePaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Yellow Gradient"; }
        }

        public override void SetStaticDefaults() { 
            base.SetStaticDefaults("", "");
        }
    }
    public class DeepYellowGradientPaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepLimePaint,
                    ItemID.DeepYellowPaint,
                    ItemID.DeepOrangePaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new YellowGradientPaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class YellowGradientSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new YellowGradientPaint(); }
        }
    }
    public class DeepYellowGradientSprayPaint : DeepCustomSprayPaint {
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepYellowGradientPaint(); }
        }
        public override CustomSprayPaint baseSprayPaint {
            get { return new YellowGradientSprayPaint(); }
        }
    }

    public class CyanGradientPaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.TealPaint,
                    ItemID.CyanPaint,
                    ItemID.SkyBluePaint
                }; 
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Cyan Gradient"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class DeepCyanGradientPaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepTealPaint,
                    ItemID.DeepCyanPaint,
                    ItemID.DeepSkyBluePaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new CyanGradientPaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class CyanGradientSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new CyanGradientPaint(); }
        }
    }
    public class DeepCyanGradientSprayPaint : DeepCustomSprayPaint {
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepCyanGradientPaint(); }
        }
        public override CustomSprayPaint baseSprayPaint {
            get { return new CyanGradientSprayPaint(); }
        }
    }

    public class VioletGradientPaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.PinkPaint,
                    ItemID.VioletPaint,
                    ItemID.PurplePaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Violet Gradient"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class DeepVioletGradientPaint : DeepCustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.DeepPinkPaint,
                    ItemID.DeepVioletPaint,
                    ItemID.DeepPurplePaint
                };
            }
        }
        public override CustomPaint basePaint {
            get { return new VioletGradientPaint(); }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class VioletGradientSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new VioletGradientPaint(); }
        }
    }
    public class DeepVioletGradientSprayPaint : DeepCustomSprayPaint {
        public override DeepCustomPaint baseDeepPaint {
            get { return new DeepVioletGradientPaint(); }
        }
        public override CustomSprayPaint baseSprayPaint {
            get { return new VioletGradientSprayPaint(); }
        }
    }

    public class GrayscalePaint : CustomPaint {
        public override int[] paintIds {
            get {
                return new int[] {
                    ItemID.ShadowPaint,
                    ItemID.BlackPaint,
                    ItemID.GrayPaint,
                    ItemID.WhitePaint
                };
            }
        }
        public override byte paintMethod {
            get { return PaintMethods.Reverse; }
        }
        public override string colorName {
            get { return "Grayscale"; }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults("", "");
        }
    }
    public class GrayscaleSprayPaint : CustomSprayPaint {
        public override CustomPaint basePaint {
            get { return new GrayscalePaint(); }
        }
    }
    
    public class RedSprayPaint : VanillaSprayPaint {
        public override string colorName {
            get { return "Red"; }
        }
        public override int[] paintIds {
            get { return new int[] { ItemID.RedPaint }; }
        }
    }
    public class DeepRedSprayPaint : DeepVanillaSprayPaint {
        public override VanillaSprayPaint baseSprayPaint {
            get { return new RedSprayPaint(); }
        }
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepRedPaint }; }
        }
    }

    public class OrangeSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.OrangePaint }; }
        }
        public override string colorName {
            get { return "Orange"; }
        }
    }
    public class DeepOrangeSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepOrangePaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new OrangeSprayPaint(); }
        }
    }

    public class YellowSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.YellowPaint }; }
        }
        public override string colorName {
            get { return "Yellow"; }
        }
    }
    public class DeepYellowSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepYellowPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new YellowSprayPaint(); }
        }
    }

    public class LimeSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.LimePaint }; }
        }
        public override string colorName {
            get { return "Lime"; }
        }
    }
    public class DeepLimeSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepLimePaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new LimeSprayPaint(); }
        }
    }

    public class GreenSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.GreenPaint }; }
        }
        public override string colorName {
            get { return "Green"; }
        }
    }
    public class DeepGreenSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepGreenPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new GreenSprayPaint(); }
        }
    }

    public class TealSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.TealPaint }; }
        }
        public override string colorName {
            get { return "Teal"; }
        }
    }
    public class DeepTealSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepTealPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new TealSprayPaint(); }
        }
    }

    public class CyanSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.CyanPaint }; }
        }
        public override string colorName {
            get { return "Cyan"; }
        }
    }
    public class DeepCyanSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepCyanPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new CyanSprayPaint(); }
        }
    }

    public class BlueSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.BluePaint }; }
        }
        public override string colorName {
            get { return "Blue"; }
        }
    }
    public class DeepBlueSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepBluePaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new BlueSprayPaint(); }
        }
    }

    public class PurpleSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.PurplePaint }; }
        }
        public override string colorName {
            get { return "Purple"; }
        }
    }
    public class DeepPurpleSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepPurplePaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new PurpleSprayPaint(); }
        }
    }

    public class VioletSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.VioletPaint }; }
        }
        public override string colorName {
            get { return "Violet"; }
        }
    }
    public class DeepVioletSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepVioletPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new VioletSprayPaint(); }
        }
    }

    public class PinkSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.PinkPaint }; }
        }
        public override string colorName {
            get { return "Pink"; }
        }
    }
    public class DeepPinkSprayPaint : DeepVanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.DeepPinkPaint }; }
        }
        public override VanillaSprayPaint baseSprayPaint {
            get { return new PinkSprayPaint(); }
        }
    }

    public class WhiteSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.WhitePaint }; }
        }
        public override string colorName {
            get { return "White"; }
        }
    }

    public class BlackSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.BlackPaint }; }
        }
        public override string colorName {
            get { return "Black"; }
        }
    }

    public class GraySprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.GrayPaint }; }
        }
        public override string colorName {
            get { return "Gray"; }
        }
    }

    public class BrownSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.BrownPaint }; }
        }
        public override string colorName {
            get { return "Brown"; }
        }
    }

    public class ShadowSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.ShadowPaint }; }
        }
        public override string colorName {
            get { return "Shadow"; }
        }
    }

    public class NegativeSprayPaint : VanillaSprayPaint {
        public override int[] paintIds {
            get { return new int[] { ItemID.NegativePaint }; }
        }
        public override string colorName {
            get { return "Negative"; }
        }
    }
}
