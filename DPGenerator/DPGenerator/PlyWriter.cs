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
    }
}
