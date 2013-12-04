using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DPGenerator.Model
{
    class LayerConnector
    {
        private Bitmap commonBitmap;
        private CommonDescriptor descriptor;

        private int width;
        private int height;

        private LayerPoint[,] middleLayer;
        private LayerPoint[,] upLayer;
        //private LayerPoint[,] downLayer;

        public enum LayerType
        {
            Up,
            Middle,
            Down
        }


        public LayerConnector(Bitmap commonBitmap, CommonDescriptor descriptor)
        {
            this.commonBitmap = commonBitmap;
            this.descriptor = descriptor;

            width = commonBitmap.Width;
            height = commonBitmap.Height;
        }

        public void CreateLayer()
        {
            upLayer = new LayerPoint[width, height];

            Color color;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    LayerPoint.LayerPointType pointType = GetPointTypeForLayer(x, y, LayerType.Up);

                    if (pointType == LayerPoint.LayerPointType.Undefined) continue;

                    //LayerPoint point = new LayerPoint() { Type = pointType };
                    LayerPoint point = new LayerPoint(x, y, pointType);
                    upLayer[x, y] = point;
                }
            }
        }

        public void Run(LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            if (layerType == LayerType.Down)
                layer = null;

            if (layer == null) return;

            // automat

            bool mustRun = true;

            while (mustRun)
            {
                mustRun = false;
                LayerPoint[,] nextLayer = new LayerPoint[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (layer[x, y] == null) continue;

                        // wszystkie punkty oprocz punktow rozszerzajacych przepisujemy
                        if (layer[x, y].Type != LayerPoint.LayerPointType.ExpansiblePoint)
                        {
                            nextLayer[x, y] = layer[x, y];
                            continue;
                        }


                         // od tego miejca nastepuja juz zmiany w puktach lub istnieja punkty rozszerznia
                        mustRun = true;
                        Point2D[] neighborCords = Helper.GetMoorNeighbors(x, y, width, height);
                        
                        // sasiadow wybieramy tylko zposrod punktow konturu rozszerzajacego
                        // lub konturu rozszerzenia
                        List<LayerPoint> neighbors = new List<LayerPoint>();
                        foreach (Point2D p in neighborCords)
                        {
                            if (layer[p.X, p.Y] == null) continue;
                                
                            if(layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpandedContour ||
                                layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                            {
                                neighbors.Add(layer[p.X, p.Y]);
                            }
                        }

                        // przepisujemy punkt
                        if (neighbors.Count < 1)
                        {
                            nextLayer[x, y] = layer[x, y];
                            continue;
                        }

                        Random r = new Random();
                        LayerPoint bestPoint = neighbors[r.Next(neighbors.Count)];

                        // wybrany sasiad jest konturem rozszerzenia wiec mozemy go przepisac
                        if(bestPoint.Type == LayerPoint.LayerPointType.ExpandedContour)
                        {
                            nextLayer[x, y] = bestPoint;
                        }
                        else
                        {
                            // wybrany sasiad jest konturem rozszerzajacym wiec musimy
                            // przerobic go na kontur rozszerzenia

                            LayerPoint point = new LayerPoint();
                            point.Type = LayerPoint.LayerPointType.ExpandedContour;
                            point.ConnectionX = bestPoint.ConnectionX;
                            point.ConnectionY = bestPoint.ConnectionY;

                            nextLayer[x, y] = point;
                        }
                        
                        
                    }

                } // koniec for'ow

                if (mustRun)
                {
                    layer = null;
                    layer = nextLayer;
                }

            } // koniec while()

            if(layerType == LayerType.Up)
                upLayer = layer;
            //if(layerType == LayerType.Down)
                

        }

        public void ClearLayer(LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            //if(layerType == LayerType.Down)

            if (layer == null) return;

            List<Point2D> toDel = new List<Point2D>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (layer[x, y] == null) continue;
                    if (layer[x, y].Type == LayerPoint.LayerPointType.CommonContour) continue;
                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                    {
                        toDel.Add(new Point2D(x, y));
                        continue;
                    }
                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpandedContour)
                    {
                        bool del = true;
                        Point2D[] neighCoords = Helper.GetMoorNeighbors(x, y, width, height);

                        foreach (Point2D p in neighCoords)
                        {
                            if (layer[p.X, p.Y] == null)
                            {
                                del = false;
                                break;
                            }

                        }

                        if (del)
                        {
                            toDel.Add(new Point2D(x, y));
                        }
                    }

                }
            }


            foreach (Point2D p in toDel)
            {
                layer[p.X, p.Y] = null;
            }

            if (layerType == LayerType.Up)
                upLayer = layer;

        }



        /// <summary>
        /// Sprawdza typ punktu dla danej warstwy
        /// </summary>
        private LayerPoint.LayerPointType GetPointTypeForLayer(int x, int y, LayerType layerType)
        {
            LayerPoint.LayerPointType type = LayerPoint.LayerPointType.Undefined;

            Color c = commonBitmap.GetPixel(x, y);

            // dla punktow z czesci wspolnej
            if (descriptor.EqualCommon(c))
            {
                // kontur czesci wspolnej
                if (IsContour(x, y, c))
                {
                    // jesli punkt jest konturem czesci wspolnej
                    // i sasiaduje z punktami analizowanej warstwy
                    if (IsNeighborWithLayer(x, y, layerType))
                    {
                        return LayerPoint.LayerPointType.ExpansibleContour;
                    }

                    return LayerPoint.LayerPointType.CommonContour;
                }
            }

            // dla puntow z gory
            if (descriptor.EqualUp(c))
            {
                if (layerType == LayerType.Up)
                    return LayerPoint.LayerPointType.ExpansiblePoint;
                else
                    return type;
            }

            // dla punktow z dolu
            if (descriptor.EqualUp(c))
            {
                if (layerType == LayerType.Down)
                    return LayerPoint.LayerPointType.ExpansiblePoint;
                else 
                    return type;
            }

            return type;
        }

        /// <summary>
        /// Sprawdza czy punkt o podanych wspolrzednych i kolorze jest konturem
        /// czy sasiaduje z punktami o innym kolorze
        /// </summary>
        private bool IsContour(int x, int y, Color color)
        {

            if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                return true;

            Point2D[] neighbor = Helper.GetMoorNeighbors(x, y, width, height);
            Color neighColor;

            // jesli ktorykolwiek z sasiadow ma inny kolor niz podany punkt
            // to punkt nalezy do konturu
            foreach (Point2D p in neighbor)
            {
                neighColor = commonBitmap.GetPixel(p.X, p.Y);

                if (neighColor.R != color.R ||
                    neighColor.G != color.G ||
                    neighColor.B != color.B)

                    return true;
            }

            return false;

        }

        /// <summary>
        /// Sprawdza czy punkt sasiaduje z podana warstwa
        /// </summary>
        private bool IsNeighborWithLayer(int x, int y, LayerType layerType)
        {
            Color layerColor;

            if (layerType == LayerType.Up)
            {
                layerColor = descriptor.UpColor;
            }
            else
                if (layerType == LayerType.Down)
                {
                    layerColor = descriptor.DownColor;
                }
                else
                    layerColor = descriptor.CommonColor;


            Point2D[] neighbor = Helper.GetMoorNeighbors(x, y, width, height);
            Color neighColor;

            // jesli ktorykolwiek z sasiadow ma kolor podanej warstwy 
            // to punkt jest sasiadem z podana warstwa
            foreach (Point2D p in neighbor)
            {
                neighColor = commonBitmap.GetPixel(p.X, p.Y);

                if (neighColor.R == layerColor.R &&
                    neighColor.G == layerColor.G &&
                    neighColor.B == layerColor.B)

                    return true;
            }

            return false;
        }


        public void SaveLayer(LayerType layerType, string fileName)
        {
            Bitmap bm = new Bitmap(width, height);

            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;
            if (layerType == LayerType.Down)
                layer = null;


            if (layer == null) return;

            for(int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (layer[x, y] == null) continue;

                    if (layer[x, y].Type == LayerPoint.LayerPointType.CommonContour)
                        bm.SetPixel(x, y, Color.FromArgb(0, 150, 0));

                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                        bm.SetPixel(x, y, Color.FromArgb(0, 255, 0));

                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpandedContour)
                        bm.SetPixel(x, y, Color.FromArgb(0, 0, 0));

                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpansiblePoint)
                        if(layerType == LayerType.Up)
                            bm.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        else
                            bm.SetPixel(x, y, Color.FromArgb(0, 0, 255));
                }

            bm.Save(fileName);
            
        }


    }
}
