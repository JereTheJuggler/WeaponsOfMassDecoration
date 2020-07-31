using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using WeaponsOfMassDecoration.Items;
using System.Collections.Generic;

namespace WeaponsOfMassDecoration {
    public class PaintIDs {
        public const byte None = 0;
        public const byte Red = 1;
        public const byte Orange = 2;
        public const byte Yellow = 3;
        public const byte Lime = 4;
        public const byte Green = 5;
        public const byte Teal = 6;
        public const byte Cyan = 7;
        public const byte SkyBlue = 8;
        public const byte Blue = 9;
        public const byte Purple = 10;
        public const byte Violet = 11;
        public const byte Pink = 12;
        public const byte DeepRed = 13;
        public const byte DeepOrange = 14;
        public const byte DeepYellow = 15;
        public const byte DeepLime = 16;
        public const byte DeepGreen = 17;
        public const byte DeepTeal = 18;
        public const byte DeepCyan = 19;
        public const byte DeepSkyBlue = 20;
        public const byte DeepBlue = 21;
        public const byte DeepPurple = 22;
        public const byte DeepViolet = 23;
        public const byte DeepPink = 24;
        public const byte Black = 25;
        public const byte White = 26;
        public const byte Gray = 27;
        public const byte Brown = 28;
        public const byte Shadow = 29;
        public const byte Negative = 30;

        public static readonly int[] itemIds = new int[] {
            0,
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
            ItemID.PinkPaint,
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
            ItemID.DeepPinkPaint,
            ItemID.BlackPaint,
            ItemID.WhitePaint,
            ItemID.GrayPaint,
            ItemID.BrownPaint,
            ItemID.ShadowPaint,
            ItemID.NegativePaint
        };
    }

    public class PaintColors {
        public static Color NoColor { get {     return Color.White; } }
        public static Color Red { get {         return new Color(255, 127, 127); } }
        public static Color Orange { get {      return new Color(255, 191, 127); } }
        public static Color Yellow { get {      return new Color(255, 255, 127); } }
        public static Color Lime { get {        return new Color(191, 255, 127); } }
        public static Color Green { get {       return new Color(127, 255, 127); } }
        public static Color Teal { get {        return new Color(127, 255, 210); } }
        public static Color Cyan { get {        return new Color(127, 255, 255); } }
        public static Color SkyBlue { get {     return new Color(127, 214, 255); } }
        public static Color Blue { get {        return new Color(127, 127, 255); } }
        public static Color Purple { get {      return new Color(191, 127, 255); } }
        public static Color Violet { get {      return new Color(246, 127, 255); } }
        public static Color Pink { get {        return new Color(255, 127, 214); } }
		public static Color DeepRed { get {     return new Color(255,   0,   0); } }
		public static Color DeepOrange { get {  return new Color(255, 127,   0); } }
		public static Color DeepYellow { get {  return new Color(255, 255,   0); } }
		public static Color DeepLime { get {    return new Color(125, 255,   0); } }
		public static Color DeepGreen { get {   return new Color(  0, 255,   0); } }
		public static Color DeepTeal { get {    return new Color(  0, 255, 169); } }
		public static Color DeepCyan { get {    return new Color(  0, 255, 255); } }
		public static Color DeepSkyBlue { get { return new Color(  0, 174, 255); } }
		public static Color DeepBlue { get {    return new Color(  0,   0, 255); } }
		public static Color DeepPurple { get {  return new Color(131,   0, 255); } }
		public static Color DeepViolet { get {  return new Color(242,   0, 255); } }
		public static Color DeepPink { get {    return new Color(255,   0, 172); } }
		public static Color Black { get {       return new Color( 30,  30,  30); } }
        public static Color White { get {       return Color.White; } }
        public static Color Gray { get {        return new Color(127, 127, 127); } }
        public static Color Brown { get {       return new Color(151, 107,  75); } }
        public static Color Shadow { get {      return Color.Black; } }
        public static Color Negative { get {    return Color.Black; } }

        public static Color[] colors {
            get {
                return new Color[] {
                    NoColor,
                    Red,
                    Orange,
                    Yellow,
                    Lime,
                    Green,
                    Teal,
                    Cyan,
                    SkyBlue,
                    Blue,
                    Purple,
                    Violet,
                    Pink,
                    DeepRed,
                    DeepOrange,
                    DeepYellow,
                    DeepLime,
                    DeepGreen,
                    DeepTeal,
                    DeepCyan,
                    DeepSkyBlue,
                    DeepBlue,
                    DeepPurple,
                    DeepViolet,
                    DeepPink,
                    Black,
                    White,
                    Gray,
                    Brown,
                    Shadow,
                    Negative
                };
            }
        }
    }
}
