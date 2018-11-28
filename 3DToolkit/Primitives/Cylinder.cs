using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Media3D;
using ThreeDToolkit.Helps;
using System.Windows.Media;

namespace ThreeDToolkit.Primitives
{
    public class Cylinder : ModelVisual3D
    {
        private const int Stacks = 20;
        private const bool IsSharePoint = true;

        private GeometryModel3D _sideGeometryModel3D = null;
        private GeometryModel3D _topGeometryModel3D = null;
        private GeometryModel3D _bottomGeometryModel3D = null;

        #region Dependency Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Cylinder), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.ConstructModel3DGroup();
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Cylinder), new UIPropertyMetadata(8d, OnHeightPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.ConstructModel3DGroup();
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Cylinder), new PropertyMetadata(4d, OnRadiusPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.ConstructModel3DGroup();
        }

        private static object CoerceDistance(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        private static bool IsValidDoubleValue(object value)
        {
            var d = (double)value;
            return !(double.IsNaN(d) || double.IsInfinity(d));
        }

        public static readonly DependencyProperty SideMaterialProperty =
            DependencyProperty.Register("SideMaterial", typeof(Material), typeof(Cylinder), new PropertyMetadata(new DiffuseMaterial(Brushes.LightGray), OnSideMaterialPropertyChanged));
        public Material SideMaterial
        {
            get { return (Material)GetValue(SideMaterialProperty); }
            set { SetValue(SideMaterialProperty, value); }
        }

        static void OnSideMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.UpdateMaterial(ctrl._sideGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty TopMaterialProperty =
            DependencyProperty.Register("TopMaterial", typeof(Material), typeof(Cylinder), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnTopMaterialPropertyChanged));
        public Material TopMaterial
        {
            get { return (Material)GetValue(TopMaterialProperty); }
            set { SetValue(TopMaterialProperty, value); }
        }
        static void OnTopMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.UpdateMaterial(ctrl._topGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty BottomMaterialProperty =
            DependencyProperty.Register("BottomMaterial", typeof(Material), typeof(Cylinder), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnBottomMaterialPropertyChanged));
        public Material BottomMaterial
        {
            get { return (Material)GetValue(BottomMaterialProperty); }
            set { SetValue(BottomMaterialProperty, value); }
        }
        static void OnBottomMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cylinder;
            ctrl.UpdateMaterial(ctrl._bottomGeometryModel3D, (Material)e.NewValue);
        }

        #endregion

        private void UpdateMaterial(GeometryModel3D geometryModel3D, Material material)
        {
            if (geometryModel3D == null)
                ConstructModel3DGroup();
            else
                geometryModel3D.Material = material;
        }

        private void ConstructModel3DGroup()
        {
            if (DoubleUtil.LessThanOrClose(Radius, 0) || DoubleUtil.LessThanOrClose(Height, 0))
                return;

            var modelGroup = new Model3DGroup();

            //side
            var cylinderSideMesh = GenerateCylinderSideMesh(Origin, Radius, Height, IsSharePoint);
            _sideGeometryModel3D = new GeometryModel3D
            {
                Geometry = cylinderSideMesh,
                Material = SideMaterial
            };

            //Top
            _topGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateCylinderTopMesh(cylinderSideMesh.Positions.Where((p3D, i) => i % 2 != 0), Radius, new Point3D(Origin.X, Origin.Y + Height, Origin.Z), IsSharePoint),
                Material = TopMaterial
            };

            //Bottom
            _bottomGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateCylinderBottomMesh(cylinderSideMesh.Positions.Where((p3D, i) => i % 2 == 0), Radius, Origin, IsSharePoint),
                Material = BottomMaterial
            };

            modelGroup.Children.Add(_sideGeometryModel3D);
            modelGroup.Children.Add(_topGeometryModel3D);
            modelGroup.Children.Add(_bottomGeometryModel3D);

            this.Content = modelGroup;
        }

        //Relative Start Point3D : 0,0,Z --> 0,Y,Z
        private MeshGeometry3D GenerateCylinderSideMesh(Point3D origin, double radius, double height, bool isSharePoint = false)
        {
            var mesh = new MeshGeometry3D();

            double rad, rad_next;
            Point3D bp, tp, bp_next, tp_next;
            var indexBase = isSharePoint ? 2 : 4;

            for (int i = 0; i <= Stacks; i++)
            {
                rad = 2 * Math.PI * i / Stacks;
                rad_next = 2 * Math.PI * (i + 1) / Stacks;

                GeneratePosition(rad, origin, radius, height, out bp, out tp);
                mesh.Positions.Add(bp);
                mesh.Positions.Add(tp);

                mesh.TextureCoordinates.Add(new Point((double)i / Stacks, 1));
                mesh.TextureCoordinates.Add(new Point((double)i / Stacks, 0));

                if (i < Stacks)
                {
                    mesh.TriangleIndices.Add(i * indexBase);
                    mesh.TriangleIndices.Add(i * indexBase + 2);
                    mesh.TriangleIndices.Add(i * indexBase + 1);

                    mesh.TriangleIndices.Add(i * indexBase + 1);
                    mesh.TriangleIndices.Add(i * indexBase + 2);
                    mesh.TriangleIndices.Add(i * indexBase + 3);

                    if (!isSharePoint)
                    {
                        GeneratePosition(rad_next, origin, radius, height, out bp_next, out tp_next);
                        mesh.Positions.Add(bp_next);
                        mesh.Positions.Add(tp_next);

                        mesh.TextureCoordinates.Add(new Point((double)(i + 1) / Stacks, 1));
                        mesh.TextureCoordinates.Add(new Point((double)(i + 1) / Stacks, 0));

                        if (i == Stacks - 1)
                            break;
                    }
                }
            }

            return mesh;
        }

        private MeshGeometry3D GenerateCylinderTopMesh(IEnumerable<Point3D> positions, double radius, Point3D circleCenter, bool isSharePoint)
        {
            return GenerateCylinderBottomOrTopMesh(positions, radius, circleCenter, isSharePoint,
                    (mesh, i) =>
                    {
                        mesh.TriangleIndices.Add(i);
                        mesh.TriangleIndices.Add(i + 1);
                        mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
                    });
        }

        private MeshGeometry3D GenerateCylinderBottomMesh(IEnumerable<Point3D> positions, double radius, Point3D circleCenter, bool isSharePoint)
        {
            return GenerateCylinderBottomOrTopMesh(positions, radius, circleCenter, isSharePoint,
                    (mesh, i) =>
                    {
                        mesh.TriangleIndices.Add(i);
                        mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
                        mesh.TriangleIndices.Add(i + 1);
                    });
        }

        private MeshGeometry3D GenerateCylinderBottomOrTopMesh(IEnumerable<Point3D> positions, double radius, Point3D circleCenter, bool isSharePoint, Action<MeshGeometry3D, int> drawTriangleAction)
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(positions)
            };

            //center of a circle
            mesh.Positions.Add(circleCenter);

            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                /*Texture : real 2D position maps to 0-1
                  (0,0)         (1,0)
                  @-------------@
                  |             |
                  |             |
                  |             |
                  @-------------@
                  (0,1)         (1,1)
                 */
                var point3D = mesh.Positions[i];
                mesh.TextureCoordinates.Add(new Point((point3D.X - circleCenter.X + radius) / (2 * radius), (point3D.Z - circleCenter.Z + radius) / (2 * radius)));

                //Triangle
                if (isSharePoint && i <= mesh.Positions.Count - 3)
                {
                    drawTriangleAction(mesh, i);
                }

                if (!isSharePoint && i <= mesh.Positions.Count - 2 && i % 2 == 0)
                {
                    drawTriangleAction(mesh, i);
                }
            }

            return mesh;
        }

        private void GeneratePosition(double rad, Point3D origin, double radius, double height, out Point3D bp, out Point3D tp)
        {
            bp = new Point3D(radius * (-Math.Sin(rad)) + origin.X, origin.Y, radius *(-Math.Cos(rad)) + origin.Z);
            tp = new Point3D(bp.X, bp.Y + height, bp.Z);
        }
    }
}
