using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DPGenerator.Model
{
    class CommonCreator
    {
        private Bitmap top;
        private Bitmap down;
        private Bitmap outputBitmap;
        private CommonDescriptor descriptor;

        private int width;
        private int height;

        public Bitmap CommonBitmap { get { return outputBitmap; } }
        public CommonDescriptor Descriptor { get { return descriptor; } }


        public CommonCreator(string fileTop, string fileDown)
        {
            top = new Bitmap(fileTop);
            down = new Bitmap(fileDown);
        }

        public void Generate(int r, int g, int b)
        {
            int w = top.Width;
            int h = top.Height;

            if (down.Width < w) w = down.Width;
            if (down.Height < h) h = down.Height;

            Color comonColor = Color.FromArgb(0, 255, 0);
            Color upColor = Color.FromArgb(255, 0, 0);
            Color downColor = Color.FromArgb(0, 0, 255);

            outputBitmap = new Bitmap(w, h);
            width = w;
            height = h;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color tpoint = top.GetPixel(x, y);
                    Color dpoint = down.GetPixel(x, y);

                    // wybiera wspulne punkty
                    if (tpoint.R == dpoint.R && tpoint.G == dpoint.G && tpoint.B == dpoint.B
                        && tpoint.R == r && tpoint.G == g && tpoint.B == b)
                    {
                        //commPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = r, G = g, B = b });
                        outputBitmap.SetPixel(x, y, comonColor);
                    }
                    else
                        // wybiera punkty ktore sa na gorze a nie ma na dole
                        if (tpoint.R == r && tpoint.G == g && tpoint.B == b)
                        {
                            //topPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = 255, G = 0, B = 0 });
                            outputBitmap.SetPixel(x, y, upColor);
                        }
                        else
                            // wybiera punktu ktore sa na dole a nie ma na gorze
                            if (dpoint.R == r && dpoint.G == g && dpoint.B == b)
                            {
                                //topPoints.Add(new Point3D() { X = x, Y = y, Z = 0, R = 0, G = 0, B = 255 });
                                outputBitmap.SetPixel(x, y, downColor);
                            }



                }
            }

            top.Dispose();
            down.Dispose();

        }


        public void GenerateCommonDescriptor()
        {
            List<Color> greenTones = new List<Color>();
            int green = 255;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixelColor = outputBitmap.GetPixel(x, y);

                    if (pixelColor.R == 0 && pixelColor.G == 255 && pixelColor.B == 0)
                    {
                        green--;
                        Color newColor = Color.FromArgb(0, green, 0);
                        greenTones.Add(newColor);

                        PaintRegion(x, y, newColor);
                    }

                }
            }

            descriptor = new CommonDescriptor();

            descriptor.UpColor = Color.FromArgb(255, 0, 0);
            descriptor.DownColor = Color.FromArgb(0, 0, 255);
            descriptor.CommonColor = greenTones;

        }

        private void PaintRegion(int start_x, int start_y, Color color)
        {
            Color oldColor = outputBitmap.GetPixel(start_x, start_y);

            bool[,] mask = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mask[x, y] = false;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(start_x, start_y));
            mask[start_x, start_y] = true;

            Point current;
            Color currentColor;

            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                currentColor = outputBitmap.GetPixel(current.X, current.Y);

                if(currentColor.R == oldColor.R && 
                    currentColor.G == oldColor.G &&
                    currentColor.B == oldColor.B)
                {
                    outputBitmap.SetPixel(current.X, current.Y, color);

                    int left = current.X - 1;
                    int right = current.X + 1;
                    int top = current.Y - 1;
                    int down = current.Y + 1;

                    // dla lewego sasiada
                    if (left >= 0)
                    {
                        if (!mask[left, current.Y])
                        {
                            queue.Enqueue(new Point(left, current.Y));
                            mask[left, current.Y] = true;
                        }
                    }

                    // dla prawego
                    if (right < width)
                    {
                        if (!mask[right, current.Y])
                        {
                            queue.Enqueue(new Point(right, current.Y));
                            mask[right, current.Y] = true;
                        }
                    }

                    // dla gornego
                    if (top >= 0)
                    {
                        if (!mask[current.X, top])
                        {
                            queue.Enqueue(new Point(current.X, top));
                            mask[current.X, top] = true;
                        }
                    }

                    // dla dolnego
                    if (down < height)
                    {
                        if (!mask[current.X, down])
                        {
                            queue.Enqueue(new Point(current.X, down));
                            mask[current.X, down] = true;
                        }
                    }
                }
                
            }

        }




    }
}
