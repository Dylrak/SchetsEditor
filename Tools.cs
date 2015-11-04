using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
        void Compleet(SchetsControl s, Graphics g, Point p1, Point p2);
        bool Collision(SchetsControl s, int i, Point p);
        void CreatePointList();
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        public Brush kwast;
        public int bound = 3;

        public virtual void MuisVast(SchetsControl s, Point p)
        {   startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
        public virtual void CreatePointList() { }
        public virtual void Compleet(SchetsControl s, Graphics g, Point p1, Point p2) 
        {
        }
        public virtual bool Collision(SchetsControl s, int i, Point p) 
        {
            return false;
        }
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c)
        {
            if (c >= 32)
            {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = 
                gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
                gr.DrawString   (tekst, font, kwast, 
                                              this.startpunt, StringFormat.GenericTypographic);
                //gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
                startpunt.X += (int)sz.Width;
                startpunt.Y = (int)sz.Height;
                s.Invalidate();
            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                                , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {   Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p)
        {   base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            this.Compleet(s, s.MaakBitmapGraphics(), this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);
        
        public override void Compleet(SchetsControl s, Graphics g, Point p1, Point p2)
        {
            kwast = new SolidBrush(s.PenKleur);
            this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {   g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override bool Collision(SchetsControl s, int i, Point p)
        {
            Point beginpunt = new Point(Math.Min(s.LayerList[i].Item3.X, s.LayerList[i].Item4.X),
                Math.Min(s.LayerList[i].Item3.Y, s.LayerList[i].Item4.Y));
            Point eindpunt = new Point(Math.Max(s.LayerList[i].Item3.X, s.LayerList[i].Item4.X),
                Math.Max(s.LayerList[i].Item3.Y, s.LayerList[i].Item4.Y));
            return (TweepuntTool.Punten2Rechthoek(beginpunt, eindpunt).Contains(p));
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(SchetsControl s, Graphics g, Point p1, Point p2)
        {
            base.Compleet(s, g, p1, p2);
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class OvaalTool : TweepuntTool
    {
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override bool Collision(SchetsControl s, int i, Point p)
        {
            Point beginpunt = s.LayerList[i].Item3;
            Point eindpunt = s.LayerList[i].Item4;
            double dx, dy, h, k, rx, ry;
            rx = Math.Abs(beginpunt.X - eindpunt.X) / 2;
            ry = Math.Abs(beginpunt.Y - eindpunt.Y) / 2;
            h = Math.Min(beginpunt.X, eindpunt.X) + (Math.Abs(beginpunt.X - eindpunt.X) / 2);
            k = Math.Min(beginpunt.Y, eindpunt.Y) + (Math.Abs(beginpunt.Y - eindpunt.Y) / 2);
            dx = p.X - h;
            dy = p.Y - k;
            return (Formule(dx, rx, dy, ry));
        }
        public virtual bool Formule(double dx,double rx,double dy,double ry)
        {
            return (dx * dx) / ((rx + bound * 2) * (rx + bound * 2)) + (dy * dy) / ((ry + bound * 2) * (ry + bound * 2)) <= 1.0 &&
                (dx * dx) / ((rx - bound * 2) * (rx - bound * 2)) + (dy * dy) / ((ry - bound * 2) * (ry - bound * 2)) >= 1.0;
        }
    }

    public class VolOvaalTool : OvaalTool
    {
        public override string ToString() { return "ovol"; }

        public override void Compleet(SchetsControl s, Graphics g, Point p1, Point p2)
        {
            base.Compleet(s, g, p1, p2);
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override bool Formule(double dx,double rx,double dy,double ry)
        { 
            return (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1.0;
        }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {   g.DrawLine(MaakPen(this.kwast, bound), p1, p2);
        }
        public override bool Collision(SchetsControl s, int i, Point p)
        {
            double dx, dy, x0, y0, x1, y1, x2, y2, k;
            x0 = s.LayerList[i].Item3.X;
            y0 = s.LayerList[i].Item3.Y;
            x1 = s.LayerList[i].Item4.X;
            y1 = s.LayerList[i].Item4.Y;
            x2 = p.X; y2 = p.Y;
            dx = x1 - x0;
            dy = y1 - y0;

            k = ((x2 - x0) * dx + (y2 - y0) * dy) / (dx * dx + dy * dy);
            if (k > 1)
            {
                k = 1;
            }
            else if (k < 0)
            {
                k = 0;
            }
            dx = (x0 + k * dx) - x2;
            dy = (y0 + k * dy) - y2;
            return dx * dx + dy * dy <= bound * bound * 4;
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {   this.MuisLos(s, p);
            this.MuisVast(s, p);
            s.Scribble.Add(p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            base.MuisLos(s, p);
        }
        public override bool Collision(SchetsControl s, int i, Point p)
        {
            foreach (Point line in s.LayerList[i].Item6)
            {
                if ((line.X - p.X) * (line.X - p.X) + (line.Y - p.Y) * (line.Y - p.Y) <= bound * bound * 8) //Cirkelvergelijking
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    public class GumTool : TweepuntTool
    {
        public override string ToString() { return "gum"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            for (int i = s.LayerList.Count - 1; i >= 0; i--)
            {
                if (s.LayerList[i].Item1.Collision(s, i, p))
                {
                    s.LayerList.RemoveAt(i);
                    s.RefreshList();
                    s.Invalidate();
                    break;
                }
            }
        }
        public override void Bezig(Graphics g, Point p1, Point p2)
        {
        }
    }
}
