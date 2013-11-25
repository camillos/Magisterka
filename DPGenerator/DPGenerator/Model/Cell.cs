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
            Contour,    // komurka stanowiaca kontur czesci wspolnej

            Upper,      // komórka znajdująca sie na przekroju górnym, ale nie mam jej na dolnym
            Lower       // komórka znajdująca sie na przekroju dolnym, ale nie mam jej na górnym
        }

        public int X { get; set; }
        public int Y { get; set; }
        public CellType Type { get; set; }

        public int InitialX { get; private set; }
        public int InitialY { get; private set; }


        public Cell()
        {
            X = -1;
            Y = -1;
            Type = CellType.Undefined;

            SaveInitialCords();
        }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Type = CellType.Undefined;

            SaveInitialCords();
        }

        public Cell(int x, int y, CellType type)
        {
            X = x;
            Y = y;
            Type = type;

            SaveInitialCords();
        }

        public Cell(Cell pattern)
        {
            X = pattern.X;
            Y = pattern.Y;
            Type = pattern.Type;

            InitialX = pattern.InitialX;
            InitialY = pattern.InitialY;
        }

        private void SaveInitialCords()
        {
            InitialX = X;
            InitialY = Y;
        }

    }
}
