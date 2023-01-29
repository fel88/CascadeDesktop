using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            proxy.SetDegenerateModeOff();
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

            if (ofd.FileName.EndsWith(".stp"))
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

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            var p = panel1.PointToClient(Cursor.Position);
            proxy.MoveTo(p.X, p.Y);

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {


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
            proxy.SetSelectionMode((int)TopAbs_ShapeEnum.TopAbs_FACE);
        }

        private void boxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            proxy.MakeBox(0,0,0,100,100,100);
            proxy.MakeBox(50, 50, 50, 150, 150, 150);

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

        private void diffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void unionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeBool();
        }
    }

    public class AutoDialog
    {
        public DialogForm StartDialog()
        {
            DialogForm d = new DialogForm();

            return d;
        }
    }

    public class DialogForm : Form
    {

    }
}
