using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public class DialogForm : Form
    {
        public DialogForm()
        {
            Shown += DialogForm_Shown;         
        }

        public new bool ShowDialog()
        {
            return base.ShowDialog() == DialogResult.OK;
        }
        private void DialogForm_Shown(object sender, System.EventArgs e)
        {
            var tp = Controls[0] as TableLayoutPanel;
            Height = (tp.RowCount + 1) * 30 + 40;
        }

        public void AddNumericField(string key, string caption, double? _default = null)
        {
            Label text = new Label() { Text = caption };
            var tp = Controls[0] as TableLayoutPanel;
            
            NumericUpDown m = new NumericUpDown();
            m.Maximum = 5000;
            m.Minimum = -5000;
            m.DecimalPlaces = 2;
            if (_default != null)
                m.Value = (decimal)_default.Value;

            tp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tp.RowCount++;
            tp.Controls.Add(text, 0, tp.RowCount - 1);
            tp.Controls.Add(m, 1, tp.RowCount - 1);

            prms.Add(key, m);

        }
        public void AddBoolField(string key, string caption, bool? _default = null)
        {
            Label text = new Label() { Text = caption };
            var tp = Controls[0] as TableLayoutPanel;

            CheckBox m = new CheckBox();
            
            if (_default != null)
                m.Checked = (bool)_default.Value;

            tp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tp.RowCount++;
            tp.Controls.Add(text, 0, tp.RowCount - 1);
            tp.Controls.Add(m, 1, tp.RowCount - 1);

            prms.Add(key, m);

        }
        internal double GetNumericField(string v)
        {
            return (double)((prms[v] as NumericUpDown).Value);
        }
        internal bool GetBoolField(string v)
        {
            return (bool)((prms[v] as CheckBox).Checked);
        }

        Dictionary<string, Control> prms = new Dictionary<string, Control>();
    }
}
