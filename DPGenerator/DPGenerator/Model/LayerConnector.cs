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

        public int width;
        public int height;

        private LayerPoint[,] middleLayer;
        public LayerPoint[,] upLayer;
        public LayerPoint[,] downLayer;

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
            downLayer = new LayerPoint[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    // dla gory
                    LayerPoint.LayerPointType pointType = GetPointTypeForLayer(x, y, LayerType.Up);

                    if (pointType != LayerPoint.LayerPointType.Undefined)
                    {
                        LayerPoint point = new LayerPoint(x, y, pointType);

                        // id ziarna ustawiamy tylko dla kontorow rozszerzalnych
                        if (pointType == LayerPoint.LayerPointType.ExpansibleContour)
                        {
                            Color pointColor = commonBitmap.GetPixel(x, y);
                            point.SeedID = descriptor.GetSeedID(pointColor);
                        }

                        upLayer[x, y] = point;
                    }

                    // dla dolu
                    LayerPoint.LayerPointType pointTypeDown = GetPointTypeForLayer(x, y, LayerType.Down);
                    if (pointTypeDown != LayerPoint.LayerPointType.Undefined)
                    {
                        LayerPoint point = new LayerPoint(x, y, pointTypeDown);

                        if (pointTypeDown == LayerPoint.LayerPointType.ExpansibleContour)
                        {
                            Color pointColor = commonBitmap.GetPixel(x, y);
                            point.SeedID = descriptor.GetSeedID(pointColor);
                        }

                        downLayer[x, y] = point;
                    }
                }
            }
        }

        public void Run(LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            if (layerType == LayerType.Down)
                layer = downLayer;

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

                            if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpandedContour ||
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
                        if (bestPoint.Type == LayerPoint.LayerPointType.ExpandedContour)
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
                            point.SeedID = bestPoint.SeedID;

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

            if (layerType == LayerType.Up)
                upLayer = layer;

            if (layerType == LayerType.Down)
                downLayer = layer;


        }

        public void ClearLayer(LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            if (layerType == LayerType.Down)
                layer = downLayer;

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
                        // aby nie wyrzucac punktow ktore moga powodowac dziur
                        bool del = true;
                        Point2D[] neighCoords = Helper.GetMoorClockNeighbors(x, y, width, height);

                        int changeCounter = 0;

                        for (int i = 0; i < neighCoords.Length - 1; i++)
                        {
                            if (layer[neighCoords[i].X, neighCoords[i].Y] == null && layer[neighCoords[i + 1].X, neighCoords[i + 1].Y] != null ||
                                layer[neighCoords[i].X, neighCoords[i].Y] != null && layer[neighCoords[i + 1].X, neighCoords[i + 1].Y] == null)
                            {
                                changeCounter++;
                            }
                        }

                        if (layer[neighCoords[neighCoords.Length - 1].X, neighCoords[neighCoords.Length - 1].Y] == null && layer[neighCoords[0].X, neighCoords[0].Y] != null ||
                                layer[neighCoords[neighCoords.Length - 1].X, neighCoords[neighCoords.Length - 1].Y] != null && layer[neighCoords[0].X, neighCoords[0].Y] == null)
                        {
                            changeCounter++;
                        }

                        if (changeCounter > 2)
                            del = false;

                        if (del)
                            toDel.Add(new Point2D(x, y));

                        continue;
                    }

                    // dla pikseli z czesci rozszerzonej
                    // ktore po automacie maja typ rozszerzonego konturu
                    if (layer[x, y].Type == LayerPoint.LayerPointType.ExpandedContour)
                    {
                        bool del = true;
                        Point2D[] neighCoords = Helper.GetMoorNeighbors(x, y, width, height);

                        foreach (Point2D p in neighCoords)
                        {
                            // jesli ktorykolwiek z sasiadow jest nullem, to punkt jest konturem
                            // i nie usuwamy go
                            if (layer[p.X, p.Y] == null)
                            {
                                del = false;
                                break;
                            }

                            // jesli ktorykolwiek z sasiadow pochodzi od innego ziarna (plamy)
                            // to nie usuwamy punktu, aby muc wykryc laczenie konturow

                            if (layer[x, y].SeedID != layer[p.X, p.Y].SeedID)
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

            if (layerType == LayerType.Down)
                downLayer = layer;

        }

        /// <summary>
        /// Nie dziala! wywal sie na zuzyciu pamieci
        /// 
        /// TRZEBA ZAKOMENTOWAC USUWANIE KONTURU ROZSZERZAJACEGO W CLEARLAYER
        /// 
        /// </summary>
        public void CompleteCommonConnection(LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            if (layer == null) return;

            bool[,] wasProccessed = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    wasProccessed[x, y] = false;

                    if (layer[x, y] != null)
                    {
                        if (layer[x, y].Type == LayerPoint.LayerPointType.ExpandedContour)
                        {
                            int pr_x = layer[x, y].ConnectionX.Value;
                            int pr_y = layer[x, y].ConnectionY.Value;

                            wasProccessed[pr_x, pr_y] = true;
                        }
                    }
                }



            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (layer[x, y] == null) continue;
                    if (layer[x, y].Type != LayerPoint.LayerPointType.ExpandedContour) continue;

                    if (wasProccessed[x, y]) continue;


                    // tworzenie listy punktow ktore maja ustawione connection do tego samego punktu

                    Point2D connection = new Point2D(layer[x, y].ConnectionX.Value, layer[x, y].ConnectionY.Value);

                    wasProccessed[x, y] = true;

                    Point2D[] neigh = Helper.GetVonNeumannNeighbors(x, y, width, height);
                    List<Point2D> expandedList = new List<Point2D>();
                    foreach (Point2D p in neigh)
                        if (layer[p.X, p.Y] != null)
                            if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpandedContour)
                            {
                                if (layer[p.X, p.Y].ConnectionX == connection.X && layer[p.X, p.Y].ConnectionY == connection.Y)
                                {
                                    if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID && (p.X != x || p.Y != y))
                                        expandedList.Add(p);
                                }
                            }

                    neigh = null;
                    if (expandedList.Count < 1) continue;

                    List<Point2D> side1 = new List<Point2D>();
                    List<Point2D> side2 = new List<Point2D>();

                    side1.Add(new Point2D(x, y));
                    side1.Add(expandedList[0]);

                    if (expandedList.Count > 1)
                    {
                        side2.Add(new Point2D(x, y));
                        side2.Add(expandedList[1]);
                    }


                    // dla side1
                    bool added = true;
                    while (added)
                    {
                        added = false;
                        Point2D current = side1[side1.Count - 1];
                        Point2D last = side1[side1.Count - 2];

                        neigh = Helper.GetVonNeumannNeighbors(current.X, current.Y, width, height);
                        foreach (Point2D p in neigh)
                            if (layer[p.X, p.Y] != null)
                                if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpandedContour)
                                {
                                    if (layer[p.X, p.Y].ConnectionX == connection.X && layer[p.X, p.Y].ConnectionY == connection.Y)
                                    {
                                        if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID &&
                                            (p.X != current.X || p.Y != current.Y) &&
                                            (p.X != last.X || p.Y != last.Y))
                                        {
                                            side1.Add(p);
                                            added = true;
                                        }
                                    }
                                }
                    }

                    // dla side 2
                    if (expandedList.Count > 1) added = true; else added = false;
                    while (added)
                    {
                        added = false;
                        Point2D current = side2[side2.Count - 1];
                        Point2D last = side2[side2.Count - 2];

                        neigh = Helper.GetVonNeumannNeighbors(current.X, current.Y, width, height);
                        foreach (Point2D p in neigh)
                            if (layer[p.X, p.Y] != null)
                                if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpandedContour)
                                {
                                    if (layer[p.X, p.Y].ConnectionX == connection.X && layer[p.X, p.Y].ConnectionY == connection.Y)
                                    {
                                        if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID &&
                                            (p.X != current.X || p.Y != current.Y) &&
                                                (p.X != last.X || p.Y != last.Y))
                                        {
                                            side2.Add(p);
                                            added = true;
                                        }
                                    }
                                }
                    }

                    // laczenie side1 i side2
                    side1.Reverse();
                    List<Point2D> margeLayerList = side1;

                    if (side2.Count > 0)
                    {
                        side2.RemoveAt(0);
                        margeLayerList.AddRange(side2);
                    }

                    foreach (Point2D p in margeLayerList)
                        wasProccessed[p.X, p.Y] = true;

                    // tworzymy liste punktow konturu nie uzywanych do polaczen, a sasiadujacych z naszym polaczeniem

                    neigh = Helper.GetVonNeumannNeighbors(connection.X, connection.Y, width, height);
                    expandedList.Clear();
                    foreach (Point2D p in neigh)
                        if (layer[p.X, p.Y] != null)
                            if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                            {
                                if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID && (p.X != connection.X || p.Y != connection.Y))
                                    if (!wasProccessed[p.X, p.Y])
                                        expandedList.Add(p);

                            }

                    neigh = null;
                    if (expandedList.Count < 1) continue;

                    List<Point2D> sideCommon1 = new List<Point2D>();
                    List<Point2D> sideCommon2 = new List<Point2D>();

                    sideCommon1.Add(new Point2D(connection.X, connection.Y));
                    sideCommon1.Add(expandedList[0]);

                    if (expandedList.Count > 1)
                    {
                        sideCommon2.Add(new Point2D(connection.X, connection.Y));
                        sideCommon2.Add(expandedList[1]);
                    }


                    // dla sideCommon1
                    added = true;
                    while (added)
                    {
                        added = false;
                        Point2D current = sideCommon1[sideCommon1.Count - 1];
                        Point2D last = sideCommon1[sideCommon1.Count - 2];

                        neigh = Helper.GetVonNeumannNeighbors(current.X, current.Y, width, height);
                        foreach (Point2D p in neigh)
                            if (layer[p.X, p.Y] != null)
                                if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                                {
                                    if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID &&
                                        (p.X != current.X || p.Y != current.Y) &&
                                        (p.X != last.X || p.Y != last.Y))
                                        if (!wasProccessed[p.X, p.Y])
                                        {
                                            sideCommon1.Add(p);
                                            added = true;
                                        }

                                }
                    }

                    if (expandedList.Count > 1) added = true; else added = false;
                    while (added)
                    {
                        added = false;
                        Point2D current = sideCommon2[sideCommon2.Count - 1];
                        Point2D last = sideCommon2[sideCommon2.Count - 2];

                        neigh = Helper.GetVonNeumannNeighbors(current.X, current.Y, width, height);
                        foreach (Point2D p in neigh)
                            if (layer[p.X, p.Y] != null)
                                if (layer[p.X, p.Y].Type == LayerPoint.LayerPointType.ExpansibleContour)
                                {
                                    if (layer[p.X, p.Y].SeedID == layer[x, y].SeedID &&
                                        (p.X != current.X || p.Y != current.Y) &&
                                        (p.X != last.X || p.Y != last.Y))
                                        if (!wasProccessed[p.X, p.Y])
                                        {
                                            sideCommon2.Add(p);
                                            added = true;
                                        }

                                }
                    }


                    sideCommon1.Reverse();
                    List<Point2D> margeCommonList = sideCommon1;

                    if (sideCommon2.Count > 0)
                    {
                        sideCommon2.RemoveAt(0);
                        margeCommonList.AddRange(sideCommon2);
                    }

                    //foreach (Point2D p in margeCommonList)
                    //    wasProccessed[p.X, p.Y] = true;

                    // przepisujemy polaczenia
                    if (margeCommonList.Count > margeLayerList.Count)
                    {
                        int newConnectionnCount = margeLayerList.Count;
                        int startIndex = (int)(((margeCommonList.Count - margeLayerList.Count) / 2));

                        for (int i = startIndex, j = 0; i < startIndex + newConnectionnCount && j < newConnectionnCount; i++, j++)
                        {
                            layer[margeLayerList[j].X, margeLayerList[j].Y].ConnectionX = margeCommonList[i].X;
                            layer[margeLayerList[j].X, margeLayerList[j].Y].ConnectionY = margeCommonList[i].Y;

                            wasProccessed[margeCommonList[i].X, margeCommonList[i].Y] = true;
                        }

                    }

                    margeLayerList.Clear();
                    margeCommonList.Clear();
                    side1.Clear();
                    side2.Clear();
                    sideCommon1.Clear();
                    sideCommon2.Clear();
                    expandedList.Clear();

                    neigh = null;
                    margeCommonList = null;
                    margeLayerList = null;
                    side1 = null;
                    side2 = null;
                    sideCommon1 = null;
                    sideCommon2 = null;
                    expandedList = null;

                    GC.Collect();


                }
            }




        }

        private void GetPointWithTheSameConnection(int x, int y, LayerType layerType)
        {
            LayerPoint[,] layer = null;

            if (layerType == LayerType.Up)
                layer = upLayer;

            if (layer == null) return;

            LayerPoint initPoint = layer[x, y];
            Point2D connection = new Point2D(initPoint.ConnectionX.Value, initPoint.ConnectionY.Value);



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
            if (descriptor.EqualDown(c))
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
                layerColor = descriptor.DownColor;

            // else
            //     layerColor = descriptor.CommonColor;


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
                layer = downLayer;


            if (layer == null) return;

            for (int x = 0; x < width; x++)
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
                        if (layerType == LayerType.Up)
                            bm.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        else
                            bm.SetPixel(x, y, Color.FromArgb(0, 0, 255));
                }

            bm.Save(fileName);

        }


    }
}
