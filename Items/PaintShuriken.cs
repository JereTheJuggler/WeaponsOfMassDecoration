using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;

namespace WeaponsOfMassDecoration.Items {
    class PaintShuriken : PaintingItem {
        public PaintShuriken() : base() {
            usesGSShader = true;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Paint Shuriken");
			base.SetStaticDefaults(halfDamageText);
		}

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Shuriken);
            item.value = Item.sellPrice(silver: 5);
            item.shoot = ProjectileType<Projectiles.PaintShuriken>();
        }

        protected override Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
            if((paintColor == -1 && customPaint == null) || method == PaintMethods.RemovePaint)
                return null;
            return getExtraTexture("PaintShurikenPainted");
        }
    }
}
