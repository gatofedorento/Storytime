﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoryTimeCore.DataStructures;
using Microsoft.Xna.Framework;

namespace StoryTimeCore.Extensions
{
    public static class AxisAlignedBoundingBox2DExtensions
    {
        public static AxisAlignedBoundingBox2D Combine(this AxisAlignedBoundingBox2D rec1, AxisAlignedBoundingBox2D rec2)
        {
            float combinedTop, combinedBottom, combinedLeft, combinedRight;

            combinedTop = rec1.Top.BiggerOrEqualThan(rec2.Top);
            combinedBottom = rec1.Bottom.SmallerOrEqualThan(rec2.Bottom);
            combinedLeft = rec1.Left.SmallerOrEqualThan(rec2.Left);
            combinedRight = rec1.Right.BiggerOrEqualThan(rec2.Right);

            return new AxisAlignedBoundingBox2D(
                combinedLeft,
                combinedBottom,
                combinedTop - combinedBottom, 
                combinedRight - combinedLeft
            ); 
        }

        public static AxisAlignedBoundingBox2D Combine(this IEnumerable<AxisAlignedBoundingBox2D> boxes)
        {
            if (!boxes.Any()) return new AxisAlignedBoundingBox2D();
            
            AxisAlignedBoundingBox2D? result = null;

            foreach (AxisAlignedBoundingBox2D box in boxes)
            {
                if (result == null)
                {
                    result = box;
                    continue;
                }

                result = result.Value.Combine(box);
            }

            return result.Value;
        }

        public static AxisAlignedBoundingBox2D GetRotated(
            this AxisAlignedBoundingBox2D rec, float rotation)
        {
            return rec.GetRotated(rotation, rec.Center);
        }

        public static AxisAlignedBoundingBox2D GetRotated(
            this AxisAlignedBoundingBox2D rec, float rotation, Vector2 rotationOrigin)
        {
            Vector2 rotatedBottomLeft = rec.BottomLeft.Rotate(rotation, rotationOrigin);
            Vector2 rotatedBottomRight = rec.BottomRight.Rotate(rotation, rotationOrigin);
            Vector2 rotatedTopLeft = rec.TopLeft.Rotate(rotation, rotationOrigin);
            Vector2 rotatedTopRight = rec.TopRight.Rotate(rotation, rotationOrigin);

            float[] xValues = new float[] { rotatedBottomLeft.X, rotatedBottomRight.X, rotatedTopLeft.X, rotatedTopRight.X };
            float[] yValues = new float[] { rotatedBottomLeft.Y, rotatedBottomRight.Y, rotatedTopLeft.Y, rotatedTopRight.Y };
            
            float smallestX = xValues.Min();
            float biggestX = xValues.Max();
            float smallestY = yValues.Min();
            float biggestY = yValues.Max();

            return new AxisAlignedBoundingBox2D(smallestX, smallestY, biggestY - smallestY, biggestX - smallestX);
        }

        public static AxisAlignedBoundingBox2D GetScaled(
            this AxisAlignedBoundingBox2D rec, Vector2 scale)
        {
            return rec.GetScaled(scale, rec.Center);
        }

        public static AxisAlignedBoundingBox2D GetScaled(
            this AxisAlignedBoundingBox2D rec, Vector2 scale, Vector2 scaleOrigin)
        {
            Vector2 scaledBottomLeft = rec.BottomLeft.GetScaled(scale, scaleOrigin);
            Vector2 scaledTopRight = rec.TopRight.GetScaled(scale, scaleOrigin);
            float height = scaledTopRight.Y - scaledBottomLeft.Y;
            float width = scaledTopRight.X - scaledBottomLeft.X;

            return new AxisAlignedBoundingBox2D(
                scaledBottomLeft.X, 
                scaledBottomLeft.Y,
                height,
                width);
        }

        public static BoundingBox2D GetBoundingBox2D(
            this AxisAlignedBoundingBox2D rec)
        {
            return new BoundingBox2D(
                rec.BottomLeft,
                rec.TopLeft,
                rec.TopRight,
                rec.BottomRight);
        }
    }
}