using CSPLib;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public partial class DraftEditorCSP : Form, CSPLib. IEditor
    {
        public DraftEditorCSP()
        {
            InitializeComponent(); 
            
            Form = this;
            de = new DraftEditorControl();
            de.UndosChanged += De_UndosChanged;
            de.Init(this);
            Controls.Add(de);

            Load += Form1_Load;

            _currentTool = new SelectionTool(this);

            de.Visible = true;

            de.SetDraft(new Draft());
            de.FitAll();

            de.Dock = DockStyle.Fill;
        }

        

        public DraftEditorControl de;
        MessageFilter mf = null;
        public event Action<CSPLib.ITool> ToolChanged;

        
        CSPLib.ITool _currentTool;
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

        
        public void SetTool(CSPLib.ITool tool)
        {
            _currentTool.Deselect();
            _currentTool = tool;
            _currentTool.Select();
            ToolChanged?.Invoke(_currentTool);
        }
        public void CircleStart()
        {
            //SetTool(new DraftEllipseTool(de));
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

        public CSPLib.ITool CurrentTool { get => _currentTool; }

        public IDrawable[] Parts => throw new NotImplementedException();

        public IntersectInfo Pick => throw new NotImplementedException();

        CSPLib.IntersectInfo CSPLib.IEditor.Pick => throw new NotImplementedException();

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
    }
    public class LinearConstraintTool : AbstractDraftTool
    {
        public LinearConstraintTool(IDraftEditor ee) : base(ee)
        {

        }


        public override void Deselect()
        {
            queue.Clear();
        }

        public override void Draw()
        {

        }
        List<DraftElement> queue = new List<DraftElement>();

        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var _draft = Editor.Draft;
                var nearest = Editor.nearest;
                if (nearest is DraftPoint)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftPoint);
                }
                if (nearest is DraftLine dl)
                {
                    if (queue.Count > 0 && queue[0] is DraftPoint dp1)
                    {
                        var lcd = AutoDialog.DialogHelpers.StartDialog();
                        lcd.AddNumericField("len", "Length", dl.Length, 1000000, 0.00001m);
                        lcd.ShowDialog();
                        var len = (decimal)lcd.GetNumericField("len");
                        var cc = new LinearConstraint(dl, dp1, len, _draft);
                        if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                        {
                            Editor.Backup();
                            _draft.AddConstraint(cc);
                            _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                            _draft.Childs.Add(_draft.Helpers.Last());
                        }
                        else
                        {
                            GUIHelpers.Warning("such constraint already exist");
                        }
                        queue.Clear();
                        //editor.ResetTool();
                    }
                    else
                    {

                        if (_draft.Constraints.OfType<EqualsConstraint>().Any(uu => uu.TargetLine == dl))
                        {
                            GUIHelpers.Warning("overconstrained");
                        }
                        else
                        {
                            var lcd = AutoDialog.DialogHelpers.StartDialog();
                            lcd.AddNumericField("len", "Length", dl.Length, 1000000, 0.00001m);
                            lcd.ShowDialog();
                            var len = (decimal)lcd.GetNumericField("len");
                            var cc = new LinearConstraint(dl.V0, dl.V1, len, _draft);
                            if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                            {
                                Editor.Backup();

                                _draft.AddConstraint(cc);
                                _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                                _draft.Childs.Add(_draft.Helpers.Last());
                            }
                            else
                            {
                                GUIHelpers.Warning("such constraint already exist");
                            }
                            queue.Clear();
                            //editor.ResetTool();
                        }
                    }
                    return;

                }
                if (queue.Count > 1)
                {
                    var lcd = AutoDialog.DialogHelpers.StartDialog();
                    lcd.AddNumericField("len", "Length", ((queue[0] as DraftPoint).Location - (queue[1] as DraftPoint).Location).Length, 1000000, 0.00001m);
                    lcd.ShowDialog();
                    var len = (decimal)lcd.GetNumericField("len");

                    var cc = new LinearConstraint(queue[0], queue[1], len, _draft);
                    if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                    {
                        Editor.Backup();

                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist");
                    }
                    queue.Clear();
                    //editor.ResetTool();
                }
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {

        }

        public override void Select()
        {
            queue.Clear();
        }

        public override void Update()
        {

        }
    }
}
