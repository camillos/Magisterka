using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DPGenerator.Model
{
    /// <summary>
    /// Automat komurkowy w 2D. 
    /// Na podstawie bitmapy opisującej część wspólną oraz piksele z góry i dólu
    /// rozrasta część wspólną, tworząc dopasowania góra - środek - dół
    /// </summary>
    class CellularAutomaton2D
    {
        public List<Cell> OutputCells { get; private set; }
        
        
        
        private readonly int width;
        private readonly int height;

        private Cell[,] cells;
        private CommonDescriptor descriptor;

        private long iterCounter = 0;       // jesli trzeba zapisywac bitmapy z iteracji to potrzebujemy licznika
       
        /// <param name="common">Bitmapa częsci wspólnej</param>
        /// <param name="commonDescriptor">Deskryptor bitmapy, opisujeacy zastosowane kolory</param>
        public CellularAutomaton2D(Bitmap common, CommonDescriptor commonDescriptor)
        {
            width = common.Width;
            height = common.Height;
            descriptor = commonDescriptor;
            cells = new Cell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //cells[x, y] = new Cell(x, y, GetCellType(x, y, common));
                    cells[x, y] = CreateCell(x, y, common);
                }
            }
        }

        /// <summary>
        /// Uruchamia interacje automatu
        /// </summary>
        public void Run()
        {
            Cell[,] nextCells;

            bool mustContinue = true;
            while (mustContinue)
            {
                mustContinue = false;
                nextCells = new Cell[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // zmianie moze ulec tylko komurka z gory lub z dolu
                        // w kazdym innym przypadku przepisujemy stan komurki do nowej struktury
                        if(cells[x, y].Type != Cell.CellType.Lower &&
                            cells[x, y].Type != Cell.CellType.Upper)
                        {
                            nextCells[x, y] = cells[x, y];
                            continue;
                        }

                        //List<Cell> neigh =  GetMoorContourNeigh(x, y);
                        List<Cell> neigh = GetMoorNeigh(x, y);

                        // jesli komurka nie posiada w sasiedztwie zadnej komurki konturu
                        // czesci wspolnej to przepisujemy ja, aby mogla byc rozpatrywana
                        // w nastepnych iteracjach
                        if (neigh.Count < 1)
                        {
                            nextCells[x, y] = cells[x, y];
                            continue;
                        }

   
                        //----------------------------------------------------------------------
                        // prawdopodobnie bedzie tu trzeba zrobic rozroznienie ktozy sasiedzi
                        // sa z ktorej plamy, ale narazie nie mamy jeszcze takiej informacji
                        // oraz po wybraniu sasaida zaznaczyc ktory to, aby trzymac informacje do
                        // budowy modelu 3d
                        // to ponizej moze byc bezsensu

                        // wybieramy losowego sasiada
                        Random r = new Random();
                        int bestNeigh = r.Next(neigh.Count);

                        //nextCells[x, y] = new Cell(x, y, Cell.CellType.Contour);
                        nextCells[x, y] = new Cell(x, y, neigh[bestNeigh]);


                        
                    }
                }

                // przepisanie nowopowstalej struktury
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // jesli ktoras z komurek zostala konturem, aktualizujemy automat
                        if(!cells[x,y].IsContour &&
                            nextCells[x, y].IsContour )
                        {
                            cells[x, y].IsContour = true;
                            mustContinue = true;
                        }

                        // jesli ktoras z komurek zostala rozszerzeniem czesci wspolenj
                        if (!cells[x, y].IsExtended && nextCells[x, y].IsExtended)
                        {
                            //cells[x, y].IsExtended = true;
                            cells[x, y] = nextCells[x, y];
                            mustContinue = true;
                        }

                    }
                }

                // aktualizujemy typy komurek, bo ktoras z nich nie musi byc juz konturem
                //UpdateCellsType();


                // jesli configuracja wymaga zapisywania kazdej interacji na dysku generujemy bitmape
                if (Config.CA_SaveAllIteration)
                {
                    SaveIterationToBmp(string.Empty);
                }

                iterCounter++;
            }

            // juz jestesmy po automacie
            // wiec mozemy wyznaczyc polaczenia


            nextCells = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();


        }

        /// <summary>
        /// Tworzy polaczenia pomiedzy czescia wspolna a warstwa dolna oraz gorna
        /// </summary>
        public void CreateConnection()
        {
            List<CellConnection> connection = new List<CellConnection>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    // nie interesuja nas punkty nie stanowiace rozszrzenia ani konturu
                    if (!cells[x, y].IsExtended && !cells[x, y].IsContour)
                    {
                        // nie bedzie nam potrzebny wiec mozeny go zwolnic
                        if (cells[x, y].Type != Cell.CellType.Common)
                            cells[x, y] = null;

                        continue;
                    }

                    // bez warunków periodycznych
                    int leftX = x - 1; if (leftX < 0) leftX = 0;
                    int rightX = x + 1; if (rightX >= width) rightX = width - 1;
                    int topY = y - 1; if (topY < 0) topY = 0;
                    int downY = y + 1; if (downY >= height) downY = height - 1;

                    Point2D[] neighCoords = new Point2D[8];
                    neighCoords[0] = new Point2D(leftX, topY);
                    neighCoords[1] = new Point2D(x, topY);
                    neighCoords[2] = new Point2D(rightX, topY);

                    neighCoords[3] = new Point2D(leftX, y);
                    neighCoords[4] = new Point2D(rightX, y);

                    neighCoords[5] = new Point2D(leftX, downY);
                    neighCoords[6] = new Point2D(x, downY);
                    neighCoords[7] = new Point2D(rightX, downY);

                    bool needConnected = false;

                    // jesli ktorykolwiek z sasiadow jest z drugiej fazy to w tedy nasz punkt stanowi
                    // kontur i laczymy go
                    foreach (Point2D p in neighCoords)
                    {
                        // punkt zostal usuniety wiec musi stanowic druga faze
                        if (cells[p.X, p.Y] == null)
                        {
                            needConnected = true;
                        }
                        else if (cells[p.X, p.Y].Type == Cell.CellType.Empty)
                            needConnected = true;
                    }

                    if (needConnected)
                    {
                        int initialX = cells[x, y].InitialX;
                        int initialY = cells[x, y].InitialY;

                        if (cells[x, y].Type == Cell.CellType.Upper)
                        {
                            cells[initialX, initialY].UpConnectedCells.Add(cells[x, y]);
                        }

                        if (cells[x, y].Type == Cell.CellType.Lower)
                        {
                            cells[initialX, initialY].DownConnectedCells.Add(cells[x, y]);
                        }

                        if (cells[x, y].Type == Cell.CellType.Common && cells[x, y].IsContour)
                        {
                            cells[x, y].DownConnectedCells.Add(cells[x, y]);
                            cells[x, y].UpConnectedCells.Add(cells[x, y]);
                        }

                    }
                    else cells[x, y] = null;

                }
            }
        }


        /// <summary>
        /// Pobiera polaczenia pomiedzy punktami
        /// </summary>
        public List<Cell> GetCellWithConnection()
        {
            List<Cell> connection = new List<Cell>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // tylko punkty z konturu powinny miec polaczenia
                    if (!cells[x, y].IsContour) continue;

                    if (cells[x, y].UpConnectedCells != null)
                        if (cells[x, y].UpConnectedCells.Count > 0)
                            connection.Add(cells[x, y]);

                    if (cells[x, y].DownConnectedCells != null)
                        if (cells[x, y].DownConnectedCells.Count > 0)
                            connection.Add(cells[x, y]);
                    
                }
            }
            return connection;
        }





        #region Nie uzywane
        /// <summary>
        /// Sprawdza typ komurki, na podstawie jej koloru, oraz koloru sasaidow
        /// </summary>
        //private Cell.CellType GetCellType(int x, int y, Bitmap common)
        //{
        //    if(x  < 0 || x >= width || y < 0 || y >= height)
        //        return Cell.CellType.Undefined;

        //    Color pixel = common.GetPixel(x, y);

        //    // dla pikseli nalezacych tylko do gornego przekroju
        //    if (pixel.R == descriptor.UpColor.R &&
        //       pixel.G == descriptor.UpColor.G &&
        //       pixel.B == descriptor.UpColor.B)
        //    {
        //        return Cell.CellType.Upper;
        //    }
        //    else

        //        // dla pikseli nalezacych tylko do dolengo przekroju
        //        if (pixel.R == descriptor.DownColor.R &&
        //            pixel.G == descriptor.DownColor.G &&
        //            pixel.B == descriptor.DownColor.B)
        //        {
        //            return Cell.CellType.Lower;
        //        }
        //        else

        //            // dla pikseli nalezacych do czesci wspolnej
        //            if (pixel.R == descriptor.CommonColor.R &&
        //                pixel.G == descriptor.CommonColor.G &&
        //                pixel.B == descriptor.CommonColor.B)
        //            {
        //                // kontur jest wybierany na podstawie 8 sasiadów
        //                int leftX = x - 1;  if (leftX < 0) leftX = 0;
        //                int rightX = x + 1; if (rightX >= width) rightX = width - 1;
        //                int topY = y - 1;   if (topY < 0) topY = 0;
        //                int downY = y + 1;  if (downY >= height) downY = height - 1;

        //                // jesli ktory kolwiek sasiad ma inny kolor niz czesc wspolna to punkt jest na konturze
        //                Color neigh;
                        
        //                // lewy gorny sasiad [1]
        //                neigh = common.GetPixel(leftX, topY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // srodkowy gorny sasiad [2]
        //                neigh = common.GetPixel(x, topY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;


        //                // prawy gorny sasiad [3]
        //                neigh = common.GetPixel(rightX, topY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // lewy srodkowy sasiad [4]
        //                neigh = common.GetPixel(leftX, y);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // prawy srodkowy sasiad [5]
        //                neigh = common.GetPixel(rightX, y);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // lewy dolny sasiad [6]
        //                neigh = common.GetPixel(leftX, downY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // srodkowy dolny sasiad [7]
        //                neigh = common.GetPixel(x, downY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // prawy dolny sasiad [8]
        //                neigh = common.GetPixel(rightX, downY);
        //                if (neigh.R != descriptor.CommonColor.R ||
        //                    neigh.G != descriptor.CommonColor.G ||
        //                    neigh.B != descriptor.CommonColor.B)

        //                    return Cell.CellType.Contour;

        //                // kazdy z sasiadow ma kolor zgodny ze wspolnym kolorem
        //                return Cell.CellType.Common;
        //            }
        //            else

        //                // dla pikseli innej fazy
        //                return Cell.CellType.Empty;
        //}

        #endregion

        /// <summary>
        /// Tworzy komurke automatu, na podstawie piksela botmapy, oraz pikseli go otaczajacych
        /// </summary>
        private Cell CreateCell(int x, int y, Bitmap common)
        {
            Cell cell = new Cell();

            if (x < 0 || x >= width || y < 0 || y >= height)
                return cell;

            Color pixel = common.GetPixel(x, y);

            // dla pikseli nalezacych tylko do gornego przekroju
            if (pixel.R == descriptor.UpColor.R &&
               pixel.G == descriptor.UpColor.G &&
               pixel.B == descriptor.UpColor.B)
            {
                cell = new Cell(x, y, Cell.CellType.Upper, false);
                return cell;
            }

            // dla pikseli nalezacych tylko do dolengo przekroju
            if (pixel.R == descriptor.DownColor.R &&
                pixel.G == descriptor.DownColor.G &&
                pixel.B == descriptor.DownColor.B)
            {
                cell = new Cell(x, y, Cell.CellType.Lower, false);
                return cell;
            }


            // dla pikseli nalezacych do czesci wspolnej
            if (pixel.R == descriptor.CommonColor.R &&
                pixel.G == descriptor.CommonColor.G &&
                pixel.B == descriptor.CommonColor.B)
            {
                // kontur jest wybierany na podstawie 8 sasiadów
                int leftX = x - 1; if (leftX < 0) leftX = 0;
                int rightX = x + 1; if (rightX >= width) rightX = width - 1;
                int topY = y - 1; if (topY < 0) topY = 0;
                int downY = y + 1; if (downY >= height) downY = height - 1;

                // jesli ktory kolwiek sasiad ma inny kolor niz czesc wspolna to punkt jest na konturze
                Color neigh;

                // lewy gorny sasiad [1]
                neigh = common.GetPixel(leftX, topY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // srodkowy gorny sasiad [2]
                neigh = common.GetPixel(x, topY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // prawy gorny sasiad [3]
                neigh = common.GetPixel(rightX, topY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // lewy srodkowy sasiad [4]
                neigh = common.GetPixel(leftX, y);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // prawy srodkowy sasiad [5]
                neigh = common.GetPixel(rightX, y);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // lewy dolny sasiad [6]
                neigh = common.GetPixel(leftX, downY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // srodkowy dolny sasiad [7]
                neigh = common.GetPixel(x, downY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // prawy dolny sasiad [8]
                neigh = common.GetPixel(rightX, downY);
                if (neigh.R != descriptor.CommonColor.R ||
                    neigh.G != descriptor.CommonColor.G ||
                    neigh.B != descriptor.CommonColor.B)
                {
                    cell = new Cell(x, y, Cell.CellType.Common, true);
                    return cell;
                }

                // kazdy z sasiadow ma kolor zgodny ze wspolnym kolorem
                cell = new Cell(x, y, Cell.CellType.Common, false);
                return cell;
            }
            else
            {
                cell = new Cell(x, y, Cell.CellType.Empty, false);
            }

            return cell;
        }

        #region Nie uzywane

        /// <summary>
        /// Pobiera sasiadow komurki ktorzy sa konturem czesci wspolnej
        /// </summary>
        //private List<Cell> GetMoorContourNeigh(int x, int y)
        //{
            
        //    List<Cell> neigh = new List<Cell>();

        //    if (x < 0 || x >= width || y < 0 || y >= height)
        //        return neigh;

        //    // bez warunków periodycznych
        //    int leftX = x - 1;  if (leftX < 0) leftX = 0;
        //    int rightX = x + 1; if (rightX >= width) rightX = width - 1;
        //    int topY = y - 1;   if (topY < 0) topY = 0;
        //    int downY = y + 1;  if (downY >= height) downY = height - 1;

        //    if (cells[leftX, topY] != null && cells[leftX, topY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[leftX, topY]);

        //    if (cells[x, topY] != null && cells[x, topY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[x, topY]);

        //    if (cells[rightX, topY] != null && cells[rightX, topY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[rightX, topY]);



        //    if (cells[leftX, y] != null && cells[leftX, y].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[leftX, y]);

        //    if (cells[rightX, y] != null && cells[rightX, y].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[rightX, y]);


            
        //    if (cells[leftX, downY] != null && cells[leftX, downY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[leftX, downY]);

        //    if (cells[x, downY] != null && cells[x, downY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[x, downY]);

        //    if (cells[rightX, downY] != null && cells[rightX, downY].Type == Cell.CellType.Contour)
        //        neigh.Add(cells[rightX, downY]);

        //    return neigh;

        //}
        #endregion


        /// <summary>
        /// Pobiera sasiadow stanowiacych kontur lub rozszerzenie
        /// </summary>
        private List<Cell> GetMoorNeigh(int x, int y)
        {
            List<Cell> neigh = new List<Cell>();

            if (x < 0 || x >= width || y < 0 || y >= height)
                return neigh;

            // bez warunków periodycznych
            int leftX = x - 1;      if (leftX < 0) leftX = 0;
            int rightX = x + 1;     if (rightX >= width) rightX = width - 1;
            int topY = y - 1;       if (topY < 0) topY = 0;
            int downY = y + 1;      if (downY >= height) downY = height - 1;

            Point2D[] neighCoords = new Point2D[8];
            neighCoords[0] = new Point2D(leftX, topY);
            neighCoords[1] = new Point2D(x, topY);
            neighCoords[2] = new Point2D(rightX, topY);

            neighCoords[3] = new Point2D(leftX, y);
            neighCoords[4] = new Point2D(rightX, y);

            neighCoords[5] = new Point2D(leftX, downY);
            neighCoords[6] = new Point2D(x, downY);
            neighCoords[7] = new Point2D(rightX, downY);

            foreach (Point2D p in neighCoords)
            {
                if(cells[p.X, p.Y] != null)
                    if (cells[p.X, p.Y].IsContour || cells[p.X, p.Y].IsExtended)
                        neigh.Add(cells[p.X, p.Y]);
            }

            return neigh;
        }

       
        /// <summary>
        /// Aktualizuje stan komurek, w przypadku kiedy w trakcie dzialania automatu
        /// ktoras komurka konturu przeszla w czesc wspolna
        /// </summary>
        private void UpdateCellsType()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // dla tych komurek stan nie mogl ulec zmianie
                    if (cells[x, y].Type == Cell.CellType.Empty ||
                        cells[x, y].Type == Cell.CellType.Common ||
                        cells[x, y].Type == Cell.CellType.Undefined)
                    {
                        continue;
                    }
                    

                    int leftX = x - 1; if (leftX < 0) leftX = 0;
                    int rightX = x + 1; if (rightX >= width) rightX = width - 1;
                    int topY = y - 1; if (topY < 0) topY = 0;
                    int downY = y + 1; if (downY >= height) downY = height - 1;

                    int counter = 0;

                    if (cells[leftX, topY].Type == Cell.CellType.Lower || cells[leftX, topY].Type == Cell.CellType.Upper)
                    {                    
                        counter++;
                        
                    }
                    if (cells[x, topY].Type == Cell.CellType.Lower || cells[x, topY].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }
                    if (cells[rightX, topY].Type == Cell.CellType.Lower || cells[rightX, topY].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }




                    if (cells[leftX, y].Type == Cell.CellType.Lower || cells[leftX, y].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }
                    if (cells[rightX, y].Type == Cell.CellType.Lower || cells[rightX, y].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }



                    if (cells[leftX, downY].Type == Cell.CellType.Lower || cells[leftX, downY].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }
                    if (cells[x, downY].Type == Cell.CellType.Lower || cells[x, downY].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }
                    if (cells[rightX, downY].Type == Cell.CellType.Lower || cells[rightX, downY].Type == Cell.CellType.Upper)
                    {
                        counter++;
                    }
                    
                    if(counter == 0) cells[x, y].Type = Cell.CellType.Common;

                }
            }
        }


        /// <summary>
        /// Funkcja zapisujaca rozrastanie sie czescie wspulnej co interacje
        /// </summary>
        private void SaveIterationToBmp(string filePath)
        {
            // aby sie nie zapchiac
            //if (iterCounter < 50)
            //{
                Bitmap bm = new Bitmap(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color color = Color.Yellow;

                        if (cells[x, y].Type == Cell.CellType.Common)
                            color = descriptor.CommonColor;

                        if (cells[x, y].IsContour)
                            color = Color.Black;

                        if (cells[x, y].Type == Cell.CellType.Empty)
                            color = Color.White;
                        if (cells[x, y].Type == Cell.CellType.Lower)
                            color = descriptor.DownColor;
                        if (cells[x, y].Type == Cell.CellType.Undefined)
                            color = Color.Yellow;
                        if (cells[x, y].Type == Cell.CellType.Upper)
                            color = descriptor.UpColor;

                        if (cells[x, y].IsExtended)
                            color = Color.DarkGreen;

                        bm.SetPixel(x, y, color);
                    }
                }

                bm.Save(filePath + iterCounter.ToString() + "_iteration.bmp");
            //}
        }


        /// <summary>
        /// Usuwa niepotrzebne komurki, zostawiajac jedynie te ktore beda potrzebne
        /// do stworzenie polaczen
        /// </summary>
        public void ClearUnnecessaryCells()
        {
            List<Cell> necessaryCells = new List<Cell>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // kotur jest na pewno potrzebny
                    if (cells[x, y].IsContour)
                    {
                        necessaryCells.Add(cells[x, y]);
                        continue;
                    }

                    // jesli punkt nie jest rozszerzeniem to nie bedzie potrzebny
                    if (!cells[x, y].IsExtended) continue;


                    // bez warunków periodycznych
                    int leftX = x - 1; if (leftX < 0) leftX = 0;
                    int rightX = x + 1; if (rightX >= width) rightX = width - 1;
                    int topY = y - 1; if (topY < 0) topY = 0;
                    int downY = y + 1; if (downY >= height) downY = height - 1;

                    Point2D[] neighCoords = new Point2D[8];
                    neighCoords[0] = new Point2D(leftX, topY);
                    neighCoords[1] = new Point2D(x, topY);
                    neighCoords[2] = new Point2D(rightX, topY);

                    neighCoords[3] = new Point2D(leftX, y);
                    neighCoords[4] = new Point2D(rightX, y);

                    neighCoords[5] = new Point2D(leftX, downY);
                    neighCoords[6] = new Point2D(x, downY);
                    neighCoords[7] = new Point2D(rightX, downY);

                    bool isNecessary = false;

                    // jesli ktorys z sasiadow jest drugiej fazy, to punkt jest konturem
                    foreach (Point2D p in neighCoords)
                    {
                        if (cells[p.X, p.Y].Type == Cell.CellType.Empty)
                            isNecessary = true;
                    }

                    if (isNecessary)
                    {
                        necessaryCells.Add(cells[x, y]);
                    }

                }
            }


            cells = null;
            OutputCells = necessaryCells;

        }


        public List<CellConnection> GetConnection()
        {
            List<CellConnection> connection = new List<CellConnection>();

            var middleCells = from middle 
                              in OutputCells 
                              where middle.IsContour && middle.Type == Cell.CellType.Common 
                              select middle;

            foreach (Cell c in middleCells)
            {
                connection.Add(new CellConnection(){ Middle = c });
            }




            return connection;
        }


    }
}
