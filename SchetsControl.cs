using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SchetsEditor
{   public class SchetsControl : UserControl
    {   private Schets schets;
        private Color penkleur;
        private List<Tuple<ISchetsTool, Color, Point, Point, String, List<Point>>> layerlist = new List<Tuple<ISchetsTool, Color, Point, Point, String, List<Point>>>();
        //A list containing information about each "layer", such as the tool and color used as well as beginning- and end coordinates.

        public List<Tuple<ISchetsTool, Color, Point, Point, String, List<Point>>> LayerList
        {
            get { return layerlist; }
            set { layerlist = value; }
        }
        public List<Point> Scribble = new List<Point>();
        public Color PenKleur
        { get { return penkleur; }
          set { penkleur = value; }
        }
        public Schets Schets
        { get { return schets;   }
        }
        public SchetsControl()
        {   this.BorderStyle = BorderStyle.Fixed3D;
            this.schets = new Schets();
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        private void teken(object o, PaintEventArgs pea)
        {
            schets.Teken(pea.Graphics);
        }
        private void veranderAfmeting(object o, EventArgs ea)
        {   schets.VeranderAfmeting(this.ClientSize);
            this.Invalidate();
        }
        public void RefreshList()
        {
            schets.Schoon();
            foreach (Tuple<ISchetsTool, Color, Point, Point, String, List<Point>> Layer in LayerList) 
            {
                PenKleur = Layer.Item2;
                if (Layer.Item1.GetType() == typeof(PenTool))
                {
                    for (int i = 0; i < Layer.Item6.Count - 1; i++)
                    {
                        Layer.Item1.Compleet(this, MaakBitmapGraphics(), Layer.Item6[i], Layer.Item6[i+1]);
                    }
                }
                else
                {
                    Layer.Item1.Compleet(this, MaakBitmapGraphics(), Layer.Item3, Layer.Item4);
                }
            }
        }
        public Graphics MaakBitmapGraphics()
        {   Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
        public void Schoon(object o, EventArgs ea)
        {
            DialogResult dr = MessageBox.Show("Weet je zeker dat je het scherm wilt opschonen?", "Schets editor", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                schets.Schoon();
                LayerList.Clear();
                this.Invalidate();
            }
        }
        public void Roteer(object o, EventArgs ea)
        {
            RefreshList();
            //schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
            //schets.Roteer();
            //this.Invalidate();
        }
        public void VeranderKleur(object obj, EventArgs ea)
        {   string kleurNaam = ((ComboBox)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void VeranderKleurViaMenu(object obj, EventArgs ea)
        {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void Opslaan(string path)
        {
            string[] cutpath = path.Split('/');
            if (cutpath[cutpath.Length - 1] == ".sketch")
            {
                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bf.Serialize(stream, LayerList);
                }
            }
            else
            {
                Schets.Opslaan(path, this.LayerList);
            }
        }
        public void Open(string path)
        {
            string[] cutpath = path.Split('/');
            if (cutpath[cutpath.Length - 1] == ".sketch")
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    LayerList = (List<Tuple<ISchetsTool, Color, Point, Point, string, List<Point>>>)bf.Deserialize(stream);
                    RefreshList();
                    this.Invalidate();
                }
            }
            else
            {
                Schets.Open(path);
            }
        }
    }
}
