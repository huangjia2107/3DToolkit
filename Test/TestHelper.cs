using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThreeDToolkit.Helps;
using System.Windows;
using ThreeDToolkit.Models;

namespace Test
{
    public static class TestHelper
    {
        public static List<Point> GenerateCylinderSideMesh(double topRadius, double bottomRadius, IsoscelesTrapezoid trapezoid, int stacks = 20, bool isSharePoint = true)
        {
            var mesh = new List<Point>();

            var indexBase = isSharePoint ? 2 : 4;

            //for texture
            double topSectorRadius = 0, bottomSectorRadius = 0, sectorRadian = 0d;
            Size sectorSize = new Size();
            Func<int, int, Point> topSectorArcPosFunc = null, bottomSectorArcPosFunc = null;

            if (!DoubleUtil.AreClose(topRadius, bottomRadius))
            {
                CalculateSectorRadius(topRadius, bottomRadius, trapezoid.Height, out topSectorRadius, out bottomSectorRadius);

                sectorRadian = 2 * Math.PI * topRadius / topSectorRadius;
                topSectorArcPosFunc = GenerateSectorArcPositionFunc(topSectorRadius, sectorRadian);
                bottomSectorArcPosFunc = GenerateSectorArcPositionFunc(bottomSectorRadius, sectorRadian);

                sectorSize = CalculateHalfSectorSize(topRadius, bottomRadius, trapezoid.Height, topSectorRadius, bottomSectorRadius, sectorRadian);
            }

            for (int i = 0; i <= stacks; i++)
            {
                InsertTexturePoint(mesh, i, stacks, topRadius, bottomRadius, topSectorRadius, bottomSectorRadius, sectorRadian, sectorSize,
                           topSectorArcPosFunc, bottomSectorArcPosFunc);

                if (i < stacks)
                { 
                    if (!isSharePoint)
                    {
                        InsertTexturePoint(mesh, i + 1, stacks, topRadius, bottomRadius, topSectorRadius, bottomSectorRadius, sectorRadian, sectorSize,
                            topSectorArcPosFunc, bottomSectorArcPosFunc);

                        if (i == stacks - 1)
                            break;
                    }
                }
            }

            return mesh;
        }

        public static double GetRadian(double radius, double arcLength, int index, int stacks)
        {
            return Math.Min(2 * Math.PI * radius, arcLength) * index / stacks / radius;
        }

        private static void InsertTexturePoint(List<Point> mesh, int index, int stacks, double topRadius, double bottomRadius,
                   double topSectorRadius, double bottomSectorRadius, double sectorRadian, Size sectorSize,
                   Func<int, int, Point> topSectorArcPosFunc, Func<int, int, Point> bottomSectorArcPosFunc)
        {
            if (DoubleUtil.AreClose(topRadius, bottomRadius))
            {
                mesh.Add(new Point((double)index / stacks, 1));
                mesh.Add(new Point((double)index / stacks, 0));
            }
            else
            {
                var toPoint = topSectorArcPosFunc(index, stacks);
                var bottomPoint = bottomSectorArcPosFunc(index, stacks);

                Point smallArcTexturePoint, largeArcTexturePoint;
                if (DoubleUtil.LessThan(topRadius, bottomRadius))
                {
                    smallArcTexturePoint = CalculateSmallArcTexturePoint(toPoint, topSectorRadius, bottomSectorRadius, sectorRadian);
                    largeArcTexturePoint = CalculateLargeArcTexturePoint(bottomPoint, topSectorRadius, bottomSectorRadius, sectorRadian);

                    mesh.Add(new Point(largeArcTexturePoint.X / sectorSize.Width, largeArcTexturePoint.Y / sectorSize.Height));
                    mesh.Add(new Point(smallArcTexturePoint.X / sectorSize.Width, smallArcTexturePoint.Y / sectorSize.Height));
                }
                else
                {
                    smallArcTexturePoint = CalculateSmallArcTexturePoint(bottomPoint, bottomSectorRadius, topSectorRadius, sectorRadian);
                    largeArcTexturePoint = CalculateLargeArcTexturePoint(toPoint, bottomSectorRadius, topSectorRadius, sectorRadian);

                    smallArcTexturePoint = new Point(smallArcTexturePoint.X, sectorSize.Height - smallArcTexturePoint.Y);
                    largeArcTexturePoint = new Point(largeArcTexturePoint.X, sectorSize.Height - largeArcTexturePoint.Y);

                    mesh.Add(new Point(smallArcTexturePoint.X / sectorSize.Width, smallArcTexturePoint.Y / sectorSize.Height));
                    mesh.Add(new Point(largeArcTexturePoint.X / sectorSize.Width, largeArcTexturePoint.Y / sectorSize.Height));
                }
            }
        }

        private static Point CalculateSmallArcTexturePoint(Point smallArcPoint, double smallArcSectorRadius, double largeArcSectorRadius, double sectorRadian)
        {
            var left = 0d;
            var top = 0d;

            if (DoubleUtil.LessThan(sectorRadian, Math.PI))
            {
                left = smallArcPoint.X + (largeArcSectorRadius - smallArcSectorRadius) * Math.Sin(sectorRadian / 2);
                top = smallArcPoint.Y - smallArcSectorRadius * Math.Cos(sectorRadian / 2);
            }
            else
            {
                left = smallArcPoint.X + (largeArcSectorRadius - smallArcSectorRadius);
                top = smallArcPoint.Y + (smallArcSectorRadius - largeArcSectorRadius) * Math.Cos(sectorRadian / 2);
            }

            return new Point(left, top);
        }

        private static Point CalculateLargeArcTexturePoint(Point largeArcPoint, double smallArcSectorRadius, double largeArcSectorRadius, double sectorRadian)
        {
            var left = 0d;
            var top = 0d;

            if (DoubleUtil.LessThan(sectorRadian, Math.PI))
            {
                left = largeArcPoint.X;
                top = largeArcPoint.Y - smallArcSectorRadius * Math.Cos(sectorRadian / 2);
            }
            else
            {
                left = largeArcPoint.X;
                top = largeArcPoint.Y;
            }

            return new Point(left, top);
        }

        private static void CalculateSectorRadius(double topRadius, double bottomRadius, double height, out double topSectorRadius, out double bottomSectorRadius)
        {
            if (DoubleUtil.AreClose(topRadius, bottomRadius))
                throw new InvalidOperationException("Top radius is same as the bottom radius.");

            if (DoubleUtil.LessThan(topRadius, bottomRadius))
            {
                topSectorRadius = Math.Sqrt(Math.Pow(topRadius, 2) + Math.Pow(topRadius * height / (bottomRadius - topRadius), 2));
                bottomSectorRadius = Math.Sqrt(Math.Pow(bottomRadius, 2) + Math.Pow(height + topRadius * height / (bottomRadius - topRadius), 2));
            }
            else
            {
                bottomSectorRadius = Math.Sqrt(Math.Pow(bottomRadius, 2) + Math.Pow(bottomRadius * height / (topRadius - bottomRadius), 2));
                topSectorRadius = Math.Sqrt(Math.Pow(topRadius, 2) + Math.Pow(height + bottomRadius * height / (topRadius - bottomRadius), 2));
            }
        }

        //According to the expanded cylinder (half-sector);
        /* 1.see half-sector as two total-sector
         * 2.deal with total-sector one by one
         * 3.calculate relative point(0--1)
         */
        private static Func<int, int, Point> GenerateSectorArcPositionFunc(double sectorRadius, double sectorRadian)
        {
            var leftRadian = (2 * Math.PI - sectorRadian) / 2;
            var w = DoubleUtil.GreaterThanOrClose(sectorRadian, Math.PI) ? sectorRadius : (sectorRadius * Math.Sin(leftRadian));
            var h = DoubleUtil.LessThanOrClose(sectorRadian, Math.PI) ? sectorRadius : (sectorRadius * Math.Cos(leftRadian));

            return (index, stacks) =>
            {
                var curRadian = leftRadian + sectorRadian * index / stacks;

                var y = (DoubleUtil.LessThanOrClose(sectorRadian, Math.PI) ? 0 : h) - sectorRadius * Math.Cos(curRadian);
                var x = w - sectorRadius * Math.Sin(curRadian);

                return new Point(x, y);
            };
        }

        private static Size CalculateHalfSectorSize(double topRadius, double bottomRadius, double height, double topSectorRadius, double bottomSectorRadius, double sectorRadian)
        {
            if (DoubleUtil.AreClose(topRadius, bottomRadius))
                throw new InvalidOperationException("Top radius is same as the bottom radius.");

            var h = 0d;
            var w = 0d;

            if (DoubleUtil.LessThan(topRadius, bottomRadius))
            {
                h = DoubleUtil.LessThan(sectorRadian, Math.PI) ? (bottomSectorRadius - topSectorRadius * Math.Cos(sectorRadian / 2))
                           : (bottomSectorRadius * (1 - Math.Cos(sectorRadian / 2)));

                w = DoubleUtil.LessThan(sectorRadian, Math.PI) ? (bottomSectorRadius * Math.Sin(sectorRadian / 2) * 2) : (bottomSectorRadius * 2);
            }
            else
            {
                h = DoubleUtil.LessThan(sectorRadian, Math.PI) ? (topSectorRadius - bottomSectorRadius * Math.Cos(sectorRadian / 2))
                           : (topSectorRadius * (1 - Math.Cos(sectorRadian / 2)));

                w = DoubleUtil.LessThan(sectorRadian, Math.PI) ? (topSectorRadius * Math.Sin(sectorRadian / 2) * 2) : (topSectorRadius * 2);
            }

            return new Size(w, h);
        }
    }
}
