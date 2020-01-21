using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;
using ThreeDToolkit.Helps;

namespace ThreeDToolkit.Primitives
{
    public class Location : ModelVisual3D
    {
        private readonly int Stacks = 20;
        private readonly List<MeshPoint3D> _meshPoint3ds = new List<MeshPoint3D>();

        private Model3DGroup _modelGroup = null;

        private GeometryModel3D _innerSideGeometryModel3D = null;
        private GeometryModel3D _outterSideGeometryModel3D = null;
        private GeometryModel3D _topGeometryModel3D = null;
        private GeometryModel3D _bottomGeometryModel3D = null;

        struct MeshPoint3D
        {
            public Point3D InnerTop { get; set; }
            public Point3D InnerBottom { get; set; }
            public Point3D OutterTop { get; set; }
            public Point3D OutterBottom { get; set; }
        }

        #region Basic Property

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point3D), typeof(Location), new UIPropertyMetadata(new Point3D(0, 0, 0), OnOriginPropertyChanged));
        public Point3D Origin
        {
            get { return (Point3D)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }
        static void OnOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Location), new UIPropertyMetadata(8d, OnHeightPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Location), new PropertyMetadata(4d, OnRadiusPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateGeometry();
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(Location), new PropertyMetadata(3d, OnThicknessPropertyChanged, CoerceDistance), IsValidDoubleValue);
        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }
        static void OnThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
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

        #endregion

        #region Material

        public static readonly DependencyProperty InnerSideMaterialProperty =
            DependencyProperty.Register("InnerSideMaterial", typeof(Material), typeof(Location), new PropertyMetadata(new DiffuseMaterial(Brushes.LightGray), OnInnerSideMaterialPropertyChanged));
        public Material InnerSideMaterial
        {
            get { return (Material)GetValue(InnerSideMaterialProperty); }
            set { SetValue(InnerSideMaterialProperty, value); }
        }
        static void OnInnerSideMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateMaterial(ctrl._innerSideGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty OutterSideMaterialProperty =
            DependencyProperty.Register("OutterSideMaterial", typeof(Material), typeof(Location), new PropertyMetadata(new DiffuseMaterial(Brushes.LightGray), OnOutterSideMaterialPropertyChanged));
        public Material OutterSideMaterial
        {
            get { return (Material)GetValue(OutterSideMaterialProperty); }
            set { SetValue(OutterSideMaterialProperty, value); }
        }
        static void OnOutterSideMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateMaterial(ctrl._outterSideGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty TopMaterialProperty =
            DependencyProperty.Register("TopMaterial", typeof(Material), typeof(Location), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnTopMaterialPropertyChanged));
        public Material TopMaterial
        {
            get { return (Material)GetValue(TopMaterialProperty); }
            set { SetValue(TopMaterialProperty, value); }
        }
        static void OnTopMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateMaterial(ctrl._topGeometryModel3D, (Material)e.NewValue);
        }

        public static readonly DependencyProperty BottomMaterialProperty =
            DependencyProperty.Register("BottomMaterial", typeof(Material), typeof(Location), new PropertyMetadata(new DiffuseMaterial(Brushes.Gray), OnBottomMaterialPropertyChanged));
        public Material BottomMaterial
        {
            get { return (Material)GetValue(BottomMaterialProperty); }
            set { SetValue(BottomMaterialProperty, value); }
        }
        static void OnBottomMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Location;
            ctrl.UpdateMaterial(ctrl._bottomGeometryModel3D, (Material)e.NewValue);
        }

        #endregion

        private void UpdateGeometry()
        {
            if (!GenerateMeshPoint3Ds())
            {
                this.Content = null;
                return;
            }

            if (_modelGroup == null)
                ConstructModel3DGroup();
            else
            {
                _innerSideGeometryModel3D.Geometry = GenerateSideMesh(true);
                _outterSideGeometryModel3D.Geometry = GenerateSideMesh(false);
                _topGeometryModel3D.Geometry = GenerateTopMesh(true);
                _bottomGeometryModel3D.Geometry = GenerateTopMesh(false);
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
            if (DoubleUtil.LessThanOrClose(Radius, 0) || DoubleUtil.LessThanOrClose(Height, 0) || DoubleUtil.LessThanOrClose(Thickness, 0))
                return;

            //Inner Side
            _innerSideGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateSideMesh(true),
                Material = InnerSideMaterial
            };

            //Outter Side
            _outterSideGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateSideMesh(false),
                Material = OutterSideMaterial
            };

            //Top
            _topGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateTopMesh(true),
                Material = TopMaterial
            };

            //Bottom
            _bottomGeometryModel3D = new GeometryModel3D
            {
                Geometry = GenerateTopMesh(false),
                Material = BottomMaterial
            };

            //Model3DGroup
            _modelGroup = new Model3DGroup();
            _modelGroup.Children.Add(_innerSideGeometryModel3D);
            _modelGroup.Children.Add(_outterSideGeometryModel3D);
            _modelGroup.Children.Add(_topGeometryModel3D);
            _modelGroup.Children.Add(_bottomGeometryModel3D);

            this.Content = _modelGroup;
        }

        private double AngleToRadian(double angle)
        {
            return Math.PI / 180 * angle;
        }

        private bool GenerateMeshPoint3Ds()
        {
            if (DoubleUtil.LessThanOrClose(Radius, 0) || DoubleUtil.LessThanOrClose(Height, 0) || DoubleUtil.LessThanOrClose(Thickness, 0))
                return false;

            _meshPoint3ds.Clear();

            var innerRadius = Radius;
            var outterRadius = Radius + Thickness;

            for (int i = 0; i <= Stacks; i++)
            {
                var radian = AngleToRadian(360d / Stacks * i);

                var ix = innerRadius * Math.Cos(radian);
                var ox = outterRadius * Math.Cos(radian);

                var iz = -innerRadius * Math.Sin(radian);
                var oz = -outterRadius * Math.Sin(radian);

                _meshPoint3ds.Add(new MeshPoint3D
                {
                    InnerBottom = new Point3D(Origin.X + ix, Origin.Y, Origin.Z + iz),
                    InnerTop = new Point3D(Origin.X + ix, Origin.Y + Height, Origin.Z + iz),

                    OutterBottom = new Point3D(Origin.X + ox, Origin.Y, Origin.Z + oz),
                    OutterTop = new Point3D(Origin.X + ox, Origin.Y + Height, Origin.Z + oz),
                });
            }

            return true;
        }

        private MeshGeometry3D GenerateSideMesh(bool isInner)
        {
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= Stacks; i++)
            {
                var mp = _meshPoint3ds[i];

                //used for Triangle
                mesh.Positions.Add(isInner ? mp.InnerBottom : mp.OutterBottom);
                mesh.Positions.Add(isInner ? mp.InnerTop : mp.OutterTop);

                //used for Texture
                var relativeX = 360d / Stacks * i;
                mesh.TextureCoordinates.Add(new Point(relativeX, 1));
                mesh.TextureCoordinates.Add(new Point(relativeX, 0));

                if (i < Stacks)
                {
                    if (isInner)
                    {
                        mesh.TriangleIndices.Add(i * 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                        mesh.TriangleIndices.Add(i * 2 + 2);

                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                        mesh.TriangleIndices.Add(i * 2 + 3);
                    }
                    else
                    {
                        mesh.TriangleIndices.Add(i * 2);
                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);

                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 3);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                    }
                }
            }

            return mesh;
        }

        private MeshGeometry3D GenerateTopMesh(bool isTop)
        {
            var mesh = new MeshGeometry3D();

            var innerRadius = Radius;
            var outterRadius = Radius + Thickness;

            for (int i = 0; i <= Stacks; i++)
            {
                var mp = _meshPoint3ds[i];

                //used for Triangle
                mesh.Positions.Add(isTop ? mp.InnerTop : mp.InnerBottom);
                mesh.Positions.Add(isTop ? mp.OutterTop : mp.OutterBottom);

                //used for Texture
                var radian = AngleToRadian(360d / Stacks * i);
                var cos = Math.Cos(radian);
                var sin = Math.Sin(radian);
                mesh.TextureCoordinates.Add(new Point((outterRadius + innerRadius * cos) / (2 * outterRadius), (outterRadius - innerRadius * sin) / (2 * outterRadius)));
                mesh.TextureCoordinates.Add(new Point((outterRadius + outterRadius * cos) / (2 * outterRadius), (outterRadius - innerRadius * sin) / (2 * outterRadius)));

                if (i < Stacks)
                {
                    if (isTop)
                    {
                        mesh.TriangleIndices.Add(i * 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                        mesh.TriangleIndices.Add(i * 2 + 2);

                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                        mesh.TriangleIndices.Add(i * 2 + 3);
                    }
                    else
                    {
                        mesh.TriangleIndices.Add(i * 2);
                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 1);

                        mesh.TriangleIndices.Add(i * 2 + 2);
                        mesh.TriangleIndices.Add(i * 2 + 3);
                        mesh.TriangleIndices.Add(i * 2 + 1);
                    }
                }
            }

            return mesh;
        }
    }
}
