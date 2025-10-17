using CascadeDesktop.Interfaces;
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

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var occ = (listView1.SelectedItems[i].Tag as OccSceneObject);
                occ.SwitchVisible();
            }
            UpdateList();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                RemoveSelected();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void RemoveSelected()
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            if (!StaticHelpers.ShowQuestion($"Are you sure to delete: {listView1.SelectedItems.Count} objects?", Text))
                return;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var occ = (listView1.SelectedItems[i].Tag as OccSceneObject);
                Editor.Remove(occ);
            }

            Editor.Proxy.UpdateCurrentViewer();

            UpdateList();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelected();
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
