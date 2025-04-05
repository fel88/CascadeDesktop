using CSPLib;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Cascade.Common;
using System.Linq;
using CascadeDesktop.ToolsCSP;
using CSPLib.Interfaces;

namespace CascadeDesktop
{
    public partial class DraftEditorCSP : Form, IEditor
    {
        public DraftEditorCSP()
        {
            InitializeComponent();

            Form = this;
            de = new DraftEditorControl();
            de.UndosChanged += De_UndosChanged;
            de.Init(this);
            panel1.Controls.Add(de);

            Load += Form1_Load;

            de.Visible = true;
            de.SetTool(new SelectionTool(this));
            de.SetDraft(new Draft());
            de.FitAll();

            de.Dock = DockStyle.Fill;
        }



        public DraftEditorControl de;
        MessageFilter mf = null;
        public event Action<ITool> ToolChanged;


        public static DraftEditorCSP Form;

        private void Form1_Load(object sender, EventArgs e)
        {
            /*menu = new RibbonMenu();
            Controls.Add(menu);
            menu.AutoSize = true;
            menu.Dock = DockStyle.Top;

            toolStripContainer1.TopToolStripPanel.Visible = false;*/

            mf = new MessageFilter();
            Application.AddMessageFilter(mf);

            //tableLayoutPanel1.ColumnStyles[2].Width = 0;
        }
        public void RectangleStart()
        {
            SetTool(new RectDraftTool(de));
            // uncheckedAllToolButtons();
            //toolStripButton3.Checked = true;
        }


        private void De_UndosChanged()
        {
            //toolStripButton16.Enabled = de.CanUndo;
        }
        internal void SetStatus(string v)
        {
            //toolStripStatusLabel1.Text = v;
        }

        public void SetTool(ITool tool)
        {
            CurrentTool.Deselect();
            de.SetTool(tool);
            CurrentTool.Select();
            ToolChanged?.Invoke(CurrentTool);
        }

        public void CircleStart()
        {
            SetTool(new DraftEllipseTool(de));
            //uncheckedAllToolButtons();
            //  toolStripButton4.Checked = true;
        }

        public void ObjectSelect(object nearest)
        {
            //throw new NotImplementedException();
        }

        public void ResetTool()
        {
            SetTool(new SelectionTool(this));
        }

        public void Backup()
        {
            //throw new NotImplementedException();
        }
        public List<string> Undos = new List<string>();

        public void Undo()
        {
            //if (EditMode == EditModeEnum.Draft)
            {
                de.Undo();
                return;
            }
            /*
            if (Undos.Count == 0) return;
            var el = XElement.Parse(Undos.Last());
            //Scene.Restore(el);
            Undos.RemoveAt(Undos.Count - 1);
            //UndosChanged?.Invoke();*/
        }

        public ITool CurrentTool { get => de.CurrentTool; }

        public IDrawable[] Parts => throw new NotImplementedException();

        public IntersectInfo Pick => throw new NotImplementedException();

        CSPLib.IntersectInfo IEditor.Pick => throw new NotImplementedException();

        private void erctangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RectangleStart();
        }
        void exportDxf(Draft draft)
        {
            //export to dxf
            IxMilia.Dxf.DxfFile file = new IxMilia.Dxf.DxfFile();
            foreach (var item in draft.DraftLines)
            {
                if (item.Dummy)
                    continue;

                file.Entities.Add(new DxfLine(new DxfPoint(item.V0.X, item.V0.Y, 0), new DxfPoint(item.V1.X, item.V1.Y, 0)));
            }
            foreach (var item in draft.DraftEllipses)
            {
                if (item.Dummy)
                    continue;
                if (!item.SpecificAngles)
                {
                    //file.Entities.Add(new DxfEllipse(new DxfPoint(item.Center.X, item.Center.Y, 0), new DxfVector((double)item.Radius, 0, 0), 360));
                    file.Entities.Add(new DxfCircle(new DxfPoint(item.Center.X, item.Center.Y, 0), (double)item.Radius));
                    //file.Entities.Add(new DxfArc(new DxfPoint(item.Center.X, item.Center.Y, 0), (double)item.Radius, 0, 360));
                }
                else
                {
                    var pp = item.GetPoints();

                    //file.Entities.Add(new DxfPolyline(pp.Select(zz => new DxfVertex(new DxfPoint(zz.X, zz.Y, 0)))));
                    for (int i = 1; i <= pp.Length; i++)
                    {
                        var p0 = pp[i - 1];
                        var p1 = pp[i % pp.Length];
                        //polyline?

                        file.Entities.Add(new DxfLine(new DxfPoint(p0.X, p0.Y, 0), new DxfPoint(p1.X, p1.Y, 0)));
                    }
                }
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "DXF files (*.dxf)|*.dxf";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var ed = AutoDialog.DialogHelpers.StartDialog();
            ed.AddBoolField("mm", "mm units");
            ed.ShowDialog();
            var mmUnit = ed.GetBoolField("mm");

            if (mmUnit)
            {
                file.Header.DefaultDrawingUnits = DxfUnits.Millimeters;
                file.Header.Version = DxfAcadVersion.R2013; // default version does not support units
                file.Header.DrawingUnits = DxfDrawingUnits.Metric;

                file.Header.UnitFormat = DxfUnitFormat.Decimal;
                file.Header.UnitPrecision = 3;
                file.Header.DimensionUnitFormat = DxfUnitFormat.Decimal;
                file.Header.DimensionUnitToleranceDecimalPlaces = 3;
                file.Header.AlternateDimensioningScaleFactor = 0.0394;
            }

            file.Save(sfd.FileName);
            SetStatus($"{sfd.FileName} saved.");
        }

        /*public void ExportDraftToDxf()
        {
            exportDxf(editedDraft);
        }*/
        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CircleStart();
        }

        private void randomSolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            de.Draft.RandomSolve();
        }

        private void linearSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetTool(new LinearConstraintTool(de));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            exportDxf(de.Draft);
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RectangleStart();
        }

        private void circleToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CircleStart();

        }

        private void randolmSolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            de.Draft.RandomSolve();
        }

        private void linearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetTool(new LinearConstraintTool(de));
        }
        BlueprintContour[] ConnectContour(BlueprintItem[] items)
        {
            List<BlueprintContour> rets = new List<BlueprintContour>();
            List<BlueprintItem> remains = new List<BlueprintItem>();

            BlueprintContour ret = new BlueprintContour();
            rets.Add(ret);
            ret.Items.Add(items[0]);
            remains.AddRange(items.Skip(1));
            float eps = 1e-5f;
            while (remains.Any())
            {
                var p2 = ret.Items.Last().End;
                var p1 = ret.Items.First().Start;
                BlueprintItem todel = null;
                foreach (var item in remains)
                {

                    var dist1 = (item.Start.ToVector2d() - p2.ToVector2d()).Length;
                    if (dist1 < eps)
                    {
                        ret.Items.Add(item);
                        todel = item;
                        break;
                    }
                    var dist2 = (item.End.ToVector2d() - p2.ToVector2d()).Length;
                    if (dist2 < eps)
                    {
                        item.Reverse();
                        ret.Items.Add(item);

                        todel = item;
                        break;
                    }
                }

                if (todel == null)
                {
                    //new contour
                    ret = new BlueprintContour();
                    rets.Add(ret);
                    todel = remains[0];
                    ret.Items.Add(remains[0]);
                }

                remains.Remove(todel);
            }

            return rets.ToArray();
        }
        public Blueprint Blueprint => blueprint;

        Blueprint blueprint = new Blueprint();
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            foreach (var item in de.Draft.Elements)
            {
                if (item is DraftLine dl)
                {
                    var blueprintItem = new Line2D();
                    blueprintItem.Start = new Vertex2D(dl.V0.X, dl.V0.Y);
                    blueprintItem.End = new Vertex2D(dl.V1.X, dl.V1.Y);
                    blueprint.Items.Add(blueprintItem);

                }
            }
            var contours = ConnectContour(blueprint.Items.ToArray());

            blueprint.Items.Clear();
            blueprint.Contours.AddRange(contours);
            Close();

        }

        public void CutEdgeStart()
        {
            SetTool(new CutEdgeTool(de));
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            CutEdgeStart();
        }
        public void LineStart()
        {
            SetTool(new DraftLineTool(de));
            //uncheckedAllToolButtons();
            toolStripButton2.Checked = true;
        }
        private void polylineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LineStart();

        }
        public void selectorUI()
        {
            SetTool(new SelectionTool(this));
            // uncheckedAllToolButtons();
            //toolStripButton18.Checked = true;
        }
        private void sekectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectorUI();
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form.SetTool(new VerticalConstraintTool(Form.de));

        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form.SetTool(new HorizontalConstraintTool(Form.de));

        }

        private void equalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form.SetTool(new EqualsConstraintTool(Form.de));
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            selectorUI();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            de.Undo();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            de.ShowHelpers = !de.ShowHelpers;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            de.FitAll();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            de.Clear();
        }

        private void closeLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(de.CurrentTool is DraftLineTool dlt) || !dlt.AddedPoints.Any())
                return;

            var f = dlt.AddedPoints.First();
            var l = dlt.AddedPoints.Last();
            de.Draft.Elements.Add(new DraftLine(l, f, de.Draft));
            SetTool(new SelectionTool(this));
        }

        private void paramettricToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddOptionsField("type", "Type", new[] { "Rectangle", "Circle" }, 0);

            if (!d.ShowDialog())
                return;

            var idx = d.GetOptionsFieldIdx("type");
            switch (idx)
            {
                case 0:
                    {
                        d = AutoDialog.DialogHelpers.StartDialog();
                        d.AddNumericField("x", "X", 0, min: -10000);
                        d.AddNumericField("y", "Y", 0, min: -10000);
                        d.AddNumericField("w", "Width", 0);
                        d.AddNumericField("h", "Height", 0);
                        if (!d.ShowDialog())
                            return;

                        //add rectangle here
                        var xx = (float)d.GetNumericField("x");
                        var yy = (float)d.GetNumericField("y");
                        var ww = (float)d.GetNumericField("w");
                        var hh = (float)d.GetNumericField("h");

                        de.Backup();
                        RectDraftTool.AddRectangletToDraft(de.Draft, new System.Drawing.PointF(xx, yy), new System.Drawing.PointF(xx + ww, yy + hh));

                        break;
                    }
                case 1:
                    {
                        d = AutoDialog.DialogHelpers.StartDialog();
                        d.AddNumericField("x", "X", 0, min: -10000);
                        d.AddNumericField("y", "Y", 0, min: -10000);
                        d.AddNumericField("r", "Radius", 0);

                        if (!d.ShowDialog())
                            return;

                        //add rectangle here
                        var xx = (float)d.GetNumericField("x");
                        var yy = (float)d.GetNumericField("y");
                        var r = (float)d.GetNumericField("r");


                        de.Backup();


                        break;
                    }
            }
        }

        private void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            de.SolveCSP();
        }

        public void PointAnchor()
        {
            if (de.selected != null && de.selected.Count() == 1)
            {
                if (de.selected[0] is DraftPoint dp)
                {
                    var ppc = new PointPositionConstraint(dp, de.Draft);
                    de.Draft.AddConstraint(ppc);
                }
            }
            else
                MessageBox.Show("point not selected");
        }

        private void pointAnchorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PointAnchor();
        }
    }
}
