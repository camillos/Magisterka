using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    class Automat
    {
        private readonly int width;
        private readonly int height;
        public LevelPoint[,] level;

        public Automat(LevelPoint[,] processLevel, int levelWidth, int levelHeight)
        {
            level = processLevel;
            width = levelWidth;
            height = levelHeight;
        }

        public void Run()
        {
            bool mustRun = true;
            LevelPoint[,] nextLevel;

            while (mustRun)
            {
                mustRun = false;
                nextLevel = new LevelPoint[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        nextLevel[x, y] = level[x, y];
                        if (level[x, y] == null) continue;

                        // kontur czesci wspolnej przepisujemy
                        if (level[x, y].Type == LevelPoint.LevelPointType.Countour)
                        {
                            nextLevel[x, y] = level[x, y];
                            continue;
                        }

                        // dla czesci rozszerzezajacej
                        if (level[x, y].Type == LevelPoint.LevelPointType.Extended)
                        {
                            // jesli punkt byl przetworzony to go przepisujemy
                            if (level[x, y].Processed)
                            {
                                nextLevel[x, y] = level[x, y];
                                continue;
                            }

                            // pobieramy nie nullowych i przetworzonych juz sasiadow
                            Point2D[] neighborCords = Helper.GetMoorNeighbors(x, y, width, height);
                            List<LevelPoint> neighbor = new List<LevelPoint>();
                            foreach (Point2D p in neighborCords)
                            {
                                if (level[p.X, p.Y] != null)
                                    if(level[p.X, p.Y].Processed)
                                        neighbor.Add(level[p.X, p.Y]);
                            }

                            if (neighbor.Count < 1) continue;

                            Random r = new Random();
                            LevelPoint bestPoint = neighbor[r.Next(neighbor.Count)];

                            nextLevel[x, y] = new LevelPoint(x, y, LevelPoint.LevelPointType.Extended);
                            nextLevel[x, y].SetAsProcessed(bestPoint.Owner);
                            mustRun = true;

                        }
                    }
                }

                // zaszla jakas zmiana wiec nalezy zaktualizowac lvl
                if (mustRun)
                {
                    for(int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                        {
                            LevelPoint nextLvl = nextLevel[x, y];
                            LevelPoint lvl = level[x, y];


                            if (nextLevel[x, y] == null && level[x, y] == null) continue;
                            if (nextLevel[x, y].Processed && !level[x, y].Processed)
                            {
                                level[x, y] = null;
                                level[x, y] = nextLevel[x, y];
                                nextLevel[x, y] = null;
                            }
                        }
                }

            }
        }

    }
}
