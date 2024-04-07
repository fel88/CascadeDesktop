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
    public partial class ObjectsList : Form
    {
        public ObjectsList()
        {
            InitializeComponent();
        }
        IEditor Editor;
        public void Init(IEditor editor)
        {
            Editor = editor;
            UpdateList();
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in Editor.Objs)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.Visible.ToString() }) { Tag = item });
            }
        }

        private void switchVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var occ = (listView1.SelectedItems[0].Tag as OccSceneObject);
            occ.SwitchVisible();
            UpdateList();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var occ = (listView1.SelectedItems[0].Tag as OccSceneObject);
            Editor.DeleteUI(occ);
            UpdateList();
        }

        private void setNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var occ = (listView1.SelectedItems[0].Tag as OccSceneObject);
            Editor.RenameUI(occ, this);
            UpdateList();
        }
    }
}
