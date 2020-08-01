using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponsOfMassDecoration.Items{
    class PaintLaser : PaintingItem{

        //please don't look at this... there's probably a much better way to do it.

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Paint Laser");
            Item.staff[ModContent.ItemType<PaintLaser>()] = true;
        }

        public override void SetDefaults() {
            base.SetDefaults();
            item.shoot = ModContent.ProjectileType<Projectiles.PaintLaser>();
            item.rare = ItemRarityID.Green;
            item.damage = 25;
            item.width = 42;
            item.height = 30;
            item.useTime = 20;
            item.useAnimation = 20;
            item.useAmmo = -1;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.mana = 10;
            item.noMelee = true;
            item.knockBack = 1f;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.UseSound = SoundID.Item72;
            item.autoReuse = true;
            item.shootSpeed = 12f;
            item.magic = true;
        }

        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.AmethystStaff);
            recipe.AddRecipeGroup("WoMD:hmBar1", 5);
            recipe.AddIngredient(ItemID.Paintbrush);
            recipe.AddIngredient(ItemID.PaintRoller);
            recipe.AddIngredient(ItemID.PaintScraper);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();

            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.TopazStaff);
            recipe.AddRecipeGroup("WoMD:hmBar1", 5);
            recipe2.AddIngredient(ItemID.Paintbrush);
            recipe2.AddIngredient(ItemID.PaintRoller);
            recipe2.AddIngredient(ItemID.PaintScraper);
            recipe2.AddTile(TileID.DyeVat);
            recipe2.SetResult(this);
            recipe2.AddRecipe();
        }

        public enum GeneralDirection{
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            //TODO: rewrite this... again...
            /*Vector2 dir = new Vector2(speedX, speedY).SafeNormalize(new Vector2(0, 1));
            List<Vector2> endPoints = new List<Vector2>();
            int maxIterations = 600;
            int maxDistanceBetweenSpawns = 16;
            int maxTotalDistance = 1200;
            float currentDistance = 0;
            int distancePerIteration = 8;
            Vector2 currentPosition = position * 1;
            endPoints.Add(position);
            for(int i=0;i<=maxIterations && currentDistance < maxTotalDistance - 1; i++) {
                Point currentTilePosition = currentPosition.ToTileCoordinates();
                Point nextTilePosition = (currentPosition + (dir * distancePerIteration)).ToTileCoordinates();
                if(!WorldGen.InWorld(nextTilePosition.X, nextTilePosition.Y, 10))
                    break;
                Tile nextTile = Main.tile[nextTilePosition]
			}
            endPoints.Add(currentPosition);

            return false;
        }*/
            Vector2 dir = new Vector2(speedX, speedY).SafeNormalize(new Vector2(0, 1));
            int maxDistancePerIteration = 16;
            int maxIterations = 600;
            int maxDistanceBetweenSpawns = 16;
            int maxTotalDistance = 1200;
            float currentDistance = 0;
            List<Vector2> endPoints = new List<Vector2>();
            endPoints.Add(position);
            Point currentTilePos = position.ToTileCoordinates();
            for(int i = 0; i <= maxIterations && currentDistance < maxTotalDistance - 1; i++) {
                Vector2 previousDir = dir;
                float distanceThisIteration = (currentDistance + maxDistancePerIteration <= maxTotalDistance ? maxDistancePerIteration : maxTotalDistance - currentDistance);
                Vector2 nextPos = position + dir * distanceThisIteration;
                LineSegment motion = new LineSegment(position, nextPos);
                GeneralDirection genDir;
                if(dir.X >= 0) {
                    if(dir.Y <= 0) {
                        genDir = GeneralDirection.UpRight;
                    } else {
                        genDir = GeneralDirection.DownRight;
                    }
                } else {
                    if(dir.Y <= 0) {
                        genDir = GeneralDirection.UpLeft;
                    } else {
                        genDir = GeneralDirection.DownLeft;
                    }
                }
                bool hitTile = false;
                Tile currentTile = Main.tile[currentTilePos.X, currentTilePos.Y];
                Tile tileDown = Main.tile[currentTilePos.X, currentTilePos.Y + 1];

                while(true) {
                    float xCoord = 0;
                    switch(genDir) {
                        case GeneralDirection.UpRight:
                        case GeneralDirection.DownRight:
                            xCoord = currentTilePos.X * 16f + 16f;
                            break;
                        case GeneralDirection.UpLeft:
                        case GeneralDirection.DownLeft:
                            xCoord = currentTilePos.X * 16f;
                            break;
                    }
                    float yCoord = 0;
                    switch(genDir) {
                        case GeneralDirection.UpRight:
                        case GeneralDirection.UpLeft:
                            yCoord = currentTilePos.Y * 16f;
                            break;
                        case GeneralDirection.DownRight:
                        case GeneralDirection.DownLeft:
                            yCoord = currentTilePos.Y * 16f + 16f;
                            break;
                    }
                    //relative slope will represent how the slope of the current direction compares to the slope of a line going through the current position
                    //and the intersection between the next horizontal and vertical lines.
                    //
                    //if relative slope is greater than 0, the vertical line will be hit first.
                    //if relative slope is less than 0, the horizontal line will be hit first.
                    //if relative slope rounded to 3 decimals equals 0, then the current direction will hit the corner formed by the vertical and horizontal lines
                    float relativeSlope = Math.Abs((yCoord - position.Y) / (xCoord - position.X)) - Math.Abs(dir.Y / dir.X);
                    Point checkTilePos;
                    if(Math.Abs(relativeSlope) <= .05) {
                        if(WorldGen.TileEmpty(currentTilePos.X, currentTilePos.Y)) {
                            //WorldGen.PlaceTile(currentTilePos.X, currentTilePos.Y, TileID.Bubble, true, true);
                        } else if(WorldGen.SolidOrSlopedTile(currentTilePos.X, currentTilePos.Y)) {
                            //Main.tile[currentTilePos.X, currentTilePos.Y].type = TileID.AmethystGemspark;
                        }
                        //motion will hit the corner, if it can travel far enough
                        if(Math.Abs(xCoord - position.X) > Math.Abs(nextPos.X - position.X)) {
                            //the corner will not be hit before travelling the max distance per iteration. the projectile will not hit a tile
                            break;
                        }
                        TileDef upLeft;
                        TileDef upRight;
                        TileDef downLeft;
                        TileDef downRight;
                        switch(genDir) {
                            case GeneralDirection.UpLeft:
                                checkTilePos = new Point(currentTilePos.X - 1, currentTilePos.Y - 1);
                                upLeft = new TileDef(checkTilePos);
                                break;
                            case GeneralDirection.UpRight:
                                checkTilePos = new Point(currentTilePos.X + 1, currentTilePos.Y - 1);
                                upLeft = new TileDef(checkTilePos.X - 1, checkTilePos.Y);
                                break;
                            case GeneralDirection.DownLeft:
                                checkTilePos = new Point(currentTilePos.X - 1, currentTilePos.Y + 1);
                                upLeft = new TileDef(checkTilePos.X, checkTilePos.Y - 1);
                                break;
                            case GeneralDirection.DownRight:
                            default:
                                checkTilePos = new Point(currentTilePos.X + 1, currentTilePos.Y + 1);
                                upLeft = new TileDef(checkTilePos.X - 1, checkTilePos.Y - 1);
                                break;
                        }
                        upRight = new TileDef(upLeft.coords.X + 1, upLeft.coords.Y);
                        downLeft = new TileDef(upLeft.coords.X, upLeft.coords.Y + 1);
                        downRight = new TileDef(upLeft.coords.X + 1, upLeft.coords.Y + 1);
                        //take care of corner between 2 similar sloped blocks first
                        if((genDir != GeneralDirection.DownRight && upRight.slope == 3 && downLeft.slope == 3) ||
                           (genDir != GeneralDirection.UpLeft && upRight.slope == 2 && downLeft.slope == 3)) {
                            // tiles look like this
                            //     /]
                            //   /]
                            // or like this
                            //     [/
                            //   [/
                            //
                            hitTile = true;
                            nextPos = new Vector2(xCoord, yCoord);
                            reflectAgainstYEqualsNegativeX(ref dir);
                            currentTilePos = checkTilePos;
                            break;
                        }
                        if((genDir != GeneralDirection.DownLeft && upLeft.slope == 4 && downRight.slope == 4) ||
                           (genDir != GeneralDirection.UpRight && upLeft.slope == 1 && downRight.slope == 1)) {
                            // tiles look like this
                            //   \] 
                            //     \]
                            // or like this
                            //   [\
                            //     [\
                            //
                            hitTile = true;
                            //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Stone;
                            nextPos = new Vector2(xCoord, yCoord);
                            reflectAgainstYEqualsX(ref dir);
                            currentTilePos = checkTilePos;
                            break;
                        }
                    } else {
                        //get the next tile position to check, if it's not out of reach
                        if(relativeSlope > 0) {
                            //vertical line will be hit first, if at all
                            if(Math.Abs(xCoord - position.X) > Math.Abs(nextPos.X - position.X)) {
                                //the line will not be hit before travelling the max distance per iteration. the projectile will not hit a tile
                                break;
                            }
                            if(genDir == GeneralDirection.UpLeft || genDir == GeneralDirection.DownLeft) {
                                checkTilePos = new Point(currentTilePos.X - 1, currentTilePos.Y);
                            } else {
                                checkTilePos = new Point(currentTilePos.X + 1, currentTilePos.Y);
                            }
                        } else {
                            //horizontal line will be hit first, if at all
                            if(Math.Abs(yCoord - position.Y) > Math.Abs(nextPos.Y - position.Y)) {
                                //the line will not be hit before travelling the max distance per iteration. the projectile will not hit a tile
                                break;
                            }
                            if(genDir == GeneralDirection.UpLeft || genDir == GeneralDirection.UpRight) {
                                checkTilePos = new Point(currentTilePos.X, currentTilePos.Y - 1);
                            } else {
                                checkTilePos = new Point(currentTilePos.X, currentTilePos.Y + 1);
                            }
                        }
                        Tile checkTile = Main.tile[checkTilePos.X, checkTilePos.Y];
                        TileDef tileDef = new TileDef(checkTilePos);
                        TileRect tileRect = new TileRect(checkTilePos);
                        if(tileDef.slope != -1) {
                            //check tile is an active, solid/sloped block
                            if(relativeSlope > 0) {
                                //vertical line hit first
                                if(//check right edge of tile
                                   ((genDir == GeneralDirection.UpLeft || genDir == GeneralDirection.DownLeft) && (new int[] { 0, 2, 4 }).Contains(tileDef.slope)) ||
                                   //check left edge of tile
                                   ((genDir == GeneralDirection.UpRight || genDir == GeneralDirection.DownRight) && (new int[] { 0, 1, 3 }).Contains(tileDef.slope))) {
                                    //tile has a side along the vertical line
                                    hitTile = true;
                                    //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Copper;
                                    reflectAgainstY(ref dir);
                                    List<Vector2> interceptList = (new LineEquation(xCoord)).getIntercepts(motion.equation);
                                    if(interceptList.Count > 0) {
                                        nextPos = interceptList[0];
                                        currentTilePos = checkTilePos;
                                    }
                                    break;
                                }
                                LineSegment extraSide;
                                switch(tileDef.slope) {
                                    case 1: // [\
                                    case 4: // \]
                                        extraSide = new LineSegment(tileRect.topLeft, tileRect.bottomRight);
                                        break;
                                    case 2: // /]
                                    case 3: // [/
                                        extraSide = new LineSegment(tileRect.topRight, tileRect.bottomLeft);
                                        break;
                                    default: //half block
                                        if(genDir == GeneralDirection.UpLeft || genDir == GeneralDirection.DownLeft) {
                                            //right side
                                            extraSide = new LineSegment(tileRect.middleRight, tileRect.bottomRight);
                                        } else {
                                            //left side
                                            extraSide = new LineSegment(tileRect.middleLeft, tileRect.bottomLeft);
                                        }
                                        break;
                                }
                                List<Vector2> intercepts = extraSide.getIntercepts(motion);
                                if(intercepts.Count > 0) {
                                    nextPos = intercepts[0];
                                    hitTile = true;
                                    //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Lead;
                                    switch(tileDef.slope) {
                                        case 1: // [\
                                        case 4: // \]
                                            reflectAgainstYEqualsX(ref dir);
                                            break;
                                        case 2: // /]
                                        case 3: // [/
                                            reflectAgainstYEqualsNegativeX(ref dir);
                                            break;
                                        default: //half block
                                            reflectAgainstY(ref dir);
                                            break;
                                    }
                                    currentTilePos = checkTilePos;
                                    break;
                                }
                            } else {
                                //horizontal line hit first
                                if(genDir == GeneralDirection.UpRight || genDir == GeneralDirection.UpLeft) {
                                    //check bottom edge of tile
                                    if((new int[] { 0, 1, 2, 5 }).Contains(tileDef.slope)) {
                                        hitTile = true;
                                        //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.RedBrick;
                                        reflectAgainstX(ref dir);
                                        nextPos = (new LineEquation(0, yCoord)).getIntercepts(motion.equation)[0];
                                        currentTilePos = checkTilePos;
                                        break;
                                    }
                                    LineSegment slopedSide;
                                    if(tileDef.slope == 3) { // [/
                                        slopedSide = new LineSegment(tileRect.topRight, tileRect.bottomLeft);
                                    } else { // \]
                                        slopedSide = new LineSegment(tileRect.topLeft, tileRect.bottomRight);
                                        //slopedSide = new LineSegment(tileRect.topLeft, tileRect.bottomRight);
                                    }
                                    List<Vector2> intercepts = slopedSide.getIntercepts(motion);
                                    if(intercepts.Count > 0) {
                                        nextPos = intercepts[0];
                                        hitTile = true;
                                        //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Cloud;
                                        if(tileDef.slope == 3) { // [/
                                            reflectAgainstYEqualsNegativeX(ref dir);
                                        } else { // \]
                                            reflectAgainstYEqualsX(ref dir);
                                        }
                                        currentTilePos = checkTilePos;
                                        break;
                                    }
                                } else {
                                    if((new int[] { 0, 3, 4 }).Contains(tileDef.slope)) {
                                        hitTile = true;
                                        //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Asphalt;
                                        reflectAgainstX(ref dir);
                                        nextPos = (new LineEquation(0, yCoord)).getIntercepts(motion.equation)[0];
                                        currentTilePos = checkTilePos;
                                        break;
                                    }
                                    LineSegment extraSide;
                                    if(tileDef.slope == 1) // [\
                                        extraSide = new LineSegment(tileRect.topLeft, tileRect.bottomRight);
                                    else if(tileDef.slope == 2) // /]
                                        extraSide = new LineSegment(tileRect.topRight, tileRect.bottomLeft);
                                    else // half block
                                        extraSide = new LineSegment(tileRect.middleLeft, tileRect.middleRight);
                                    List<Vector2> intercepts = extraSide.getIntercepts(motion);
                                    if(intercepts.Count > 0) {
                                        nextPos = intercepts[0];
                                        hitTile = true;
                                        //Main.tile[checkTilePos.X, checkTilePos.Y].type = TileID.Glass;
                                        if(tileDef.slope == 1)
                                            reflectAgainstYEqualsX(ref dir);
                                        else if(tileDef.slope == 2)
                                            reflectAgainstYEqualsNegativeX(ref dir);
                                        else
                                            reflectAgainstX(ref dir);
                                        currentTilePos = checkTilePos;
                                    }
                                    break;
                                }
                            }
                        }//else the check tile is either actuated or a background object. stay in loop and get the next check tile
                    }
                    //if the loop has not been escaped then a tile has not been hit and not all tiles along the projectile's motion have been checked
                    currentTilePos = checkTilePos;
                    //currentTilePos = new Point((int) Math.Floor(nextPos.X / 16f),(int) Math.Floor(nextPos.Y / 16f));
                }
                Vector2 previousPoint = endPoints[endPoints.Count - 1];
                endPoints.Add(nextPos);
                currentDistance += (float)Math.Sqrt(Math.Pow(nextPos.X - previousPoint.X, 2) + Math.Pow(nextPos.Y - previousPoint.Y, 2));
                if(i == maxIterations) {
                    i = maxIterations;
                }
                position = nextPos;
            }
            for(int p = 0; p < endPoints.Count - 1; p++) {
                Vector2 currentPos = endPoints[p];
                Vector2 nextPos = endPoints[p + 1];
                Vector2 disp = nextPos - currentPos;
                float totalDistance = (float)Math.Sqrt(Math.Pow(disp.X, 2) + Math.Pow(disp.Y, 2));
                int totalSpawns = (int)Math.Ceiling(totalDistance / maxDistanceBetweenSpawns);
                Vector2 spawnOffset = disp / totalSpawns;
                for(int i = 1; i <= totalSpawns; i++) {
                    Vector2 previousPos = currentPos + spawnOffset * (i - 1);
                    int projId = Projectile.NewProjectile(currentPos + spawnOffset * i, new Vector2(0, 0), ModContent.ProjectileType<Projectiles.PaintLaser>(), damage, 0f, player.whoAmI, previousPos.X, previousPos.Y);
                }
            }


            return false;
        }/*
            SetDefaults();
            Vector2 dir = new Vector2(speedX, speedY).SafeNormalize(new Vector2(0, 1));
            int maxDistance = 2;
            int maxIterations = 300;
            int iterationsBetweenSpawns = 3;
            int iterationsToNextSpawn = iterationsBetweenSpawns;
            maxDistance = 4;
            position += dir * 64;
            Vector2 lastSpawnPosition = position;
            for(int proj = 0;proj <= maxIterations; proj++) {
                bool spawnProjectile = iterationsToNextSpawn == 0;
                Vector2 previousDir = dir;
                Vector2 nextPos = position + dir * maxDistance;
                Point nextTilePos = new Point((int)Math.Floor(nextPos.X / 16), (int)Math.Floor(nextPos.Y / 16));
                LineSegment motion = new LineSegment(position, nextPos);
                TileBorder border = new TileBorder(nextTilePos.X, nextTilePos.Y);
                List<LineIntercept> intercepts = border.getIntercepts(motion);
                if(intercepts.Count > 0) {
                    //hit a wall. force spawning a projectile
                    spawnProjectile = true;
                    LineIntercept intercept = intercepts[0];
                    float closestDistance = 99;
                    if(intercepts.Count > 1) {
                        for(int i = 0; i < intercepts.Count; i++) {
                            float distance = (float)Math.Sqrt(Math.Pow(intercepts[i].point.Y - position.Y, 2) + Math.Pow(intercepts[i].point.X - position.X, 2));
                            if(distance < closestDistance) {
                                closestDistance = distance;
                                intercept = intercepts[i];
                            }
                        }
                    } else {
                        closestDistance = (float)Math.Sqrt(Math.Pow(intercepts[0].point.Y - position.Y, 2) + Math.Pow(intercepts[0].point.X - position.X, 2));
                    }
                    Vector2 cornerPoint = new Vector2(0, 0);
                    bool hitCorner = false;
                    if(Math.Abs(Math.Round(intercept.point.X) - Math.Round(intercept.segment.point1.X)) <= 2 &&
                       Math.Abs(Math.Round(intercept.point.Y) - Math.Round(intercept.segment.point1.Y)) <= 2) {
                        cornerPoint = intercept.segment.point1;
                        hitCorner = true;
                    } else if(Math.Abs(Math.Round(intercept.point.X) - Math.Round(intercept.segment.point2.X)) <= 2 &&
                              Math.Abs(Math.Round(intercept.point.Y) - Math.Round(intercept.segment.point2.Y)) <= 2) {
                        cornerPoint = intercept.segment.point2;
                        hitCorner = true;
                    }
                    nextPos = position + dir * (.9f * closestDistance);
                    bool bypassNormalReflection = false;
                    if(hitCorner) {
                        cornerPoint = new Vector2((float)Math.Round(cornerPoint.X / 16) * 16f, (float)Math.Round(cornerPoint.Y / 16) * 16f);
                        nextPos = cornerPoint;
                        TileDef upLeft = new TileDef((cornerPoint + new Vector2(-4f, -4f)));
                        TileDef upRight = new TileDef((cornerPoint + new Vector2(4f, -4f)));
                        TileDef downLeft = new TileDef((cornerPoint + new Vector2(-4f, 4f)));
                        TileDef downRight = new TileDef((cornerPoint + new Vector2(4f, 4f)));
                        bool hitsUpLeft = (new int[] { 0, 1, 2, 4, 5 }).Contains(upLeft.slope);
                        bool hitsUpRight = (new int[] { 0, 1, 2, 3, 5 }).Contains(upRight.slope);
                        bool hitsDownLeft = (new int[] { 0, 2, 3, 4 }).Contains(downLeft.slope);
                        bool hitsDownRight = (new int[] { 0, 1, 3, 4 }).Contains(downRight.slope);
                        if(dir.X >= 0 && dir.Y <= 0) { //moving up right
                            if(hitsUpLeft && hitsDownRight) {
                                if(upLeft.slope == 4 && downRight.slope == 4) {
                                    dir = new Vector2(-1 * dir.Y, -1 * dir.X);
                                } else {
                                    dir *= -1;
                                }
                                bypassNormalReflection = true;
                            } else if(hitsUpLeft && hitsUpRight) {
                                dir = new Vector2(dir.X, dir.Y * -1);
                                bypassNormalReflection = true;
                            } else if(hitsUpRight && hitsDownRight) {
                                dir = new Vector2(dir.X * -1, dir.Y);
                                bypassNormalReflection = true;
                            }
                        } else if(dir.X < 0 && dir.Y <= 0) { //moving up left
                            if(hitsUpRight && hitsDownLeft) {
                                if(upRight.slope == 3 && downLeft.slope == 3) {
                                    dir = new Vector2(-1 * dir.Y, -1 * dir.X);
                                } else {
                                    dir *= -1;
                                }
                                bypassNormalReflection = true;
                            } else if(hitsUpRight && hitsUpLeft) {
                                dir = new Vector2(dir.X, dir.Y * -1);
                                bypassNormalReflection = true;
                            } else if(hitsUpLeft && hitsDownLeft) {
                                dir = new Vector2(dir.X * -1, dir.Y);
                                bypassNormalReflection = true;
                            }
                        } else if(dir.X >= 0 && dir.Y > 0) {//moving down right
                            if(hitsUpRight && hitsDownLeft) {
                                if(upRight.slope == 2 && downLeft.slope == 2) {
                                    dir = new Vector2(-1 * dir.Y, -1 * dir.X);
                                } else {
                                    dir *= -1;
                                }
                                bypassNormalReflection = true;
                            } else if(hitsDownLeft && hitsDownRight) {
                                dir = new Vector2(dir.X, dir.Y * -1);
                                bypassNormalReflection = true;
                            } else if(hitsDownRight && hitsUpRight) {
                                dir = new Vector2(dir.X * -1, dir.Y);
                                bypassNormalReflection = true;
                            }
                        } else { //moving down left
                            if(hitsDownRight && hitsUpLeft) {
                                if(upLeft.slope == 1 && downRight.slope == 1) {
                                    dir = new Vector2(-1 * dir.Y, -1 * dir.X);
                                } else {
                                    dir *= -1;
                                }
                                bypassNormalReflection = true;
                            } else if(hitsDownLeft && hitsUpLeft) {
                                dir = new Vector2(dir.X * -1, dir.Y);
                                bypassNormalReflection = true;
                            } else if(hitsDownLeft && hitsDownRight) {
                                dir = new Vector2(dir.X, dir.Y * -1);
                                bypassNormalReflection = true;
                            }
                        }
                        if(bypassNormalReflection) {
                            nextPos += dir * 2f;
                        }
                    }
                    if(!bypassNormalReflection) {
                        LineSegment line = intercept.segment;
                        if(line.point1.X == line.point2.X) {
                            //vertical line
                            dir = new Vector2(dir.X * -1, dir.Y);
                        } else if(line.point1.Y == line.point2.Y) {
                            //horizontal line
                            dir = new Vector2(dir.X, dir.Y * -1);
                        } else if(line.point1.X == line.minX && line.point1.Y == line.minY) {
                            //y = x \
                            dir = new Vector2(dir.Y, dir.X);
                        } else {
                            //y = -x /
                            dir = new Vector2(-1 * dir.Y, -1 * dir.X);
                        }
                    } else {
                        //return false;
                    }
                }
                if(proj == maxIterations)
                    spawnProjectile = true;
                if(spawnProjectile) {
                    int projId = Projectile.NewProjectile(nextPos,dir, ModContent.ProjectileType<Projectiles.PaintLaser>(), damage, 0f, player.whoAmI, lastSpawnPosition.X,lastSpawnPosition.Y);
                    lastSpawnPosition = nextPos;
                    iterationsToNextSpawn = iterationsBetweenSpawns;
                } else {
                    iterationsToNextSpawn--;
                }
                position = nextPos;
            }
            return false;
        }//*/

        public void reflectAgainstY(ref Vector2 dir) {
            dir = new Vector2(-1 * dir.X, dir.Y);
        }
        public void reflectAgainstX(ref Vector2 dir) {
            dir = new Vector2(dir.X, -1 * dir.Y);
        }
        public void reflectAgainstYEqualsX(ref Vector2 dir) {
            dir = new Vector2(dir.Y, dir.X);
        }
        public void reflectAgainstYEqualsNegativeX(ref Vector2 dir) {
            dir = new Vector2(-1 * dir.Y, -1 * dir.X);
        }
    }

    public class TileRect{
        public Point topLeft;
        public Point topRight;
        public Point middleLeft;
        public Point middleRight;
        public Point bottomLeft;
        public Point bottomRight;

        public TileRect(Point tilePos) {
            setCorners(tilePos.X * 16, tilePos.Y * 16);
        }
        public TileRect(int tileX, int tileY) {
            setCorners(tileX * 16, tileY * 16);
        }

        private void setCorners(int x, int y) {
            topLeft = new Point(x, y);
            topRight = new Point(x + 15, y);
            middleLeft = new Point(x, y + 8);
            middleRight = new Point(x + 15, y + 8);
            bottomLeft = new Point(x, y + 15);
            bottomRight = new Point(x + 15, y + 15);
        }
    }

    public class TileBorder{
        public List<LineSegment> segments;
        public Tile tile;
        public Vector2 topLeftCorner;
        public Vector2 bottomLeftCorner;
        public Vector2 topRightCorner;
        public Vector2 bottomRightCorner;
        public Vector2 middleLeftCorner;
        public Vector2 middleRightCorner;
        public Vector2 paddedTopLeftCorner;
        public Vector2 paddedTopRightCorner;
        public Vector2 paddedBottomLeftCorner;
        public Vector2 paddedBottomRightCorner;
        public Vector2 paddedMiddleLeftCorner;
        public Vector2 paddedMiddleRightCorner;

        public TileBorder(int x, int y) {
            segments = new List<LineSegment>();
            tile = Main.tile[x, y];
            topLeftCorner = new Vector2(x * 16, y * 16);
            bottomLeftCorner = topLeftCorner + new Vector2(0, 16);
            topRightCorner = topLeftCorner + new Vector2(16, 0);
            bottomRightCorner = topRightCorner + new Vector2(0, 16);
            middleLeftCorner = topLeftCorner + new Vector2(0, 7);
            middleRightCorner = topRightCorner + new Vector2(0, 7);

            paddedTopLeftCorner = topLeftCorner + new Vector2(-2, -2);
            paddedTopRightCorner = topRightCorner + new Vector2(2, -2);
            paddedBottomLeftCorner = bottomLeftCorner + new Vector2(-2, 2);
            paddedBottomRightCorner = bottomRightCorner + new Vector2(2, 2);
            paddedMiddleLeftCorner = middleLeftCorner + new Vector2(-2, -2);
            paddedMiddleRightCorner = middleRightCorner + new Vector2(2, -2);

            if(tile.active() && tile.collisionType != -1) {
                if(tile.halfBrick()) {
                    segments.Add(new LineSegment(middleLeftCorner, bottomLeftCorner));
                    segments.Add(new LineSegment(bottomLeftCorner, bottomRightCorner));
                    segments.Add(new LineSegment(bottomRightCorner, middleRightCorner));
                    segments.Add(new LineSegment(middleLeftCorner, middleRightCorner));

                    segments.Add(new LineSegment(paddedMiddleLeftCorner, paddedMiddleRightCorner));
                    segments.Add(new LineSegment(paddedMiddleRightCorner, paddedBottomRightCorner));
                    segments.Add(new LineSegment(paddedBottomRightCorner, paddedBottomLeftCorner));
                    segments.Add(new LineSegment(paddedBottomLeftCorner, paddedMiddleLeftCorner));
                } else {
                    byte slope = tile.slope();
                    if(slope == 0 || slope == 1 || slope == 3) {
                        segments.Add(new LineSegment(topLeftCorner, bottomLeftCorner));
                        segments.Add(new LineSegment(paddedTopLeftCorner, paddedBottomLeftCorner));
                    }
                    if(slope == 0 || slope == 3 || slope == 4) {
                        segments.Add(new LineSegment(topLeftCorner, topRightCorner));
                        segments.Add(new LineSegment(paddedTopLeftCorner, paddedTopRightCorner));
                    }
                    if(slope == 0 || slope == 4 || slope == 2) {
                        segments.Add(new LineSegment(topRightCorner, bottomRightCorner));
                        segments.Add(new LineSegment(paddedTopRightCorner, paddedBottomRightCorner));
                    }
                    if(slope == 0 || slope == 1 || slope == 2) {
                        segments.Add(new LineSegment(bottomRightCorner, bottomLeftCorner));
                        segments.Add(new LineSegment(paddedBottomRightCorner, paddedBottomLeftCorner));
                    }
                    if(slope == 1 || slope == 4) {
                        segments.Add(new LineSegment(topLeftCorner, bottomRightCorner));
                        segments.Add(new LineSegment(paddedTopLeftCorner, paddedBottomRightCorner));
                    }
                    if(slope == 3 || slope == 2) {
                        segments.Add(new LineSegment(topRightCorner, bottomLeftCorner));
                        segments.Add(new LineSegment(paddedTopRightCorner, paddedBottomLeftCorner));
                    }
                }
            }
        }

        public List<LineIntercept> getIntercepts(LineSegment segment) {
            List<LineIntercept> intercepts = new List<LineIntercept>();
            for(int s = 0; s < segments.Count; s++) {
                List<Vector2> newIntercept = segments[s].getIntercepts(segment);
                if(newIntercept.Count > 0)
                    intercepts.Add(new LineIntercept(segments[s], newIntercept[0]));
            }
            return intercepts;
        }
    }

    public class LineSegment{
        public Vector2 point1;
        public Vector2 point2;

        public float minX;
        public float maxX;

        public float minY;
        public float maxY;

        public LineEquation equation;

        public LineSegment(float x1, float y1, float x2, float y2) {
            point1 = new Vector2((float)Math.Round(x1, 5), (float)Math.Round(y1, 5));
            point2 = new Vector2((float)Math.Round(x2, 5), (float)Math.Round(y2, 5));
            if(x1 <= x2) {
                minX = point1.X;
                maxX = point2.X;
            } else {
                minX = point2.X;
                maxX = point1.X;
            }
            if(y1 <= y2) {
                minY = point1.Y;
                maxY = point2.Y;
            } else {
                minY = point2.Y;
                maxY = point1.Y;
            }
            equation = new LineEquation(this);
        }
        public LineSegment(Vector2 point1, Vector2 point2) {
            this.point1 = point1;
            this.point2 = point2;
            if(point1.X <= point2.X) {
                minX = point1.X;
                maxX = point2.X;
            } else {
                minX = point2.X;
                maxX = point1.X;
            }
            if(point1.Y <= point2.Y) {
                minY = point1.Y;
                maxY = point2.Y;
            } else {
                minY = point2.Y;
                maxY = point1.Y;
            }
            equation = new LineEquation(this);
        }
        public LineSegment(Point point1, Point point2) {
            this.point1 = point1.ToVector2();
            this.point2 = point2.ToVector2();
            if(point1.X <= point2.X) {
                minX = point1.X;
                maxX = point2.X;
            } else {
                minX = point2.X;
                maxX = point1.X;
            }
            if(point1.Y <= point2.Y) {
                minY = point1.Y;
                maxY = point2.Y;
            } else {
                minY = point2.Y;
                maxY = point1.Y;
            }
            equation = new LineEquation(this);
        }

        public List<Vector2> getIntercepts(LineSegment segment) {
            List<Vector2> intercepts = equation.getIntercepts(segment.equation);
            List<Vector2> trueIntercepts = new List<Vector2>();
            for(var i = 0; i < intercepts.Count; i++) {
                if(intercepts[i].X >= minX && intercepts[i].X <= maxX && intercepts[i].X >= segment.minX && intercepts[i].X <= segment.maxX)
                    trueIntercepts.Add(intercepts[i]);
            }
            return trueIntercepts;
        }
    }

    public class LineIntercept{
        public Vector2 point;
        public LineSegment segment;

        public LineIntercept(LineSegment segment, Vector2 point) {
            this.segment = segment;
            this.point = point;
        }
    }

    public class LineEquation{
        public float slope;
        public float yIntercept;
        public float x;
        public bool vertical;

        public LineEquation(float slope, float yIntercept) {
            this.slope = slope;
            this.yIntercept = yIntercept;
            vertical = false;
            x = 0;
        }
        public LineEquation(float x) {
            vertical = true;
            this.x = x;
        }
        public LineEquation(LineSegment segment) {
            if(segment.point1.X == segment.point2.X) {
                vertical = true;
                x = segment.point1.X;
            } else {
                vertical = false;
                //m = (y1-y2) / (x1-x2)
                slope = (float)Math.Round((segment.point1.Y - segment.point2.Y) / (segment.point1.X - segment.point2.X), 5);
                //y = mx + b
                //b = y - mx
                yIntercept = (float)Math.Round(segment.point1.Y - slope * segment.point1.X, 5);
            }
        }

        public List<Vector2> getIntercepts(LineEquation equation) {
            List<Vector2> intercepts = new List<Vector2>();
            if((vertical && equation.vertical) || (slope == equation.slope)) {
                //either exact same line or no intersections
                return intercepts;
            }
            if(vertical) {
                //this is vertical. other eq is not. get y from other eq where x = this eq's x
                intercepts.Add(new Vector2(x, equation.slope * x + equation.yIntercept));
            } else if(equation.vertical) {
                //other eq is vertical. this is not. get y from this eq where x = other eq's x
                intercepts.Add(new Vector2(equation.x, slope * equation.x + yIntercept));
            } else {
                // lines are oblique
                // y1 = m1 * x1 + b1
                // y2 = m2 * x2 + b2
                //
                // set ys equal to each other and solve for x
                // m1 * x + b1 = m2 * x + b2
                // (m1 * x) - (m2 * x) = b2 - b1
                // x * (m1 - m2) = b2 - b1
                // x = (b2 - b1) / (m1 - m2)
                //
                // shove x back into original eq to get y
                float interceptX = (equation.yIntercept - yIntercept) / (slope - equation.slope);
                float interceptY = slope * interceptX + yIntercept;
                intercepts.Add(new Vector2(interceptX, interceptY));
            }
            return intercepts;
        }
    }

    public class TileDef {
        public Point coords;
        public Tile tile;
        public int slope; //-1 slope will specify an inactive tile and 5 will specify a half block, to spare having to reference 2 additional properties.

        public TileDef(int tileX, int tileY) {
            init(new Point(tileX, tileY));
        }
        public TileDef(Point coords) {
            init(coords);
        }
        public TileDef(Vector2 coords) {
            init(new Point((int)Math.Floor(coords.X / 16), (int)Math.Floor(coords.Y / 16)));
        }

        private void init(Point coords) {
            this.coords = coords;
            tile = Main.tile[coords.X, coords.Y];
            if(!tile.active() || tile.collisionType == -1) {
                slope = -1;
            } else if(tile.halfBrick()) {
                slope = 5;
            } else {
                slope = tile.slope();
            }
        }
    }
}