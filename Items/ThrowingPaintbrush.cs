using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using static WeaponsOfMassDecoration.WeaponsOfMassDecoration;
using static Terraria.ModLoader.ModContent;

namespace WeaponsOfMassDecoration.Items {
    class ThrowingPaintbrush : PaintingItem{
        public ThrowingPaintbrush() {
            usesGSShader = true;
		}

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Throwing Paintbrush");
            SetStaticDefaults(halfDamageText,"This is how most modern art is created anyways, right?");
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThrowingKnife);
            item.rare = ItemRarityID.Green;

            item.thrown = true;
            item.shoot = ProjectileType<Projectiles.ThrowingPaintbrush>();
        }

        protected override Texture2D getTexture(int paintColor, CustomPaint customPaint, PaintMethods method) {
            if((paintColor == -1 && customPaint == null) || method == PaintMethods.RemovePaint)
                return null;
            return getExtraTexture("ThrowingPaintbrushPainted");
        }
    }
}
