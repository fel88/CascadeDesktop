using System.Windows.Forms;

namespace CascadeDesktop
{
    public class DialogHelpers
    {
        public static bool ShowQuestion(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static DialogForm StartDialog()
        {
            DialogForm d = new DialogForm()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            TableLayoutPanel tp = new TableLayoutPanel();
            tp.Dock = DockStyle.Fill;
            d.Controls.Add(tp);
            Button ok = new Button() { Text = "apply" };
            tp.Controls.Add(ok, 0, tp.RowCount - 1);
            ok.Click += (s, e) =>
            {
                d.DialogResult = DialogResult.OK;
                d.Close();
            };
            return d;
        }
    }
}
