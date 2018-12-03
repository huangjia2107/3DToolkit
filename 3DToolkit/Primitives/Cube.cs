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
    public class Cube : ModelVisual3D
    {
        private Model3DGroup _modelGroup = null;

        private GeometryModel3D _leftGM3D = null;
        private GeometryModel3D _rightGM3D = null;
        private GeometryModel3D _topGM3D = null;
        private GeometryModel3D _bottomGM3D = null;
        private GeometryModel3D _frontGM3D = null;
        private GeometryModel3D _backGM3D = null;

        #region Dependency Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Cube), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cube;
            ctrl.ConstructModel3DGroup();
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Cube), new UIPropertyMetadata(8d, OnHeightPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cube;
            ctrl.ConstructModel3DGroup();
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Cube), new UIPropertyMetadata(8d, OnWidthPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cube;
            ctrl.ConstructModel3DGroup();
        }

        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register("Length", typeof(double), typeof(Cube), new UIPropertyMetadata(8d, OnLengthPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Length
        {
            get { return (double)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        static void OnLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cube;
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

        public static readonly DependencyProperty SideMaterialsProperty =
            DependencyProperty.Register("SideMaterials", typeof(MaterialGroup), typeof(Cube), new PropertyMetadata(null, OnSideMaterialsPropertyChanged));
        public MaterialGroup SideMaterials
        {
            get { return (MaterialGroup)GetValue(SideMaterialsProperty); }
            set { SetValue(SideMaterialsProperty, value); }
        }
        static void OnSideMaterialsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Cube;
            ctrl.UpdateSideMaterials((MaterialGroup)e.NewValue);
        }

        #endregion

        private void UpdateSideMaterials(MaterialGroup sideMaterials)
        {
            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
            {
                for (int i = 0; i < _modelGroup.Children.Count; i++)
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

        private void ConstructModel3DGroup()
        {
            if (DoubleUtil.LessThanOrClose(Width, 0) || DoubleUtil.LessThanOrClose(Height, 0) || DoubleUtil.LessThanOrClose(Length, 0))
                return;

            //Model3DGroup
            _modelGroup = new Model3DGroup();

            //side
            GenerateAllSideGeometryModel3D(Origin, Width, Height, Length);

            this.Content = _modelGroup;
        }

        private GeometryModel3D GenerateSideGeometryModel3D(Point3D origin, IEnumerable<Point3D> point3Ds, int materialIndex)
        {
            var gm3D = new GeometryModel3D
            {
                Geometry = GenerateSideMesh(origin, new Point3DCollection(point3Ds)),
                Material = new DiffuseMaterial(Brushes.Gray),
                BackMaterial = new DiffuseMaterial(Brushes.LightGray)
            };

            if (SideMaterials != null && SideMaterials.Children.Count > materialIndex)
                gm3D.Material = SideMaterials.Children[materialIndex];

            return gm3D;
        }

        //start point3D is left-back-bottom corner
        private void GenerateAllSideGeometryModel3D(Point3D origin, double width, double height, double length)
        {
            //left
            _leftGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(0, 0, 0), new Point3D(0, 0, width), new Point3D(0, height, 0), new Point3D(0, height, width) }), 0);

            //right
            _rightGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(length, 0, width), new Point3D(length, 0, 0), new Point3D(length, height, width), new Point3D(length, height, 0) }), 1);

            //top
            _topGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(0, height, width), new Point3D(length, height, width), new Point3D(0, height, 0), new Point3D(length, height, 0) }), 2);

            //bottom
            _bottomGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(length, 0, width), new Point3D(0, 0, width), new Point3D(length, 0, 0), new Point3D(0, 0, 0) }), 3);

            //front
            _frontGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(0, 0, width), new Point3D(length, 0, width), new Point3D(0, height, width), new Point3D(length, height, width) }), 4);

            //back
            _backGM3D = GenerateSideGeometryModel3D(origin,
                new Point3DCollection(new Point3D[] { new Point3D(length, 0, 0), new Point3D(0, 0, 0), new Point3D(length, height, 0), new Point3D(0, height, 0) }), 5);

            _modelGroup.Children.Add(_leftGM3D);
            _modelGroup.Children.Add(_rightGM3D);
            _modelGroup.Children.Add(_topGM3D);
            _modelGroup.Children.Add(_bottomGM3D);
            _modelGroup.Children.Add(_frontGM3D);
            _modelGroup.Children.Add(_backGM3D);
        }

        /*  add point3d by 0->1->2->3
            triangle by 0->1->2  2->1->3
        
            (2)            (3)
             @--------------@
             |              |
             |              |
             |              |
             |              |
             @--------------@
             (0)          (1)
        */
        private MeshGeometry3D GenerateSideMesh(Point3D origin, Point3DCollection point3Ds)
        {
            var mesh = new MeshGeometry3D();

            foreach (var p3D in point3Ds)
            {
                mesh.Positions.Add(new Point3D(p3D.X + origin.X, p3D.Y + origin.Y, p3D.Z + origin.Z));
            }

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);

            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));

            return mesh;
        }

    }
}
