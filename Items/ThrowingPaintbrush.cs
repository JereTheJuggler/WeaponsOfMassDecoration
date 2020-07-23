using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace WeaponsOfMassDecoration.Items {
    class ThrowingPaintbrush : PaintingItem{
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Throwing Paintbrush");
            SetStaticDefaults(halfDamageText,"This is how most modern art is created anyways, right?");
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThrowingKnife);
            item.rare = 2;

            item.thrown = true;
            item.shoot = ModContent.ProjectileType<Projectiles.ThrowingPaintbrush>();
        }
    }
}
