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
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        IEnumerable<Point> _points = null;

        public TestWindow(IEnumerable<Point> points,ImageSource source)
        {
            InitializeComponent();

            _points = points;

            image.Source = source;
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PointItemControl.ItemsSource = _points.Select(p => new { Left = PointItemControl.ActualWidth * p.X, Right = PointItemControl.ActualHeight * p.Y });
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PointItemControl.ItemsSource = _points.Select(p => new { Left = PointItemControl.ActualWidth * p.X, Right = PointItemControl.ActualHeight * p.Y });
        }
    }
}
