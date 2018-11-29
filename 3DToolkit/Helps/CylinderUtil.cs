using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ThreeDToolkit.Helps
{
    public static class CylinderUtil
    {
        private static MeshGeometry3D GenerateCylinderSideMesh(Point3D origin, double height, double topRadius, double bottomRadius, Rect range, int stacks = 20, bool isSharePoint = false)
        {
            var mesh = new MeshGeometry3D();

            double rad, rad_next;
            Point3D bp, tp, bp_next, tp_next;
            var indexBase = isSharePoint ? 2 : 4;

            for (int i = 0; i <= stacks; i++)
            {
                rad = range.X / bottomRadius + range.Width * i / stacks / bottomRadius;

                GeneratePosition(rad, origin, bottomRadius, height, range, out bp, out tp);
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
                        rad_next = range.X / bottomRadius + range.Width * (i + 1) / stacks / bottomRadius;

                        GeneratePosition(rad_next, origin, bottomRadius, height, range, out bp_next, out tp_next);
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

        public static void GeneratePosition(double rad, Point3D origin, double radius, double height, Rect range, out Point3D bp, out Point3D tp)
        {
            bp = new Point3D(radius * (-Math.Sin(rad)) + origin.X, height - range.Y - range.Height + origin.Y, radius * (-Math.Cos(rad)) + origin.Z);
            tp = new Point3D(bp.X, bp.Y + range.Height, bp.Z);
        }
    }
}
