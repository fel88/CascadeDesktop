using System;
using System.Drawing;
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
        }

        Point startDrag;
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            startDrag = e.Location;
            proxy.Select();
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
            proxy.Select(0, 0, 500, 500);
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.TopView();
        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.FrontView();
        }

        private void boxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeBox(0, 0, 0, 100, 100, 100);
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

        private void boxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var cs1 = proxy.MakeBox(0, 0, 0, 100, 100, 100);
            var cs2 = proxy.MakeBox(50, 50, 50, 150, 150, 150);
            /*proxy.MakeDiff(cs1, cs2);
            proxy.Erase(cs1);
            proxy.Erase(cs2);*/
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.LeftView();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            proxy.ExportStep(sfd.FileName);
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
        }

        private void unionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeFuse(obj1, obj2);
        }

        private void wireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Wire);

        }

        private void shapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Shape);

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

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
            proxy.MoveObject(proxy.GetSelectedObject(), 50, 50, 50, true);
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeCommon(obj1, obj2);
        }

        private void cylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeCylinder(15, 100);
        }

        private void sphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeSphere(15);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            proxy.RotateObject(proxy.GetSelectedObject(), 0, 0, 1, 45 * Math.PI / 180f, true);
        }
    }
}
