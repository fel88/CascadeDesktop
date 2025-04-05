using System;
using System.Windows.Forms;

namespace CSPLib
{
    public static class GUIHelpers
    {
        public static DialogResult ShowQuestion(string text, string caption = null, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            if (caption == null) { /*caption = Form1.Form.Text;*/ }
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
        public static DialogResult Warning(string text, string caption = null, MessageBoxButtons btns = MessageBoxButtons.OK)
        {
            if (caption == null) {/* caption = Form1.Form.Text; */}
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Warning);
        }
        public static object EditorStart(object init, string nm, Type control, bool dialog = true)
        {
            Form f = new Form() { Text = nm };
            f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterScreen;
            var cc = Activator.CreateInstance(control) as UserControl;
            (cc as IPropEditor).Init(init);
            f.Controls.Add(cc);
            f.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            f.AutoSize = true;
            if (dialog)
            {
                f.ShowDialog();
            }
            else
            {
                f.TopMost = true;
                f.Show();
            }
            return (cc as IPropEditor).ReturnValue;
        }
    }
}
