using System;
using System.Drawing;
using System.Windows.Forms;

namespace SchetsEditor
{
    public class Hoofdscherm : Form
    {
        MenuStrip menuStrip;
        ToolStripDropDownItem menu;
        SchetsWin s;

        public Hoofdscherm()
        {   this.ClientSize = new Size(800, 700);
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakHelpMenu();
            this.Text = "Schets editor";
            this.IsMdiContainer = true;
            this.MainMenuStrip = menuStrip;
            menuStrip.Items[0].Click += EnableSave;
        }
        private void maakFileMenu()
        {
            menu = new ToolStripMenuItem("File");
            menu.DropDownItems.Add("Nieuw", null, this.nieuw);
            menu.DropDownItems.Add("Opslaan", null, this.opslaan);
            menu.DropDownItems[1].Enabled = false;
            menu.DropDownItems.Add("Open", null, this.open);
            menu.DropDownItems.Add("Exit", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void EnableSave(object o, EventArgs ea)
        {
            if (s != null)
            {
                menu.DropDownItems[1].Enabled = true;
            }
            else 
            {
                menu.DropDownItems[1].Enabled = false;
            }

        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Schets versie 1.0\n(c) UU Informatica 2010"
                           , "Over \"Schets\""
                           , MessageBoxButtons.OK
                           , MessageBoxIcon.Information
                           );
        }

        private void nieuw(object sender, EventArgs e)
        {   s = new SchetsWin();
            s.MdiParent = this;
            s.Show();
        }
        private void afsluiten(object sender, EventArgs e)
        {   this.Close();
        }
        private void opslaan(object sender, EventArgs e)
        {
            if (s != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "JPEG-image|*.jpg|PNG-image|*.png|BMP-image|*.bmp";
                save.FileName = "*.jpg";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    s.Opslaan(save.FileName);
                }
            }
        }
        private void open(object sender, EventArgs e)
        {
            s = new SchetsWin();
            s.MdiParent = this;
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "JPEG-image|*.jpg|PNG-image|*.png|BMP-image|*.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                s.Open(open.FileName);
            }
            s.Show();
        }
    }
}
