using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DPGenerator.Model;

namespace DPGenerator
{
    class CommonPart
    {
        private Bitmap top;
        private Bitmap down;

        private List<Point3D> commPoints = new List<Point3D>();
        private List<Point3D> topPoints = new List<Point3D>();      // punkty ktore sa na gorze a nie ma na dole
        private List<Point3D> downPoints = new List<Point3D>();     // punkty ktore sa na dole a nie ma na gorze
        private Bitmap outputBitmap;

        public Bitmap CommonBitmap { get { return outputBitmap; } }


        public CommonPart(string fileTop, string fileDown)
        {
            top = new Bitmap(fileTop);
            down = new Bitmap(fileDown);
        }

        public void Generate(int r, int g, int b)
        {
            commPoints.Clear();
            topPoints.Clear();
            downPoints.Clear();


            int w = top.Width;
            int h = top.Height;

            if (down.Width < w) w = down.Width;
            if (down.Height < h) h = down.Height;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    System.Drawing.Color tpoint = top.GetPixel(x, y);
                    System.Drawing.Color dpoint = down.GetPixel(x, y);

                    // wybiera wspulne punkty
                    if (tpoint.R == dpoint.R && tpoint.G == dpoint.G && tpoint.B == dpoint.B
                        && tpoint.R == r && tpoint.G == g && tpoint.B == b)
                    {
                        commPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = r, G = g, B = b });
                    }
                    else
                        // wybiera punkty ktore sa na gorze a nie ma na dole
                        if (tpoint.R == r && tpoint.G == g && tpoint.B == b)
                        {
                            topPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = 255, G = 0, B = 0 });
                        }
                        else
                            // wybiera punktu ktore sa na dole a nie ma na gorze
                            if (dpoint.R == r && dpoint.G == g && dpoint.B == b)
                            {
                                topPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = 0, G = 0, B = 255 });
                            }

                }
            }

            outputBitmap = new Bitmap(w, h);
            foreach (Point3D p in commPoints)
            {
                outputBitmap.SetPixel(p.X, p.Y, System.Drawing.Color.FromArgb(p.R, p.G, p.B));
            }

            foreach (Point3D p in topPoints)
            {
                outputBitmap.SetPixel(p.X, p.Y, System.Drawing.Color.FromArgb(p.R, p.G, p.B));
            }

            foreach (Point3D p in downPoints)
            {
                outputBitmap.SetPixel(p.X, p.Y, System.Drawing.Color.FromArgb(p.R, p.G, p.B));
            }

        }

        public void SeveBitmap(string fileName)
        {
            outputBitmap.Save(fileName);
        }
    }
}
