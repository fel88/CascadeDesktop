using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace CascadeDesktop
{
    public partial class RibbonMenu : UserControl
    {
        public RibbonMenu()
        {
            InitializeComponent();
            ElementHost elementHost1 = new ElementHost();
            Ribbon = new RibbonMenuWPF();
            Ribbon.DataContext = Form1.Form;
            elementHost1.Child = Ribbon;
            Controls.Add(elementHost1);
            elementHost1.Dock = DockStyle.Fill;
            elementHost1.AutoSize = true;
        }
        public RibbonMenuWPF Ribbon;

    }
}
