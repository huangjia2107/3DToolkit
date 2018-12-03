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
    public class Sphere : ModelVisual3D
    {
        private const int Stacks = 20;
        private const int Slices = 20;

        private Model3DGroup _modelGroup = null;
        private GeometryModel3D _geometryModel3D = null;

        #region Dependency Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Sphere), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Sphere;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Sphere), new PropertyMetadata(4d, OnRadiusPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Sphere;
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

        public static readonly DependencyProperty MaterialProperty =
            DependencyProperty.Register("Material", typeof(Material), typeof(Sphere), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnMaterialPropertyChanged));
        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }
        static void OnMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Sphere;
            ctrl.UpdateMaterial(ctrl._geometryModel3D, (Material)e.NewValue);
        }

        #endregion

        private void UpdateGeometry()
        {
            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
                _geometryModel3D.Geometry = GenerateMeshGeometry3D(Origin, Radius, Stacks, Slices);
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
            if (DoubleUtil.LessThanOrClose(Radius, 0))
                return;

            //Model3DGroup
            _modelGroup = new Model3DGroup();

            //Bottom
            _geometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateMeshGeometry3D(Origin, Radius, Stacks, Slices),
                Material = Material
            };

            _modelGroup.Children.Add(_geometryModel3D);

            this.Content = _modelGroup;
        }

        /*              
         * Horizontal Direction  (0,0,-z) -> (-x,0,0)  -> (0,0,z) -> (x,0,0)
         * Vertical Direction    (0,y,0)  -> (0,-y,0)
         * 
         * phi [0-2*Math.PI]   theta [0-Math.PI]
         */
        private MeshGeometry3D GenerateMeshGeometry3D(Point3D origin, double radius, int stacks, int slices)
        {
            var mesh = new MeshGeometry3D();

            double phi, theta, x, y, z;

            for (int i = 0; i <= slices; i++)
            {
                theta = Math.PI * i / slices;
                y = radius * Math.Cos(theta);

                for (int j = 0; j <= stacks; j++)
                {
                    phi = 2 * Math.PI * j / stacks;
                    x = -radius * Math.Sin(theta) * Math.Sin(phi);
                    z = -radius * Math.Sin(theta) * Math.Cos(phi);

                    mesh.Positions.Add(new Point3D(x + origin.X, y + origin.Y, z + origin.Z));
                    mesh.TextureCoordinates.Add(new Point((double)j / stacks, (double)i / slices));

                    if (i > 0 && j < stacks)
                    {
                        mesh.TriangleIndices.Add(i * (stacks + 1) + j);
                        mesh.TriangleIndices.Add(i * (stacks + 1) + j + 1);
                        mesh.TriangleIndices.Add((i - 1) * (stacks + 1) + j);

                        mesh.TriangleIndices.Add((i - 1) * (stacks + 1) + j);
                        mesh.TriangleIndices.Add(i * (stacks + 1) + j + 1);
                        mesh.TriangleIndices.Add((i - 1) * (stacks + 1) + j + 1);
                    }
                }
            }

            return mesh;
        }
    }
}
