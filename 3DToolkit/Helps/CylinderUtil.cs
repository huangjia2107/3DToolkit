using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using ThreeDToolkit.Models;

namespace ThreeDToolkit.Helps
{
    public static class CylinderUtil
    {
        //Relative Start Point3D : 0,0,Z --> 0,Y,Z     
        public static MeshGeometry3D GenerateCylinderSideMesh(Point3D origin, double topRadius, double bottomRadius, IsoscelesTrapezoid trapezoid, int stacks = 20, bool isSharePoint = false)
        {
            var mesh = new MeshGeometry3D();

            Point3D bp, tp, bp_next, tp_next;
            var indexBase = isSharePoint ? 2 : 4;

            for (int i = 0; i <= stacks; i++)
            {
                bp = GetPosition(
                        GetRadian(bottomRadius, trapezoid.BottomWidth, i, stacks),
                        origin, bottomRadius, 0);

                tp = GetPosition(
                        GetRadian(topRadius, trapezoid.TopWidth, i, stacks),
                        origin, topRadius, trapezoid.Height);

                mesh.Positions.Add(bp);
                mesh.Positions.Add(tp);

                mesh.TextureCoordinates.Add(new Point((double)i / stacks, 1));
                mesh.TextureCoordinates.Add(new Point((double)i / stacks, 0));

                if (i < stacks)
                {
                    mesh.TriangleIndices.Add(i * indexBase);
                    mesh.TriangleIndices.Add(i * indexBase + 2);
                    mesh.TriangleIndices.Add(i * indexBase + 1);

                    mesh.TriangleIndices.Add(i * indexBase + 1);
                    mesh.TriangleIndices.Add(i * indexBase + 2);
                    mesh.TriangleIndices.Add(i * indexBase + 3);

                    if (!isSharePoint)
                    {
                        bp_next = GetPosition(
                            GetRadian(bottomRadius, trapezoid.BottomWidth, i + 1, stacks),
                            origin, bottomRadius, 0);

                        tp_next = GetPosition(
                            GetRadian(topRadius, trapezoid.TopWidth, i + 1, stacks),
                            origin, topRadius, trapezoid.Height);

                        mesh.Positions.Add(bp_next);
                        mesh.Positions.Add(tp_next);

                        mesh.TextureCoordinates.Add(new Point((double)(i + 1) / stacks, 1));
                        mesh.TextureCoordinates.Add(new Point((double)(i + 1) / stacks, 0));

                        if (i == stacks - 1)
                            break;
                    }
                }
            }

            return mesh;
        }

        private static double GetRadian(double radius, double arcLength, int index, int stacks)
        {
            return Math.Min(2 * Math.PI * radius, arcLength) * index / stacks / radius;
        }

        private static Point3D GetPosition(double rad, Point3D origin, double radius, double y)
        {
            return new Point3D(radius * (-Math.Sin(rad)) + origin.X, y + origin.Y, radius * (-Math.Cos(rad)) + origin.Z);
        }

    }

}
