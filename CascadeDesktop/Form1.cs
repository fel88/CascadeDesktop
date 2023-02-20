using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Shown += Form1_Shown;
            SizeChanged += Form1_SizeChanged;
            Paint += Form1_Paint;
            panel1.Paint += Panel1_Paint;
            panel1.MouseWheel += Panel1_MouseWheel;
            panel1.MouseDown += Panel1_MouseDown;
            panel1.MouseUp += Panel1_MouseUp;
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            if (e.Button == MouseButtons.Left)
            {
                proxy.Select(ModifierKeys.HasFlag(Keys.Control));
            }
        }

        Point startDrag;
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            startDrag = e.Location;
            if (e.Button == MouseButtons.Right)
            {
                proxy.StartRotation(e.Location.X, e.Location.Y);
            }

            if (e.Button == MouseButtons.Left)
            {
                isDrag = true;
            }
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            proxy.Zoom(0, 0, e.Delta / 8, 0);
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            proxy.RedrawView();
            proxy.UpdateView();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            proxy.RedrawView();
            proxy.UpdateView();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            proxy.UpdateView();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            proxy = new OCCTProxy();
            proxy.InitOCCTProxy();

            
            if (!proxy.InitViewer(panel1.Handle))
            {

            }
            proxy.ActivateGrid(true);
            proxy.ShowCube();
            proxy.SetDisplayMode(1);
            proxy.SetMaterial(1);
            //proxy.SetDegenerateModeOff();
            proxy.RedrawView();


            proxy.UpdateCurrentViewer();
            proxy.UpdateView();

        }

        OCCTProxy proxy;

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            proxy.ZoomAllView();
            proxy.UpdateView();
            proxy.UpdateCurrentViewer();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //curForm.ImportModel(ModelFormat.STEP);
            // return;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (ofd.FileName.ToLower().EndsWith(".stp") || ofd.FileName.ToLower().EndsWith(".step"))
            {
                proxy.ImportStep(ofd.FileName);
            }
            if (ofd.FileName.ToLower().EndsWith(".igs") || ofd.FileName.ToLower().EndsWith(".iges"))
            {
                proxy.ImportIges(ofd.FileName);
            }
            proxy.SetDisplayMode(1);
            proxy.RedrawView();
            proxy.UpdateView();
            proxy.UpdateCurrentViewer();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            //proxy.Select(0, 0, 500, 500);
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.TopView();
        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.FrontView();
        }

        public void SetStatus(string text)
        {
            toolStripStatusLabel1.Text = text;
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();
        private void boxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("w", "Width", 50);
            d.AddNumericField("h", "Height", 50);
            d.AddNumericField("l", "Length", 50);

            d.ShowDialog();

            var w = d.GetNumericField("w");
            var h = d.GetNumericField("h");
            var l = d.GetNumericField("l");
            var cs = proxy.MakeBox(0, 0, 0, w, h, l);
            objs.Add(cs);
        }

        bool isDrag = false;
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                proxy.Rotation(e.Location.X, e.Location.Y);
            }
            if (e.Button == MouseButtons.Left && isDrag)
            {
                var delta = new Point(e.Location.X - startDrag.X, startDrag.Y - e.Location.Y);
                proxy.Pan(delta.X, delta.Y);
                startDrag = e.Location;
            }
            proxy.MoveTo(e.Location.X, e.Location.Y);

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            proxy.Erase(proxy.GetSelectedObject());
        }

        enum TopAbs_ShapeEnum
        {
            TopAbs_COMPOUND,
            TopAbs_COMPSOLID,
            TopAbs_SOLID,
            TopAbs_SHELL,
            TopAbs_FACE,
            TopAbs_WIRE,
            TopAbs_EDGE,
            TopAbs_VERTEX,
            TopAbs_SHAPE
        };

        private void faceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Face);
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.LeftView();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Step models (*.stp)|*.stp";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            proxy.ExportStep(proxy.GetSelectedObject(), sfd.FileName);
        }

        private void axoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.AxoView();
        }

        ManagedObjHandle obj1;
        ManagedObjHandle obj2;
        private void diffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeDiff(obj1, obj2);
            proxy.Erase(obj1);
            proxy.Erase(obj2);
        }

        private void unionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeFuse(obj1, obj2, true);
            proxy.Erase(obj1);
            proxy.Erase(obj2);
        }

        private void wireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Wire);

        }

        private void shapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Shape);

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                if (DialogHelpers.ShowQuestion("Are you sure to delete?", "Question"))
                {
                    proxy.Erase(proxy.GetSelectedObject());
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void vertexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Vertex);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }

        private void setObjAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            obj1 = proxy.GetSelectedObject();
        }

        private void setObjBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            obj2 = proxy.GetSelectedObject();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("x", "x", 0);
            d.AddNumericField("y", "y", 0);
            d.AddNumericField("z", "z", 0);

            d.ShowDialog();

            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");
            var z = d.GetNumericField("z");
            proxy.MoveObject(proxy.GetSelectedObject(), x, y, z, true);
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeCommon(obj1, obj2);
        }

        private void cylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 15);
            d.AddNumericField("h", "Height", 150);

            d.ShowDialog();

            var r = d.GetNumericField("r");
            var h = d.GetNumericField("h");

            var cs = proxy.MakeCylinder(r, h);
            objs.Add(cs);
        }

        private void sphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 50);

            d.ShowDialog();

            var r = d.GetNumericField("r");
            var cs = proxy.MakeSphere(r);
            objs.Add(cs);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("a", "Angle", 90);
            d.AddNumericField("x", "x", 0);
            d.AddNumericField("y", "y", 0);
            d.AddNumericField("z", "z", 1);

            d.ShowDialog();

            var ang = d.GetNumericField("a");
            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");
            var z = d.GetNumericField("z");

            proxy.RotateObject(proxy.GetSelectedObject(), x, y, z, ang * Math.PI / 180f, true);
        }

        bool grid = true;
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            grid = !grid;
            proxy.ActivateGrid(grid);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            foreach (var item in objs)
            {
                proxy.Erase(item);
            }
        }

        private void edgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Edge);
        }

        private void filletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 15);

            d.ShowDialog();

            var r = d.GetNumericField("r");
            var so = proxy.GetSelectedObject();
            var cs = proxy.MakeFillet(so, r);
            proxy.Erase(so);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Edge);
        }

        private void exportMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var res = proxy.IteratePoly(proxy.GetSelectedObject());            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<mesh>");
            for (int i = 0; i < res.Count; i += 3)
            {
                sb.AppendLine("<triangle>");
                for (int j = 0; j < 3; j++)
                {
                    sb.AppendLine($"<vertex x=\"{res[i + j].X}\" y=\"{res[i + j].Y}\" z=\"{res[i + j].Z}\"/>");
                }
                sb.AppendLine("</triangle>");
            }
            sb.AppendLine("</mesh>");
            sb.AppendLine("</root>");
            Clipboard.SetText(sb.ToString());
            SetStatus("exported to clipboard successfully");
        }

        private void extrudeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void draftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            obj1 = proxy.AddWireDraft(40);

        }
    }
}
