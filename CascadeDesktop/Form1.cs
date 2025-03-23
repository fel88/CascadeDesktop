using Cascade.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoDialog;
using CascadeDesktop.Tools;
using System.Windows;
using OpenTK.Platform.Windows;
using Microsoft.Win32.SafeHandles;
using System.IO.Compression;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;
using static CascadeDesktop.OccSceneObject;
using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;

namespace CascadeDesktop
{
    public partial class Form1 : Form, IEditor
    {
        public Form1()
        {
            InitializeComponent();
            Form = this;
            Load += Form1_Load;
            Shown += Form1_Shown;
            SizeChanged += Form1_SizeChanged;
            Paint += Form1_Paint;
            panel1.Paint += Panel1_Paint;
            panel1.MouseWheel += Panel1_MouseWheel;
            panel1.MouseDown += Panel1_MouseDown;
            panel1.MouseUp += Panel1_MouseUp;

            toolStripStatusLabel3.Alignment = ToolStripItemAlignment.Right;
            toolStripStatusLabel3.Click += ToolStripStatusLabel3_Click;
            toolStripStatusLabel3.MouseEnter += ToolStripStatusLabel3_MouseEnter;
            toolStripStatusLabel3.MouseLeave += ToolStripStatusLabel3_MouseLeave;

            _currentTool = new SelectionTool(this);
        }

        private void ToolStripStatusLabel3_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Default;
        }

        private void ToolStripStatusLabel3_MouseEnter(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Hand;
        }

        private void ToolStripStatusLabel3_Click(object sender, EventArgs e)
        {
            shortStatusOutputFormat = !shortStatusOutputFormat;
            UpdateStatus(lastSelected);
        }

        RibbonMenu menu;
        public static Form1 Form;

        private void Form1_Load(object sender, EventArgs e)
        {
            menu = new RibbonMenu();
            Controls.Add(menu);
            menu.AutoSize = true;
            menu.Dock = DockStyle.Top;

            mf = new MessageFilter();
            System.Windows.Forms.Application.AddMessageFilter(mf);

            toolStrip1.Visible = false;
        }

        MessageFilter mf = null;

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            if (e.Button == MouseButtons.Left)
            {
                proxy.Select(ModifierKeys.HasFlag(Keys.Control));
                SelectionChanged();
                _currentTool.MouseUp(e);
            }
        }

        System.Drawing.Point startDrag;
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

        ManagedObjHandle lastSelected = null;

        private void UpdateStatus(ManagedObjHandle obj)
        {
            var v = proxy.GetVertexPosition(obj);
            var face = proxy.GetFaceInfo(obj);
            var edge = proxy.GetEdgeInfoPosition(obj);

            if (edge != null)
            {
                var vect = edge.Start.ToVector3d();
                var vect2 = edge.End.ToVector3d();
                SetStatus3(string.Empty);
                AppendStatusVector("edge", vect);
                AppendStatusVector(" ", vect2);
                if (edge is CircleEdgeInfo cei)
                {
                    AppendDouble("radius", cei.Radius);
                }
                AppendStatusVector("com", edge.COM.ToVector3d());
                AppendStatus3($" len: {edge.Length}");
            }
            else if (v != null)
            {
                var vect = v.ToVector3d();
                SetStatus3($"vertex: {vect.X} {vect.Y} {vect.Z}");
            }
            else if (face != null)
            {
                var vect = face.Position.ToVector3d();
                if (face is PlaneSurfInfo p)
                {
                    var nrm = p.Normal.ToVector3d();
                    ClearStatus3();
                    AppendStatusVector("plane", vect);
                    AppendStatusVector("normal", nrm);
                    AppendStatusVector("com", p.COM.ToVector3d());
                }
                else if (face is CylinderSurfInfo c)
                {
                    ClearStatus3();
                    AppendStatusVector("cylinder", vect);
                    AppendStatusVector("COM", c.COM.ToVector3d());
                    AppendDouble("radius", c.Radius);

                }
                else if (face is SphereSurfInfo s)
                {
                    ClearStatus3();
                    AppendStatusVector("sphere", vect);
                    AppendStatusVector("COM", s.COM.ToVector3d());
                    AppendDouble("radius", s.Radius);

                }
                else
                {
                    ClearStatus3();
                    AppendStatusVector(face.GetType().Name, vect);
                    AppendStatusVector("COM", face.COM.ToVector3d());
                }
            }
            else
            {
                SetStatus3(string.Empty);
            }
        }

        private void ClearStatus3()
        {
            SetStatus3(string.Empty);
        }

        public void SelectionChanged()
        {
            if (!proxy.IsObjectSelected())
            {
                SetStatus3(string.Empty);
                return;
            }
            var occ = GetSelectedOccObject();

            SetStatus2(occ == null ? string.Empty : occ.Name);

            lastSelected = proxy.GetSelectedObject();
            UpdateStatus(lastSelected);
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            var p = e.Location;
            proxy.StartZoomAtPoint(p.X, p.Y);
            double delta = (double)(e.Delta) / (15 * 8);
            int x = p.X;
            int y = p.Y;
            int x1 = (int)(p.X + panel1.Width * delta / 100);
            int y1 = (int)(p.Y + panel1.Height * delta / 100);
            proxy.ZoomAtPoint(x, y, x1, y1);
            //proxy.Zoom(0, 0, e.Delta / 8, 0);
        }

        public void ResetTool()
        {
            //uncheckedAllToolButtons();
            SetTool(new SelectionTool(this));
            //toolStripButton9.Checked = true;
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
            proxy = new OCCTProxyWrapper();
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

            Color clr1 = Color.DarkBlue;
            Color clr2 = Color.Olive;
            //proxy.SetBackgroundColor(clr1.R, clr1.G, clr1.B, clr2.R, clr2.G, clr2.B);


            proxy.UpdateCurrentViewer();
            proxy.UpdateView();
            Width = Width + 1;
        }

        void SetBgGradient(Color clr1, Color clr2)
        {
            proxy.SetBackgroundColor(clr1.R, clr1.G, clr1.B, clr2.R, clr2.G, clr2.B);
        }

        public IOCCTProxyInterface Proxy => proxy;
        public List<OccSceneObject> Objs => Scene.Objs;

        IOCCTProxyInterface proxy;
        public void ZoomAll()
        {
            proxy.ZoomAllView();
            proxy.UpdateView();
            proxy.UpdateCurrentViewer();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ZoomAll();
        }

        public void ImportModel()
        {
            //curForm.ImportModel(ModelFormat.STEP);
            // return;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (ofd.FileName.ToLower().EndsWith(".stp") || ofd.FileName.ToLower().EndsWith(".step"))
            {
                var bts = File.ReadAllBytes(ofd.FileName).ToList();
                Objs.AddRange(proxy.ImportStep(ofd.FileName, bts).Select(z => new ImportedOccSceneObject(ofd.FileName, z, proxy) { Name = Path.GetFileNameWithoutExtension(ofd.FileName) }));
            }
            if (ofd.FileName.ToLower().EndsWith(".igs") || ofd.FileName.ToLower().EndsWith(".iges"))
            {
                Objs.AddRange(proxy.ImportIges(ofd.FileName).Select(z => new OccSceneObject(z, proxy)));
            }
            proxy.SetDisplayMode(1);
            proxy.RedrawView();
            proxy.UpdateView();
            proxy.UpdateCurrentViewer();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ImportModel();
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
        public void SetStatus(string text, InfoType type = InfoType.Info)
        {
            toolStripStatusLabel1.Text = text;
            toolStripStatusLabel1.ForeColor = Color.Black;
            toolStripStatusLabel1.BackColor = System.Drawing.SystemColors.Control;
            switch (type)
            {
                case InfoType.Warning:
                    toolStripStatusLabel1.ForeColor = Color.Black;
                    toolStripStatusLabel1.BackColor = Color.Yellow;
                    break;
                case InfoType.Error:
                    toolStripStatusLabel1.ForeColor = Color.White;
                    toolStripStatusLabel1.BackColor = Color.Red;
                    break;
            }
        }

        public void SetStatus3(string text)
        {
            toolStripStatusLabel3.Text = text;
        }

        public void SetStatus2(string text)
        {
            toolStripStatusLabel2.Text = text;
        }

        bool shortStatusOutputFormat = false;
        public void AppendStatus3(string text)
        {
            toolStripStatusLabel3.Text += text;
        }

        public void AppendStatusVector(string caption, Vector3d v)
        {
            if (shortStatusOutputFormat)
                AppendStatus3($"{caption}: {v.X} {v.Y} {v.Z} ");
            else
                AppendStatus3($"{caption}: {v.X:0.##} {v.Y:0.##} {v.Z:0.##} ");
        }

        public void AppendDouble(string caption, double v)
        {
            if (shortStatusOutputFormat)
                AppendStatus3($"{caption}: {v} ");
            else
                AppendStatus3($"{caption}: {v:0.##} ");
        }

        private void boxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBox();
        }

        public void AddBox()
        {
            var d = DialogHelpers.StartDialog();
            d.Text = "New box";
            d.AddNumericField("w", "Width", 50);
            d.AddNumericField("l", "Length", 50);
            d.AddNumericField("h", "Height", 50);

            if (!d.ShowDialog())
                return;

            var w = d.GetNumericField("w");
            var h = d.GetNumericField("h");
            var l = d.GetNumericField("l");
            var cs = proxy.MakeBox(0, 0, 0, w, l, h);
            Objs.Add(new OccSceneObject(cs, proxy));
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
                var delta = new System.Drawing.Point(e.Location.X - startDrag.X, startDrag.Y - e.Location.Y);
                proxy.Pan(delta.X, delta.Y);
                startDrag = e.Location;
            }
            proxy.MoveTo(e.Location.X, e.Location.Y);

        }

        public void Clear()
        {
            foreach (var item in Objs)
            {
                item.Remove();
            }
            Objs.Clear();

            Proxy.UpdateCurrentViewer();
        }

        public void DeleteSelected()
        {
            var occ = GetSelectedOccObject();
            DeleteUI(occ);
        }

        public void DeleteUI(OccSceneObject occ)
        {
            if (occ == null)
                return;

            if (StaticHelpers.ShowQuestion($"Are you sure to delete: {occ.Name}?", Text))
            {
                Remove(occ);
                Proxy.UpdateCurrentViewer();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DeleteSelected();
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
            FaceSelectionMode();
        }

        public void FaceSelectionMode()
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Face);

        }
        public void WireSelectionMode()
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Wire);

        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.LeftView();
        }

        public void ExportSelectedToStep()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Step models (*.stp)|*.stp";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            proxy.ExportStep(proxy.GetSelectedObject(), sfd.FileName);
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            ExportSelectedToStep();
        }

        public void ResetView()
        {
            proxy.AxoView();
            ZoomAll();
        }

        private void axoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetView();
        }

        ManagedObjHandle obj1;
        ManagedObjHandle obj2;
        private void diffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeDiff(obj1, obj2);
            proxy.Erase(obj1);
            proxy.Erase(obj2);
            proxy.UpdateCurrentViewer();
            proxy.UpdateView();
            proxy.RedrawView();
        }

        public void Fuse()
        {
            //todo: refactor to separate tool
            proxy.MakeFuse(obj1, obj2);
            proxy.Erase(obj1, false);
            proxy.Erase(obj2);
        }

        private void unionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Fuse();
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
            if (keyData == (Keys.Control | Keys.E))
            {
                EdgeSelectionMode();
                SetStatus("edge selection mode");
            }
            if (keyData == (Keys.Control | Keys.F))
            {
                FaceSelectionMode();
                SetStatus("face selection mode");
            }
            if (keyData == (Keys.Control | Keys.V))
            {
                VertexSelectionMode();
                SetStatus("vertex selection mode");
            }
            if (keyData == Keys.Delete)
            {
                DeleteSelected();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void vertexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VertexSelectionMode();
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
            MoveSelected();
        }

        public void MoveSelected()
        {
            if (!proxy.IsObjectSelected())
            {
                SetStatus("Object not selected", InfoType.Warning);
                return;
            }

            var d = DialogHelpers.StartDialog();
            d.AddOptionsField("mode", "Mode", new string[] { "Relative", "Abs", "Abs COM of selected face" }, 0);
            d.AddNumericField("x", "x", 0, 10000, -10000);
            d.AddNumericField("y", "y", 0, 10000, -10000);
            d.AddNumericField("z", "z", 0, 10000, -10000);

            if (!d.ShowDialog())
                return;

            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");
            var z = d.GetNumericField("z");

            switch (d.GetOptionsFieldIdx("mode"))
            {
                case 0:
                    proxy.MoveObject(proxy.GetSelectedObject(), x, y, z, true);
                    break;
                case 1:                    
                    proxy.MoveObject(proxy.GetSelectedObject(), x, y, z, false);
                    break;
                case 2:
                    var com = proxy.GetFaceInfo(proxy.GetSelectedObject()).COM;
                    var shift = new Vector3d(x, y, z) - com.ToVector3d();
                    proxy.MoveObject(proxy.GetSelectedObject(), shift.X, shift.Y, shift.Z, true);
                    break;
            }
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            proxy.MakeCommon(obj1, obj2);
        }

        public void AddCylinder()
        {
            var d = DialogHelpers.StartDialog();
            d.Text = "New cylinder";
            d.AddNumericField("r", "Radius", 15);
            d.AddNumericField("h", "Height", 150);

            if (!d.ShowDialog())
                return;

            var r = d.GetNumericField("r");
            var h = d.GetNumericField("h");

            var cs = proxy.MakeCylinder(r, h);
            Objs.Add(new OccSceneObject(cs, proxy));
        }

        private void cylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCylinder();
        }

        public void AddSphere()
        {
            var d = DialogHelpers.StartDialog();
            d.Text = "New sphere";
            d.AddNumericField("r", "Radius", 50);

            if (!d.ShowDialog())
                return;

            var r = d.GetNumericField("r");
            var cs = proxy.MakeSphere(r);
            Objs.Add(new OccSceneObject(cs, proxy));
        }

        private void sphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddSphere();
        }

        public void RotateSelected()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.AddNumericField("a", "Angle", 90, 720, -720);
            d.AddNumericField("x", "x", 0);
            d.AddNumericField("y", "y", 0);
            d.AddNumericField("z", "z", 1);

            if (!d.ShowDialog())
                return;

            var ang = d.GetNumericField("a");
            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");
            var z = d.GetNumericField("z");

            proxy.RotateObject(proxy.GetSelectedObject(), x, y, z, ang * Math.PI / 180f, true);
        }

        public void MirrorSelected()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.Text = "Mirror object";

            d.AddNumericField("x", "Pivot X", 0);
            d.AddNumericField("y", "Pivot Y", 0);
            d.AddNumericField("z", "Pivot Z", 0);

            d.AddNumericField("dx", "Dir X", 0);
            d.AddNumericField("dy", "Dir Y", 0);
            d.AddNumericField("dz", "Dir Z", 1);

            if (!d.ShowDialog())
                return;


            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");
            var z = d.GetNumericField("z");

            var dx = d.GetNumericField("dx");
            var dy = d.GetNumericField("dy");
            var dz = d.GetNumericField("dz");

            var h = proxy.MirrorObject(proxy.GetSelectedObject(), new Vector3(dx, dy, dz), new Vector3(x, y, z), true, true);
            Objs.Add(new OccSceneObject(h, proxy) { Name = "mirrored" });
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {

        }

        bool grid = true;
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            GridSwitch();
        }

        public void GridSwitch()
        {
            grid = !grid;
            proxy.ActivateGrid(grid);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            foreach (var item in Objs)
            {
                item.Remove();
            }
            Objs.Clear();
        }

        private void edgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EdgeSelectionMode();
        }

        public void EdgeSelectionMode()
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Edge);
        }

        public void PipeAlongWire(OccSceneObject occ, double r)
        {
            var cs = proxy.MakePipe(occ.Handle, r);
            proxy.UpdateCurrentViewer();
            Objs.Add(new OccSceneObject(cs, proxy));
            Remove(occ);
        }

        public void PipeWithSplitting(OccSceneObject occ, double r)
        {
            var edges = proxy.GetEdgesInfo(occ.Handle);
            ManagedObjHandle current = null;
            //order and convert to polyline
            for (int i = 0; i < edges.Count; i++)
            {
                var start = edges[i].Start;
                var end = edges[i].End;
                if (i > 0)
                {
                    var prev = edges[i - 1];
                    var sphere = proxy.Sphere(prev.End.X, prev.End.Y, prev.End.Z, r);
                    var temp = current;
                    current = proxy.MakeFuse(current, sphere);
                    proxy.Erase(sphere, false);
                    proxy.Erase(temp, false);
                }

                var cs = proxy.Pipe(start.X, start.Y, start.Z, end.X, end.Y, end.Z, r);
                if (current == null)
                {
                    current = cs;
                }
                else
                {
                    var temp1 = current;
                    current = proxy.MakeFuse(current, cs);
                    proxy.Erase(temp1, false);
                    proxy.Erase(cs, false);
                }
            }

            proxy.UpdateCurrentViewer();
            Objs.Add(new OccSceneObject(current, proxy));
            Remove(occ);
        }

        public void Pipe()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.Text = "Pipe";
            d.AddNumericField("r", "Radius", 1);
            d.AddOptionsField("mode", "Mode", new[] { "Along wire", "Split" }, 0);

            if (!d.ShowDialog())
                return;

            var modeIdx = d.GetOptionsFieldIdx("mode");

            var r = d.GetNumericField("r");
            var so = proxy.GetSelectedObject();
            var occ = GetSelectedOccObject();

            if (occ == null)
                return;

            if (modeIdx == 1)
                PipeWithSplitting(occ, r);
            else
                PipeAlongWire(occ, r);
        }

        public void Fillet()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.Text = "Fillet";
            d.AddNumericField("r", "Radius", 15);

            if (!d.ShowDialog())
                return;

            var r = d.GetNumericField("r");
            var so = proxy.GetSelectedObject();
            var occ = GetSelectedOccObject();
            if (occ == null)
                return;

            var faces = proxy.GetFacesInfo(so);
            ManagedObjHandle cs = null;

            if (faces.Count == 0)//2d fillet             
                cs = proxy.MakeFillet2d(so, r);
            else
                cs = proxy.MakeFillet(so, r);

            Objs.Add(new OccSceneObject(cs, proxy));
            Remove(occ);

            proxy.UpdateCurrentViewer();
        }

        public void Clone()
        {
            if (!CheckObjectSelectedUI())
                return;

            var occ = GetSelectedOccObject();
            if (occ == null)
                return;

            var so = proxy.GetSelectedObject();
            var cs = proxy.Clone(so);
            Objs.Add(new OccSceneObject(cs, proxy) { Name = $"{occ.Name}_cloned" });
            SetStatus("cloned");
        }

        private void filletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Fillet();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Edge);
        }


        public void ExportSelectedToObj()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Obj mesh|*.obj";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var res = proxy.IteratePoly(proxy.GetSelectedObject()).Select(z => new Vector3d(z.X, z.Y, z.Z)).ToArray();

            const float tolerance = 1e-8f;
            StringBuilder sb = new StringBuilder();
            List<Vector3d> vvv = new List<Vector3d>();


            for (int i = 0; i < res.Length; i += 3)
            {
                var verts = new[] { res[i], res[i + 1], res[i + 2] };
                foreach (var v in verts)
                {
                    if (vvv.Any(z => (z - v).Length < tolerance))
                        continue;

                    vvv.Add(v);
                    sb.AppendLine($"v {v.X} {v.Y} {v.Z}".Replace(",", "."));
                }
            }
            int counter = 1;
            for (int i = 0; i < res.Length; i += 3)
            {
                var verts = new[] { res[i], res[i + 1], res[i + 2] };
                List<int> indc = new List<int>();

                foreach (var vitem in verts)
                {
                    for (int k = 0; k < vvv.Count; k++)
                    {
                        if ((vvv[k] - vitem).Length < tolerance)
                        {
                            indc.Add(k + 1);
                        }
                        else
                            continue;
                    }
                }
                if (indc.GroupBy(z => z).Any(z => z.Count() > 1))
                {
                    continue;
                    //throw duplicate face vertex
                }
                sb.AppendLine($"f {indc[0]} {indc[1]} {indc[2]}");
            }

            File.WriteAllText(sfd.FileName, sb.ToString());
            SetStatus($"saved to {sfd.FileName} successfully");
        }

        private void exportMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedToObj();
        }

        public void Extrude()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.Text = "Extrude";
            d.AddNumericField("h", "Height", 50);

            if (!d.ShowDialog())
                return;

            var h = d.GetNumericField("h");
            var handler = proxy.MakePrism(proxy.GetSelectedObject(), h);
            Objs.Add(new OccSceneObject(handler, proxy));
        }

        private void extrudeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Extrude();
        }

        private void draftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            obj1 = proxy.AddWireDraft(40);
        }

        public void DrawDraft()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddOptionsField("mode", "Mode", new[] { "2D", "3D" }, 0);

            if (!d.ShowDialog())
                return;

            var modeIdx = d.GetOptionsFieldIdx("mode");

            if (modeIdx == 0)
            {
                DraftEditor dd = new DraftEditor();
                dd.StartPosition = FormStartPosition.CenterScreen;
                dd.ShowDialog();
                if (dd.Blueprint == null || !dd.Blueprint.Contours.Any())
                    return;

                var handler = proxy.ImportBlueprint(dd.Blueprint);
                Objs.Add(new OccSceneObject(handler, proxy));
            }
            else //3d
            {
                DraftEditor3d dd = new DraftEditor3d();
                dd.StartPosition = FormStartPosition.CenterScreen;
                dd.ShowDialog();
                if (dd.Blueprint == null || !dd.Blueprint.Contours.Any())
                    return;

                var handler = proxy.ImportBlueprint(dd.Blueprint);
                Objs.Add(new OccSceneObject(handler, proxy));
            }
        }

        private void draftToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DrawDraft();
        }

        void ImportDxf(string file)
        {
            var elems = DxfParser.LoadDxf(file);

            var elems2 = elems.Where(z => z.Length > DxfParser.RemoveThreshold).ToList();
            //convert to blueprintItems first, then order
            var nfps = DxfParser.ElementsToContours(elems2.ToArray());
            //var nfps2 = DxfParser.ConnectElements(elems2.ToArray());
            //  if (nfps.Any(z => z.Points.Count < 3))
            //  throw new Exception("few points");

            for (int i = 0; i < nfps.Length; i++)
            {
                for (int j = 0; j < nfps.Length; j++)
                {
                    if (i == j)
                        continue;

                    var d2 = nfps[i];
                    var d3 = nfps[j];
                    var f0 = d3.GetPoints()[0];
                    if (StaticHelpers.pnpoly(d2.GetPoints().ToArray(), f0.X, f0.Y))
                    {
                        d3.Parent = d2;
                        if (!d2.Childrens.Contains(d3))
                        {
                            d2.Childrens.Add(d3);
                        }
                    }

                }
            }
            Blueprint blueprint = new Blueprint();

            //List<HelperItem> ret = new List<HelperItem>();
            int sign = 0;
            foreach (var item in nfps)
            {
                if (item.Parent != null)
                    continue;

                sign = Math.Sign(StaticHelpers.signed_area(item.GetPoints().ToArray()));
                blueprint.Contours.Add(item.ToBlueprintContour());
            }
            //holes
            foreach (var item in nfps)
            {
                if (item.Parent == null)
                    continue;

                if (Math.Sign(StaticHelpers.signed_area(item.GetPoints().ToArray())) == sign)
                {
                    item.Reverse();
                }
                blueprint.Contours.Add(item.ToBlueprintContour());

                /*BlueprintPolyline poly = new BlueprintPolyline();
                BlueprintContour cntr = new BlueprintContour();
                cntr.Items.Add(poly);
                foreach (var pp in item.Points)
                {
                    poly.Points.Add(new Vertex2D(pp.X, pp.Y));
                }
                if (Math.Sign(StaticHelpers.signed_area(item.Points.ToArray())) == sign)
                {
                    poly.Points.Reverse();
                }

                blueprint.Contours.Add(cntr);*/
            }
            var handler = proxy.ImportBlueprint(blueprint);

        }

        public void ImportDraft()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dxf files (*.dxf)|*.dxf";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            ImportDxf(ofd.FileName);
        }

        private void importDraftFromDxfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDraft();
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetBgGradient(Color.Black, Color.Black);
        }

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //SetBgGradient(Color.LightBlue, Color.Gray);
            proxy.SetDefaultGradient();
        }

        public void Chamfer()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 15);

            if (!d.ShowDialog())
                return;

            var r = d.GetNumericField("r");
            var so = proxy.GetSelectedObject();
            var occ = GetSelectedOccObject();
            if (occ == null)
                return;

            var cs = proxy.MakeChamfer(so, r);
            Objs.Add(new OccSceneObject(cs, proxy));
            Remove(occ);
            proxy.UpdateCurrentViewer();
        }

        private void chamferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Chamfer();
        }

        private void revolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.ShowDialog();
            //proxy.MakeRevolution();
        }

        private bool CheckObjectSelectedUI()
        {
            if (!proxy.IsObjectSelected())
            {
                SetStatus("Object not selected", InfoType.Warning);
                return false;
            }
            return true;
        }

        public void AddCone()
        {
            var d = DialogHelpers.StartDialog();
            d.Text = "New cone";
            d.AddNumericField("r1", "Radius 1", 50);
            d.AddNumericField("r2", "Radius 2", 25);
            d.AddNumericField("h", "Height", 25);

            if (!d.ShowDialog())
                return;

            var r1 = d.GetNumericField("r1");
            var r2 = d.GetNumericField("r2");
            var h = d.GetNumericField("h");

            var cs = proxy.MakeCone(r1, r2, h);
            Objs.Add(new OccSceneObject(cs, proxy));
        }

        private void coneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCone();
        }

        private void facesInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FacesInfo();
        }

        ITool _currentTool;

        public event Action<ITool> ToolChanged;

        public void SetTool(ITool tool)
        {
            _currentTool.Deselect();
            _currentTool = tool;
            _currentTool.Select();
            ToolChanged?.Invoke(_currentTool);
        }
        private void adjointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdjoinTool(false);
        }

        public void AdjoinTool(bool withDistance)
        {
            SetTool(new AdjoinTool(this, withDistance));

        }
        public void AdjoinCOMsTool()
        {
            SetTool(new AdjoinCOMsTool(this));

        }
        public void FuseTool()
        {
            SetTool(new BoolTool(this, BoolTool.FuseOperation.Fuse));
        }
        public void DiffTool()
        {
            SetTool(new BoolTool(this, BoolTool.FuseOperation.Diff));
        }
        public void CommonTool()
        {
            SetTool(new BoolTool(this, BoolTool.FuseOperation.Intersect));
        }
        internal void VertexSelectionMode()
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Vertex);

        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        internal void FrontView()
        {
            proxy.FrontView();
            ZoomAll();
        }
        internal void TopView()
        {
            proxy.TopView();
            ZoomAll();
        }
        internal void RightView()
        {
            proxy.RightView();
            ZoomAll();
        }
        internal void BottomView()
        {
            proxy.BottomView();
            ZoomAll();
        }
        internal void BackView()
        {
            proxy.BackView();
            ZoomAll();
        }
        internal void LeftView()
        {
            proxy.LeftView();
            ZoomAll();
        }

        internal void ShapeSelectionMode()
        {
            proxy.ResetSelectionMode();
            proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Shape);
        }

        internal void SetDarkBackground()
        {
            SetBgGradient(Color.Black, Color.Black);
        }

        internal void SetLightBackground()
        {
            proxy.SetDefaultGradient();
        }

        internal void FacesInfo()
        {
            if (!CheckObjectSelectedUI())
                return;

            var infos = proxy.GetFacesInfo(proxy.GetSelectedObject());
            Form ff = new Form();
            RichTextBox r = new RichTextBox();
            r.Dock = DockStyle.Fill;
            ff.Controls.Add(r);
            foreach (var info in infos)
            {
                if (info is PlaneSurfInfo p)
                    r.AppendText($"PLANE {p.Position.X} {p.Position.Y} {p.Position.Z}   normal: {p.Normal.X} {p.Normal.Y} {p.Normal.Z} {Environment.NewLine}");
                else if (info is CylinderSurfInfo c)
                    r.AppendText($"CYLINDER {c.Position.X} {c.Position.Y} {c.Position.Z}   axis: {c.Axis.X} {c.Axis.Y} {c.Axis.Z}   radius: {c.Radius} {Environment.NewLine}");
                else if (info is SphereSurfInfo s)
                    r.AppendText($"SPHERE {s.Position.X} {s.Position.Y} {s.Position.Z}   radius: {s.Radius} {Environment.NewLine}");
                else
                    r.AppendText($"{info.GetType().Name}{info.Position.X} {info.Position.Y} {info.Position.Z} {Environment.NewLine}");
            }
            ff.Show();
        }

        internal void RulerTool()
        {
            SetTool(new RulerTool(this));
        }

        internal void SelectionTool()
        {
            SetTool(new SelectionTool(this));
        }

        internal void ExtrudeFace()
        {
            if (!CheckObjectSelectedUI())
                return;

            var d = DialogHelpers.StartDialog();
            d.Text = "Extrude";
            d.AddNumericField("h", "Height", 50);

            if (!d.ShowDialog())
                return;

            var h = d.GetNumericField("h");
            var mh = proxy.MakePrismFromFace(proxy.GetSelectedObject(), h);
            var occ = new OccSceneObject(mh, proxy) { Name = "extrude" };
            Objs.Add(occ);
        }

        public OccScene Scene = new OccScene();

        public OccSceneObject GetSelectedOccObject()
        {
            var h = proxy.GetSelectedObject();
            var fr = Objs.FirstOrDefault(z => z.IsEquals(h));
            return fr;
        }
        public OccSceneObject GetDetectedOccObject()
        {
            var h = proxy.GetDetectedObject();
            var fr = Objs.FirstOrDefault(z => z.IsEquals(h));
            return fr;
        }
        internal void Transparency()
        {
            if (!CheckObjectSelectedUI())
                return;

            var fr = GetSelectedOccObject();
            if (fr == null)
                return;


            fr.SwitchTransparency();
            proxy.UpdateCurrentViewer();
        }

        public void RenameSelected()
        {
            var occ = GetSelectedOccObject();
            RenameUI(occ);
        }

        public void RenameUI(OccSceneObject occ, Form owner = null)
        {
            if (occ == null)
                return;

            var d = DialogHelpers.StartDialog();
            d.AddStringField("name", "Name", occ.Name);
            if (owner != null)
            {
                if (d.ShowDialog(owner) != DialogResult.OK)
                    return;
            }
            else
            if (!d.ShowDialog())
                return;

            occ.Name = d.GetStringField("name");
        }

        internal void SaveAsProject()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Projects file (*.zip)|*.zip";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            //save zip here
            IOZipContext szc = new IOZipContext();

            using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Update, true))
                {
                    //save project xml
                    szc.Zip = archive;

                    foreach (var item in Objs)
                    {
                        item.StoreToZip(szc);
                    }
                    fileStream.Flush();
                }
            }
        }

        OccSceneObject[] LoadModelFromZipStream(IOZipContext ctx, ZipArchiveEntry entry)
        {
            MemoryStream ms = new MemoryStream();
            using (var str = entry.Open())
            {
                str.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            var bts = ms.ToArray();
            var hh = proxy.ImportStep(entry.Name, bts.ToList());
            List<OccSceneObject> ret = new List<OccSceneObject>();
            foreach (var hitem in hh)
            {
                ret.Add(new OccSceneObject(hitem, proxy) { Name = $"{entry.Name}_{ctx.ModelIdx++}" });
            }
            return ret.ToArray();
        }

        internal void OpenProject()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Projects file (*.zip)|*.zip";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            IOZipContext ctx = new IOZipContext();
            //read zip and restore all models            

            Clear();
            using (ZipArchive zip = ZipFile.Open(ofd.FileName, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    var name = entry.Name.ToLower();
                    if (name.EndsWith(".xml") && name.StartsWith("info"))
                    {
                        XDocument doc = null;
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            doc = XDocument.Parse(reader.ReadToEnd());
                        }
                        var xx = doc.Root.Element("model");
                        var path = xx.Attribute("path").Value;
                        var nm = xx.Attribute("name").Value;
                        var tr = xx.Attribute("transparency").Value;
                        var matrix = xx.Attribute("matrix").Value;
                        var ee = zip.Entries.FirstOrDefault(z => z.Name == path);
                        if (ee.Name.ToLower().EndsWith(".model"))
                        {
                            var rr = LoadModelFromZipStream(ctx, ee);
                            var clr = xx.Attribute("color").Value;
                            var cc = clr.Split(new char[] { ';' }).Select(int.Parse).ToArray();
                            foreach (var ccc in rr)
                            {
                                ccc.Name = nm;
                                ccc.SetTransparency((TransparencyLevel)Enum.Parse(typeof(TransparencyLevel), tr));
                                ccc.SetColor(Color.FromArgb(cc[0], cc[1], cc[2]));
                                ccc.SetMatrix(matrix.Split(';').Select(StaticHelpers.ParseDouble).ToArray());
                            }
                            Objs.AddRange(rr);
                        }
                    }
                }
            ResetView();
            ZoomAll();
            proxy.UpdateCurrentViewer();
        }

        internal void NewProject()
        {

        }

        public void Remove(OccSceneObject item)
        {
            Objs.Remove(item);
            item.Remove();
        }

        internal void SetColor()
        {
            var occ = GetSelectedOccObject();
            if (occ == null)
                return;

            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
                occ.SetColor(cd.Color);
        }

        internal void ShowObjectsList()
        {
            ObjectsList ol = new ObjectsList();
            ol.Init(this);
            ol.TopMost = true;
            ol.Show();
        }

        internal void Wireframe()
        {

            var occ = GetSelectedOccObject();
            if (occ == null)
                return;

            occ.SwitchWireframe();
        }

        internal void CreateText()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddStringField("text", "Text");
            d.AddNumericField("fontSize", "Font size", 20, 200, 0.1m, 1);
            d.AddNumericField("height", "Height", 5, 2000, 0.01m, 2);

            if (!d.ShowDialog())
                return;

            var text = d.GetStringField("text");
            if (string.IsNullOrEmpty(text))
            {
                StaticHelpers.ShowError("Empty string", "Error");
                return;
            }

            var fontSize = d.GetNumericField("fontSize");
            var height = d.GetNumericField("height");

            var cs = proxy.Text2Brep(text, fontSize, height);
            Objs.Add(new OccSceneObject(cs, proxy));
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    var gp = proxy.GetGravityPoint();
        //    toolStripStatusLabel4.Text = $"{gp.X} {gp.Y} {gp.Z}";
        //    gp = proxy.GetEye();
        //    toolStripStatusLabel5.Text = $" eye {gp.X} {gp.Y} {gp.Z}";
        //    gp = proxy.GetCenter();
        //    toolStripStatusLabel6.Text = $" center {gp.X} {gp.Y} {gp.Z}";
        //    gp = proxy.GetUp();
        //    toolStripStatusLabel7.Text = $" up {gp.X} {gp.Y} {gp.Z}";

        //    return;
        //    var h = proxy.GetDetectedObject();
        //    if (Objs.Any())
        //    {
        //        var ff = proxy.GetFacesInfo(Objs.First().Handle);

        //        var f1 = ff.Where(z => z.Handle == h.Handle).ToArray();
        //        var f2 = ff.Where(z => z.THandle == h.HandleT).ToArray();


        //    }
        //    var hs = GetDetectedOccObject();

        //    //if (hs != null)
        //    {
        //        try
        //        {
        //            var face = proxy.GetFaceInfo(h);
        //            if (face != null)
        //                toolStripStatusLabel4.Text = face.COM.X.ToString();
        //        }
        //        catch (Exception ex) { }
        //    }
        //    /*OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);
        //    GL.Vertex3(0, 0, 0);
        //    GL.Vertex3(110, 110, 0);
        //    GL.Vertex3(0, 110, 0);
        //    GL.End();*/
        //}
    }
}
