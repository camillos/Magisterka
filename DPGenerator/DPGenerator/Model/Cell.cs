using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    /// <summary>
    /// Klasa reprezentująca komurkę w automacie komurkowym
    /// </summary>
    class Cell 
    {
        /// <summary>
        /// Określa typ komurki
        /// </summary>
        public enum CellType
        {
            Undefined,  // komurka nie zdefiniowana
            Empty,      // komurka pusta - faza której nie rozpatrujemy

            Common,     // komurka wspólna dla przekroju dolengo i górnego
            //Contour,    // komurka stanowiaca kontur czesci wspolnej

            Upper,      // komórka znajdująca sie na przekroju górnym, ale nie mam jej na dolnym
            Lower       // komórka znajdująca sie na przekroju dolnym, ale nie mam jej na górnym
        }

        public int X { get; set; }
        public int Y { get; set; }
        public CellType Type { get; set; }
        public bool IsContour { get; set; }

        public int InitialX { get; private set; }
        public int InitialY { get; private set; }
        public CellType InitialType { get; private set; }

        public bool IsExtended { get; set; }

        // polaczenia maja tylko punkty na konturze czesci wspolnej
        // jesli punkt na konturze czesci wspolnej nie rozszerzaja sie 
        // (czyli na dolenj i gornej plaszczyznie rowniez stanowia kontur)
        // to jako polaczenie maja zawsze wpisanych samych siebie


        // punkt ktory tworzy polaczenie z nasza komurka na gornaje plaszczyznie
        public List<Cell> UpConnectedCells                      
        {
            get
            {
                if (this.IsContour)
                {
                    if (UpConnectedCells == null)
                        UpConnectedCells = new List<Cell>();
                    return UpConnectedCells;
                }
                else return null;
            }
            set
            {
                UpConnectedCells = value;
            }
        }

        // punkt ktory tworzy polaczenie z nasza komurka na dolnej plaszzyznie
        public List<Cell> DownConnectedCells
        {
            get
            {
                if (this.IsContour)
                {
                    if (DownConnectedCells == null)
                        DownConnectedCells = new List<Cell>();
                    return DownConnectedCells;
                }
                else return null;
            }
            set
            {
                DownConnectedCells = value;
            }
        }     
        

        public Cell()
        {
            X = -1;
            Y = -1;
            Type = CellType.Undefined;
            IsContour = false;
            IsExtended = false;

            SaveInitialCords();

        }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Type = CellType.Undefined;
            IsContour = false;
            IsExtended = false;

            SaveInitialCords();
        }

        public Cell(int x, int y, CellType type, bool isCountour)
        {
            X = x;
            Y = y;
            Type = type;
            IsContour = isCountour;
            IsExtended = false;

            SaveInitialCords();
        }

        public Cell(Cell pattern)
        {
            X = pattern.X;
            Y = pattern.Y;
            Type = pattern.Type;
            
            //IsContour = pattern.IsContour;
            IsContour = false;
            IsExtended = true;

            InitialX = pattern.InitialX;
            InitialY = pattern.InitialY;
        }

        public Cell(int x, int y, Cell pattern)
        {
            X = x;
            Y = y;
            Type = pattern.Type;
            IsContour = false;
            IsExtended = true;

            InitialX = pattern.InitialX;
            InitialY = pattern.InitialY;
        }

        private void SaveInitialCords()
        {
            InitialX = X;
            InitialY = Y;
            InitialType = Type;
        }

    }
}
