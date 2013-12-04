using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPGenerator.Model;

namespace DPGenerator
{
    class PlyWriter
    {
        public static void WriteConnection(LevelPoint[,] level, int width, int height)
        {
            long[,] idUp = new long[width, height];
            long[,] idDown = new long[width, height];

            long counter = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (level[x, y] == null) continue;
                    counter++;
                }
            }


            string[] header = new string[] {
                                            "ply",
                                            "format ascii 1.0",
                                            "element vertex ",   //2
                                            "property float x",
                                            "property float y",
                                            "property float z",
                                            "property uchar red",
                                            "property uchar green",
                                            "property uchar blue",
                                            "element edge ",  //9
                                            "property int vertex1",
                                            "property int vertex2",
                                            "property uchar red",
                                            "property uchar green",
                                            "property uchar blue",
                                            "end_header"
                                             };
            header[2] += (counter * 2);
            header[9] += counter;





            using (System.IO.StreamWriter file = new System.IO.StreamWriter("connections.ply"))
            {
                foreach (string s in header)
                    file.WriteLine(s);

                counter = 0;
                string line = string.Empty;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;
                        line = string.Empty;

                        idUp[x, y] = counter++;
                        line += level[x, y].X;
                        line += " ";
                        line += level[x, y].Y;
                        line += " ";
                        line += 100;
                        line += " 255 0 0";

                        file.WriteLine(line);

                    }
                }
                
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;

                        line = string.Empty;

                        idDown[x, y] = counter++;
                        line += level[x, y].Owner.X;
                        line += " ";
                        line += level[x, y].Owner.Y;
                        line += " ";
                        line += 0;
                        line += " 255 0 0";

                        file.WriteLine(line);
                        
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;

                        line = string.Empty;
                        line += idUp[x, y];
                        line += " ";
                        line += idDown[x, y];

                        line += " 0 0 255";

                        file.WriteLine(line);
                    }
                }


                
            }
        }

        public static void WriteConnection_not_finished(LevelPoint[,] level, LevelPoint[,] midleLevel, int width, int height)
        {
            // rozpoczynamy od zapisu midleLevel
            int mildeCounter = 0;
            int levelCounter = 0;
            int[,] midleID = new int[width, height];
            int[,] levelID = new int[width, height];

            string line;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("connections.ply"))
            {
                // zapis punktow ze wzpolnej warstwy
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        midleID[x, y] = -1;
                        if(midleLevel[x, y] == null) continue;

                        line = x.ToString() + " " + y.ToString() + " 0 0 255 0";
                        mildeCounter++;
                        midleID[x, y] = mildeCounter;

                        file.WriteLine(line);
                    }
                }

                // zapis punktow z dolenj warstwy
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        levelID[x, y] = -1;
                        if (level[x, y] == null) continue;
                        
                        line = x.ToString() + " " + y.ToString() + " 100 255 0 0";
                        levelCounter++;
                        levelID[x, y] = levelCounter;

                        file.WriteLine(line);
                    }
                }

                // zapis polaczen
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;

                        LevelPoint owner = level[x, y].Owner;

                        if(owner == null)
                        {
                            System.Diagnostics.Debug.WriteLine("pusty owner przy zapisie do pliku");
                            continue;
                        }

                        line = levelID[x, y].ToString() + midleID[owner.X, owner.Y] + " 0 0 255";
                        file.WriteLine(line);
                    }
                }

                // zerowanie id na srodku, ktore zostaly juz zapisane
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;
                        LevelPoint owner = level[x, y].Owner;

                        if (owner == null) continue;

                        midleID[owner.X, owner.Y] = -1;

                    }
                }

               



            }
        }

        public static void WriteConnection(LevelPoint[,] level, LevelPoint[,] midleLevel, int width, int height)
        {
            int[,] midleUsed = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    midleUsed[x, y] = 0;
            

            // wyliczamy czy punkty z czesci wspolnej byly uzyte
            // aby muc pozniej dorobic do nich polaczenia
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (level[x, y] == null) continue;

                    LevelPoint owner = level[x, y].Owner;
                    if (owner == null)
                    {
                        System.Diagnostics.Debug.WriteLine("pusty owner przy zapisie do pliku");
                        continue;
                    }

                    midleUsed[owner.X, owner.Y] = midleUsed[owner.X, owner.Y] + 1;
                }
            }

            int unusedCount = 0;

            // sprawdzamy ile punktow jest nieuzywanych
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (midleLevel[x, y] != null && midleUsed[x, y] == 0)
                    {
                        unusedCount++;
                    }
                }
            }



            // tworzymy naglowek
            int levelCounter = 0;
            int midleCounter = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (level[x, y] != null)
                        levelCounter++;
                    if (midleLevel[x, y] != null)
                        midleCounter++;
                }
            }
            /////////////////////////!!!!!!!!!!!!!/////////////////////////
            unusedCount = 0;
            //////////////////////////////////////////////////////////////
            string[] header = new string[] {
                                            "ply",
                                            "format ascii 1.0",
                                            "element vertex ",   //2
                                            "property float x",
                                            "property float y",
                                            "property float z",
                                            "property uchar red",
                                            "property uchar green",
                                            "property uchar blue",
                                            "element edge ",  //9
                                            "property int vertex1",
                                            "property int vertex2",
                                            "property uchar red",
                                            "property uchar green",
                                            "property uchar blue",
                                            "end_header"
                                             };
            header[2] += (midleCounter + levelCounter + unusedCount);
            header[9] += (levelCounter + unusedCount);

            int counter = 0;
            int[,] midleID = new int[width, height];
            int[,] levelID = new int[width, height];
            int[,] unusedID = new int[width, height];

            string line;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("connections_v2.ply"))
            {
                foreach (string s in header)
                    file.WriteLine(s);

                // zapis punktow ze wzpolnej warstwy
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        midleID[x, y] = -1;
                        if (midleLevel[x, y] == null) continue;

                        line = x.ToString() + " " + y.ToString() + " 0 0 255 0";
                        midleID[x, y] = counter++;

                        file.WriteLine(line);
                    }
                }

                // zapis punktow z dodanej warstwy
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        levelID[x, y] = -1;
                        if (level[x, y] == null) continue;

                        line = x.ToString() + " " + y.ToString() + " 100 255 0 0";
                        levelID[x, y] = counter++;

                        file.WriteLine(line);
                    }
                }

                //// zapis punktow z wspolnej warstwy, ktore byly nieuzywane na dodanej warstwie
                //for (int x = 0; x < width; x++)
                //{
                //    for (int y = 0; y < height; y++)
                //    {
                //        unusedID[x, y] = -1;
                //        // jesli punkty wystepuja na wspolnej warstwie
                //        // ale nie zostaly uzyte
                //        if (midleLevel[x, y] != null && midleUsed[x, y] == 0)
                //        {
                //            line = x.ToString() + " " + y.ToString() + " 100 255 255 0";
                //            unusedID[x, y] = counter++;

                //            file.WriteLine(line);
                //        }
                //    }
                //}


                // tworzymy polaczenia
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (level[x, y] == null) continue;

                        LevelPoint owner = level[x, y].Owner;

                        if (owner == null)
                        {
                            System.Diagnostics.Debug.WriteLine("pusty owner przy zapisie do pliku");
                            continue;
                        }

                        line = levelID[x, y].ToString() + " " + midleID[owner.X, owner.Y] + " 0 0 255";
                        file.WriteLine(line);
                    }

                }

                //// tworzymy polaczenia dla nie uzywanych punktow z wspolnej warstwy
                //for (int x = 0; x < width; x++)
                //{
                //    for (int y = 0; y < height; y++)
                //    {
                //        if (midleLevel[x, y] != null && midleUsed[x, y] == 0)
                //        {
                //            line = midleID[x, y].ToString() + " " + unusedID[x, y] + " 0 0 255";
                //            file.WriteLine(line);
                //        }
                //    }
                //}            
            }





        }

    }
}
