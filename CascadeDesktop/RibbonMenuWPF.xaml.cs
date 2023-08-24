using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CascadeDesktop
{
    /// <summary>
    /// Interaction logic for RibbonMenuWPF.xaml
    /// </summary>
    public partial class RibbonMenuWPF : UserControl
    {
        public RibbonMenuWPF()
        {
            InitializeComponent();
            RibbonWin.Loaded += RibbonMenuWPF_Loaded;
        }

        private void RibbonMenuWPF_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ab.ShowDialog();

        }        
    }
}
