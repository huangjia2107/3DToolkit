using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using ThreeDToolkit.Models;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var x = 40 * Math.Sin(2 * Math.PI * HorSlider.Value / 360);
            var z = 40 * Math.Cos(2 * Math.PI * HorSlider.Value / 360);

            camera.LookDirection = new Vector3D(-x, camera.LookDirection.Y, -z);
            camera.Position = new Point3D(x, camera.Position.Y, z);
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.LookDirection = new Vector3D(camera.LookDirection.X, -VerSlider.Value, camera.LookDirection.Z);
            camera.Position = new Point3D(camera.Position.X, VerSlider.Value, camera.Position.Z);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            var topPerimeter = 2 * Math.PI * TestCylinder.TopRadius;
            var bottomPerimeter = 2 * Math.PI * TestCylinder.BottomRadius;

            var pointList = TestHelper.GenerateCylinderSideMesh(TestCylinder.TopRadius, TestCylinder.BottomRadius,
                new IsoscelesTrapezoid { TopWidth = topPerimeter, BottomWidth = bottomPerimeter, Height = TestCylinder.Height }, 200, true);

            (new TestWindow(pointList, TestImageBrush.ImageSource)).Show();
             * */
        }
    }
}
