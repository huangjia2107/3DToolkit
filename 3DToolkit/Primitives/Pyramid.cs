using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using ThreeDToolkit.Helps;
using System.Windows.Media;
using System.Windows;

namespace ThreeDToolkit.Primitives
{
    public class Pyramid : ModelVisual3D
    {
        private Model3DGroup _modelGroup = null;
        private GeometryModel3D _bottomGeometryModel3D = null;

        #region Dependency Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Pyramid), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Pyramid), new UIPropertyMetadata(8d, OnHeightPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty SidesProperty =
           DependencyProperty.Register("Sides", typeof(int), typeof(Pyramid), new PropertyMetadata(3, OnSidesPropertyChanged, CoerceSides));
        public int Sides
        {
            get { return (int)GetValue(SidesProperty); }
            set { SetValue(SidesProperty, value); }
        }
        static void OnSidesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
            ctrl.ConstructModel3DGroup();
        }

        private static object CoerceSides(DependencyObject d, object value)
        {
            return Math.Max(3, (int)value);
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Pyramid), new PropertyMetadata(4d, OnRadiusPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
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

        public static readonly DependencyProperty SideMaterialsProperty =
            DependencyProperty.Register("SideMaterials", typeof(MaterialGroup), typeof(Pyramid), new PropertyMetadata(null, OnSideMaterialsPropertyChanged));
        public MaterialGroup SideMaterials
        {
            get { return (MaterialGroup)GetValue(SideMaterialsProperty); }
            set { SetValue(SideMaterialsProperty, value); }
        }
        static void OnSideMaterialsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
            ctrl.UpdateSideMaterials((MaterialGroup)e.NewValue);
        }

        public static readonly DependencyProperty BottomMaterialProperty =
            DependencyProperty.Register("BottomMaterial", typeof(Material), typeof(Pyramid), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnBottomMaterialPropertyChanged));
        public Material BottomMaterial
        {
            get { return (Material)GetValue(BottomMaterialProperty); }
            set { SetValue(BottomMaterialProperty, value); }
        }
        static void OnBottomMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Pyramid;
            ctrl.UpdateBottomMaterial(ctrl._bottomGeometryModel3D, (Material)e.NewValue);
        }

        #endregion

        private void UpdateGeometry()
        {
            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
            {
                var bottomPoints = new List<Point3D>();
                foreach (var gm3D in GenerateAllSideGeometryModel3D(Origin, Radius, Height, Sides))
                {
                    bottomPoints.AddRange((gm3D.Geometry as MeshGeometry3D).Positions.Where((p3D, i) => i > 0));
                    (_modelGroup.Children[bottomPoints.Count / 2 - 1] as GeometryModel3D).Geometry = gm3D.Geometry;
                }

                _bottomGeometryModel3D.Geometry = GenerateBottomMesh(bottomPoints, Radius, Origin);
            }
        }

        private void UpdateSideMaterials(MaterialGroup sideMaterials)
        {
            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
            {
                for (int i = 0; i < _modelGroup.Children.Count - 1; i++)
                {
                    var gm3D = _modelGroup.Children[i] as GeometryModel3D;

                    if (sideMaterials == null)
                        gm3D.Material = new DiffuseMaterial(Brushes.Gray);
                    else
                    {
                        if (sideMaterials.Children.Count > i)
                            gm3D.Material = sideMaterials.Children[i];
                    }
                }
            }
        }

        private void UpdateBottomMaterial(GeometryModel3D geometryModel3D, Material material)
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

            //Model3DGroup
            _modelGroup = new Model3DGroup();

            //side
            var bottomPoints = new List<Point3D>();
            foreach (var gm3D in GenerateAllSideGeometryModel3D(Origin, Radius, Height, Sides))
            {
                bottomPoints.AddRange((gm3D.Geometry as MeshGeometry3D).Positions.Where((p3D, i) => i > 0));
                _modelGroup.Children.Add(gm3D);
            }

            //Bottom
            _bottomGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateBottomMesh(bottomPoints, Radius, Origin),
                Material = BottomMaterial
            };

            _modelGroup.Children.Add(_bottomGeometryModel3D);

            this.Content = _modelGroup;
        }

        private MeshGeometry3D GenerateBottomMesh(IEnumerable<Point3D> positions, double radius, Point3D circleCenter)
        {
            return CylinderUtil.GenerateCircleMesh(positions, radius, circleCenter, false,
                    (mesh, i) =>
                    {
                        mesh.TriangleIndices.Add(i);
                        mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
                        mesh.TriangleIndices.Add(i + 1);
                    });
        }

        private MeshGeometry3D GenerateSideMesh(Point3D origin, double radius, double height, int sideIndex, int sides)
        {
            var mesh = new MeshGeometry3D();

            //top
            var tp = new Point3D(origin.X, origin.Y + height, origin.Z);

            //left point of bottom
            var bp_left = CylinderUtil.GetPosition(
                             CylinderUtil.GetRadian(radius, 2 * Math.PI * radius, sideIndex, sides),
                             origin, radius, 0);

            //right point of bottom
            var bp_right = CylinderUtil.GetPosition(
                              CylinderUtil.GetRadian(radius, 2 * Math.PI * radius, sideIndex + 1, sides),
                              origin, radius, 0);

            mesh.Positions.Add(tp);
            mesh.Positions.Add(bp_left);
            mesh.Positions.Add(bp_right);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TextureCoordinates.Add(new Point(0.5, 0));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));

            return mesh;
        }

        private IEnumerable<GeometryModel3D> GenerateAllSideGeometryModel3D(Point3D origin, double radius, double height, int sides)
        {
            for (int i = 0; i < sides; i++)
            {
                var gm3D = new GeometryModel3D
                {
                    Geometry = GenerateSideMesh(origin, radius, height, i, sides),
                    Material = new DiffuseMaterial(Brushes.Gray),
                    BackMaterial = new DiffuseMaterial(Brushes.LightGray)
                };

                if (SideMaterials != null)
                {
                    if (SideMaterials.Children.Count > i)
                        gm3D.Material = SideMaterials.Children[i];
                }

                yield return gm3D;
            }
        }
    }
}
