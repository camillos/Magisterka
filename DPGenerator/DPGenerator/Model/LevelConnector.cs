using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DPGenerator.Model
{
    /// <summary>
    /// Klasa realizujaca tworzenie polaczen pomiedzy dwoma przekrojami.
    /// </summary>
    class LevelConnector
    {
        public enum LevelType
        {
            Up,
            Down
        }


        private readonly int width;
        private readonly int height;
        private readonly CommonDescriptor descriptor;
        private readonly Bitmap common;

        private LevelPoint[,] middleLevel;
        private long countMiddleLevell = 0;

        public LevelPoint[,] upLevel;
        private long countUpLevel = 0;

        private LevelPoint[,] downLevel;
        private long countDownLevel = 0;


        public LevelConnector(Bitmap commonBitmap, CommonDescriptor commonDescriptor)
        {
            width = commonBitmap.Width;
            height = commonBitmap.Height;
            common = commonBitmap;
            descriptor = commonDescriptor;
        }

        public void CreateLevels()
        {
            middleLevel = new LevelPoint[width, height];
            upLevel = new LevelPoint[width, height];
            downLevel = new LevelPoint[width, height];

            Color color;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    color = common.GetPixel(x, y);

                    // piksel ma kolor czesci wspolnej
                    if (color.R == descriptor.CommonColor.R &&
                        color.G == descriptor.CommonColor.G &&
                        color.B == descriptor.CommonColor.B)
                    {
                        if (IsCommonContour(x, y))
                        {
                            LevelPoint point = new LevelPoint(x, y, LevelPoint.LevelPointType.Countour);
                            middleLevel[x, y] = point;
                            countMiddleLevell++;

                            if (IsCommonWithLevel(x, y, descriptor.UpColor))
                            {
                                upLevel[x, y] = point;
                                countUpLevel++;
                            }

                            if (IsCommonWithLevel(x, y, descriptor.DownColor))
                            {
                                downLevel[x, y] = point;
                                countDownLevel++;
                            }
                        }
                    }

                    // piksel ma kolor czesci gornej
                    if (color.R == descriptor.UpColor.R &&
                        color.G == descriptor.UpColor.G &&
                        color.B == descriptor.UpColor.B)
                    {
                        LevelPoint point = new LevelPoint(x, y, LevelPoint.LevelPointType.Extended);
                        upLevel[x, y] = point;
                        countUpLevel++;
                    }

                    // piksel ma kolor czesci dolnej
                    if (color.R == descriptor.DownColor.R &&
                        color.G == descriptor.DownColor.G &&
                        color.B == descriptor.DownColor.B)
                    {
                        LevelPoint point = new LevelPoint(x, y, LevelPoint.LevelPointType.Extended);
                        downLevel[x, y] = point;
                        countDownLevel++;
                    }
                }
            }
        }

        private bool IsCommonContour(int x, int y)
        {
            Color myColor = common.GetPixel(x, y);

            // jesli kolor naszego piksela jest inny niz color czesci wspolenj
            // to nie moze byc konturem
            if (myColor.R != descriptor.CommonColor.R ||
                myColor.G != descriptor.CommonColor.G ||
                myColor.B != descriptor.CommonColor.B)
                return false;


            Point2D[] neighbor = Helper.GetMoorNeighbors(x, y, width, height);
            Color neighColor;

            // jesli ktorykolwiek z sasiadow ma inny kolor niz czesc wspolna
            // to punkt nalezy do konturu
            foreach (Point2D p in neighbor)
            {
                neighColor = common.GetPixel(p.X, p.Y);

                if (neighColor.R != descriptor.CommonColor.R ||
                    neighColor.G != descriptor.CommonColor.G ||
                    neighColor.B != descriptor.CommonColor.B)

                    return true;
            }

            return false;

        }

        private bool IsCommonWithLevel(int x, int y, Color levelColor)
        {
            Point2D[] neighbor = Helper.GetMoorNeighbors(x, y, width, height);
            Color neighColor;

            foreach (Point2D p in neighbor)
            {
                neighColor = common.GetPixel(p.X, p.Y);

                if (neighColor.R == levelColor.R ||
                    neighColor.G == levelColor.G ||
                    neighColor.B == levelColor.B)

                    return true;
            }

            return false;
        }

        public void ProcessLevel(LevelType levelType)
        {
            LevelPoint[,] level = null;
            if (levelType == LevelType.Up)
                level = upLevel;
            if (levelType == LevelType.Down)
                level = downLevel;

            Automat automat = new Automat(level, width, height);
            automat.Run();

            ClearLevel(levelType);
        }

        private void ClearLevel(LevelType levelType)
        {
            LevelPoint[,] level = null;
            if (levelType == LevelType.Up)
                level = upLevel;
            if (levelType == LevelType.Down)
                level = downLevel;

            if (level == null) return;

            List<LevelPoint> pointsToDelete = new List<LevelPoint>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (level[x, y] == null) continue;

                    bool toDelete = true;

                    // jesli ktorykolwiek z sasiadow jest nullem, to nasz punkt jest konturem
                    // i go nie usuwamy
                    Point2D[] neighborCords = Helper.GetMoorNeighbors(x, y, width, height);
                    //List<LevelPoint> neighbor = new List<LevelPoint>();

                    // jesli punkt jest akurat konturem, to usuwamy go jesli sasiaduje
                    // z punktami extended (musi sasiadowac z 3 punktami  wjednej osi X lub Y)
                    if (level[x, y].Type == LevelPoint.LevelPointType.Countour)
                    {
                        toDelete = true;
                        //toDelete = false;
                        //// omijamy punkty kontruru ktore sa na brzegach struktury
                        //if (x > 0 && x < width - 1 && y > 0 && y < height - 1)
                        //{
                        //    int left = x - 1;
                        //    int right = x + 1;
                        //    int top = y - 1;
                        //    int down = y + 1;

                        //    int counter = 0;

                        //    // sprawdza gorena linie sasiadow
                        //    if (level[left, top] != null)
                        //        if (level[left, top].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[x, top] != null)
                        //        if (level[x, top].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[right, top] != null)
                        //        if (level[right, top].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;

                        //    if (counter == 3) toDelete = true;
                        //    counter = 0;

                        //    // sprawdza dolna linie sasiadow
                        //    if (level[left, down] != null)
                        //        if (level[left, down].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[x, down] != null)
                        //        if (level[x, down].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[right, down] != null)
                        //        if (level[right, down].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;

                        //    if (counter == 3) toDelete = true;
                        //    counter = 0;

                        //    // sprawdza lewa linie sasiadow
                        //    if (level[left, top] != null)
                        //        if (level[left, top].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[left, y] != null)
                        //        if (level[left, y].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[left, down] != null)
                        //        if (level[left, down].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;

                        //    if (counter == 3) toDelete = true;
                        //    counter = 0;

                        //    // sprawdza prawa linie sasiadow
                        //    if (level[right, top] != null)
                        //        if (level[right, top].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[right, y] != null)
                        //        if (level[right, y].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;
                        //    if (level[right, down] != null)
                        //        if (level[right, down].Type == LevelPoint.LevelPointType.Extended)
                        //            counter++;

                        //    if (counter == 3) toDelete = true;
                        //    counter = 0;
                        //}

                    }
                    else
                    {

                        foreach (Point2D p in neighborCords)
                        {
                            if (level[p.X, p.Y] == null) toDelete = false;
                        }
                    }

                    if (toDelete)
                    {
                        pointsToDelete.Add(level[x, y]);
                    }
                }
            }

            foreach (LevelPoint p in pointsToDelete)
            {
                level[p.X, p.Y] = null;
            }
            pointsToDelete.Clear();


        }

        public void SaveLevel()
        {
            Bitmap bm = new Bitmap(width, height);
            for(int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (middleLevel[x, y] != null)
                        bm.SetPixel(x, y, descriptor.CommonColor);

                    //if (upLevel[x, y] != null)
                    //    bm.SetPixel(x, y, descriptor.UpColor);

                    //if (downLevel[x, y] != null)
                    //    bm.SetPixel(x, y, descriptor.DownColor);
                }

            bm.Save("temp.bmp");
        }
        
    
    }
}
