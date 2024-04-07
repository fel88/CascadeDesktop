﻿using CascadeDesktop.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
    public partial class RibbonMenuWPF : System.Windows.Controls.UserControl
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
        public Form1 Form => Form1.Form;

        private void fitAll_Click(object sender, RoutedEventArgs e)
        {
            Form.ZoomAll();
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            Form.AddBox();
        }

        private void Adjoin_Click(object sender, RoutedEventArgs e)
        {
            Form.AdjoinTool(false);
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            Form.GridSwitch();
            Form.Proxy.UpdateCurrentViewer();
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            Form.MoveSelected();
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            Form.RotateSelected();
        }

        private void Face_Click(object sender, RoutedEventArgs e)
        {
            Form.FaceSelectionMode();
        }

        private void Vertex_Click(object sender, RoutedEventArgs e)
        {
            Form.VertexSelectionMode();

        }

        private void Edge_Click(object sender, RoutedEventArgs e)
        {
            Form.EdgeSelectionMode();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            Form.ImportModel();
        }

        private void StepExport_Click(object sender, RoutedEventArgs e)
        {
            Form.ExportSelectedToStep();
        }



        private void ObjExport_Click(object sender, RoutedEventArgs e)
        {
            Form.ExportSelectedToObj();

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Form.DeleteSelected();
        }

        private void Fuse_Click(object sender, RoutedEventArgs e)
        {
            Form.FuseTool();
        }

        private void ImportDraft_Click(object sender, RoutedEventArgs e)
        {
            Form.ImportDraft();
        }

        private void DrawDraft_Click(object sender, RoutedEventArgs e)
        {
            Form.DrawDraft();
        }

        private void Wire_Click(object sender, RoutedEventArgs e)
        {
            Form.WireSelectionMode();
        }

        private void Shape_Click(object sender, RoutedEventArgs e)
        {
            Form.ShapeSelectionMode();
        }

        private void Fillet_Click(object sender, RoutedEventArgs e)
        {
            Form.Fillet();
        }

        private void Clone_Click(object sender, RoutedEventArgs e)
        {
            Form.Clone();
        }

        private void Cylinder_Click(object sender, RoutedEventArgs e)
        {
            Form.AddCylinder();
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            Form.ResetView();
        }

        private void Diff_Click(object sender, RoutedEventArgs e)
        {
            Form.DiffTool();
        }

        private void Intersect_Click(object sender, RoutedEventArgs e)
        {
            Form.CommonTool();
        }
        private void FrontView_Click(object sender, RoutedEventArgs e)
        {

            Form.FrontView();
        }

        private void TopView_Click(object sender, RoutedEventArgs e)
        {
            Form.TopView();
        }

        private void RightView_Click(object sender, RoutedEventArgs e)
        {
            Form.RightView();
        }

        private void BottomView_Click(object sender, RoutedEventArgs e)
        {
            Form.BottomView();
        }

        private void LeftView_Click(object sender, RoutedEventArgs e)
        {
            Form.LeftView();
        }

        private void BackView_Click(object sender, RoutedEventArgs e)
        {
            Form.BackView();
        }

        private void Extrude_Click(object sender, RoutedEventArgs e)
        {
            Form.Extrude();
        }

        private void Chamfer_Click(object sender, RoutedEventArgs e)
        {
            Form.Chamfer();
        }

        private void Sphere_Click(object sender, RoutedEventArgs e)
        {
            Form.AddSphere();
        }

        private void Cone_CLick(object sender, RoutedEventArgs e)
        {
            Form.AddCone();
        }

        private void DarkBackground_Click(object sender, RoutedEventArgs e)
        {
            Form.SetDarkBackground();
        }

        private void LightBackground_Click(object sender, RoutedEventArgs e)
        {
            Form.SetLightBackground();

        }

        private void FacesInfo_Click(object sender, RoutedEventArgs e)
        {
            Form.FacesInfo();
        }

        private void Mirror_Click(object sender, RoutedEventArgs e)
        {
            Form.MirrorSelected();
        }

        private void AdjoinWithDistance_Click(object sender, RoutedEventArgs e)
        {
            Form.AdjoinTool(true);
        }

        private void Ruler_Click(object sender, RoutedEventArgs e)
        {
            Form.RulerTool();

        }

        private void AdjoinCOMs_Click(object sender, RoutedEventArgs e)
        {
            Form.AdjoinCOMsTool();
        }

        private void Selection_Click(object sender, RoutedEventArgs e)
        {
            Form.SelectionTool();
        }

        private void ExtrudeFace_Click(object sender, RoutedEventArgs e)
        {
            Form.ExtrudeFace();
        }

        private void transparency_Click(object sender, RoutedEventArgs e)
        {
            Form.Transparency();
        }

        private void setName_Click(object sender, RoutedEventArgs e)
        {
            Form.RenameSelected();
        }


        private void open_Click(object sender, RoutedEventArgs e)
        {
            Form.OpenProject();
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            Form.NewProject();
        }

        private void saveAs_Click(object sender, RoutedEventArgs e)
        {
            Form.SaveAsProject();
        }


        private void color_Click(object sender, RoutedEventArgs e)
        {
            Form.SetColor();
        }

        private void objectsList_Click(object sender, RoutedEventArgs e)
        {
            Form.ShowObjectsList();
        }

        private void wireframe_Click(object sender, RoutedEventArgs e)
        {
            Form.Wireframe();
        }
    }
}
