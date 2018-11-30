using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using ThreeDToolkit.Helps;
using System.Windows.Media;
using System.Windows;
using ThreeDToolkit.Models;

namespace ThreeDToolkit.Primitives
{
    public class Cone : ModelVisual3D
    {
        private const int Stacks = 20;
        private const bool IsSharePoint = true;

        private Model3DGroup _modelGroup = null;

        private GeometryModel3D _sideGeometryModel3D = null;
        private GeometryModel3D _bottomGeometryModel3D = null;

        #region Dependency Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Cone), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cone;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Cone), new UIPropertyMetadata(8d, OnHeightPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cone;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Cone), new PropertyMetadata(4d, OnRadiusPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cone;
            ctrl.UpdateGeometry();
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
            DependencyProperty.Register("SideMaterial", typeof(Material), typeof(Cone), new PropertyMetadata(new DiffuseMaterial(Brushes.LightGray), OnSideMaterialPropertyChanged));
        public Material SideMaterial
        {
            get { return (Material)GetValue(SideMaterialProperty); }
            set { SetValue(SideMaterialProperty, value); }
        }

        static void OnSideMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cone;
            ctrl.UpdateMaterial(ctrl._sideGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty BottomMaterialProperty =
            DependencyProperty.Register("BottomMaterial", typeof(Material), typeof(Cone), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnBottomMaterialPropertyChanged));
        public Material BottomMaterial
        {
            get { return (Material)GetValue(BottomMaterialProperty); }
            set { SetValue(BottomMaterialProperty, value); }
        }
        static void OnBottomMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cone;
            ctrl.UpdateMaterial(ctrl._bottomGeometryModel3D, (Material)e.NewValue);
        }

        #endregion

        private void UpdateGeometry()
        {
            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
            {
                var sideMesh = GenerateSideMesh(Origin, Radius, Height, Stacks, IsSharePoint);

                _sideGeometryModel3D.Geometry = sideMesh;
                _bottomGeometryModel3D.Geometry = GenerateBottomMesh(sideMesh.Positions.Where((p3D, i) => i % 2 == 0), Radius, Origin, IsSharePoint);
            }
        }

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

            var sideMesh = GenerateSideMesh(Origin, Radius, Height, Stacks, IsSharePoint);

            //side
            _sideGeometryModel3D = new GeometryModel3D
            {
                Geometry = sideMesh,
                Material = SideMaterial
            };

            //Bottom
            _bottomGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateBottomMesh(sideMesh.Positions.Where((p3D, i) => i % 2 == 0), Radius, Origin, IsSharePoint),
                Material = BottomMaterial
            };

            //Model3DGroup
            _modelGroup = new Model3DGroup();
            _modelGroup.Children.Add(_sideGeometryModel3D);
            _modelGroup.Children.Add(_bottomGeometryModel3D);

            this.Content = _modelGroup;
        }

        private MeshGeometry3D GenerateBottomMesh(IEnumerable<Point3D> positions, double radius, Point3D circleCenter, bool isSharePoint)
        {
            return CylinderUtil.GenerateCircleMesh(positions, radius, circleCenter, isSharePoint,
                    (mesh, i) =>
                    {
                        mesh.TriangleIndices.Add(i);
                        mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
                        mesh.TriangleIndices.Add(i + 1);
                    });
        }

        private MeshGeometry3D GenerateSideMesh(Point3D origin, double radius, double height, int stacks = 20, bool isSharePoint = true)
        {
            var mesh = new MeshGeometry3D();

            Point3D bp, bp_next;
            var tp = new Point3D(origin.X, origin.Y + height, origin.Z);
            var indexBase = isSharePoint ? 2 : 4;

            var width = GetWidthFromExpandedCone(radius, height);
            var h1 = GetH1FromExpandedCone(radius, height, width);
            var h2 = GetH2FromExpandedCone(radius, height, width);

            for (int i = 0; i <= stacks; i++)
            {
                bp = CylinderUtil.GetPosition(
                        CylinderUtil.GetRadian(radius, 2 * Math.PI * radius, i, stacks),
                        origin, radius, 0);

                mesh.Positions.Add(bp);
                mesh.Positions.Add(tp);

                mesh.TextureCoordinates.Add(GetTexturePoint(radius, height, i, stacks, h1, h2));
                mesh.TextureCoordinates.Add(new Point(0.5, 0));

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
                        bp_next = CylinderUtil.GetPosition(
                                     CylinderUtil.GetRadian(radius, 2 * Math.PI * radius, i + 1, stacks),
                                     origin, radius, 0);

                        mesh.Positions.Add(bp_next);
                        mesh.Positions.Add(tp);

                        mesh.TextureCoordinates.Add(GetTexturePoint(radius, height, i + 1, stacks, h1, h2));
                        mesh.TextureCoordinates.Add(new Point(0.5, 0));

                        if (i == stacks - 1)
                            break;
                    }
                }
            }

            return mesh;
        }

        /*       0     1     2     3     4             0     1     2     3     4     5
         *   0   ._____._____._____._____.         0   ._____._____._____._____._____.
         *   1   +_____|_____|_____|_____+         1   +_____|_____|_____|_____|_____+
         *   2   +_____|_____|_____|_____+         2   +_____|_____|_____|_____|_____+
         *   3   +___________|___________+         3   +___________|_____|___________+
         *   4   +___________|___________+         4   +___________|_____|___________+
         *                                         5   +_____________________________+
         */
        private Point GetTexturePoint(double radius, double height, int index, int stacks, double h1, double h2)
        {
            var x = (double)index / stacks;
            var y = 0d;

            if (index <= stacks / 2)
            {
                y = (h1 + h2 / stacks * index * 2) / Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(height, 2));
            }
            else
            {
                y = (h1 + h2 / stacks * (stacks - index) * 2) / Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(height, 2));
            }

            return new Point(x, y);
        }

        //Width from the expanded cone;
        /*
         *           Width
         *       |-----------|
         *    ++++-----.-----+++                                                                   .++ 
         *       |    /|\    |                                                                    /|
         *       |   / | \   |                                                                   / |
         *    H1 |  /  |  \  |  <--  Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(height, 2))  -->  /  |  height
         *       | /   |   \ |                                                                 /   |
         *    ___|/    |    \|                                                                /    |
         *    H2 |     |     |                                                               /_____|
         *    ++++-----+-----+++                                                             +-----+++
         *                                                                                   radius
         */
        private double GetWidthFromExpandedCone(double radius, double height)
        {
            return Math.PI * radius * Math.Sqrt(4 * (Math.Pow(radius, 2) + Math.Pow(height, 2)) - Math.Pow(Math.PI, 2) * Math.Pow(radius, 2)) / Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(height, 2));
        }

        private double GetH1FromExpandedCone(double radius, double height, double width)
        {
            return Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(height, 2) - Math.Pow(width, 2) / 4);
        }

        private double GetH2FromExpandedCone(double radius, double height, double width)
        {
            return Math.Sqrt(Math.Pow(Math.PI, 2) * Math.Pow(radius, 2) - Math.Pow(width, 2) / 4);
        }
    }
}
