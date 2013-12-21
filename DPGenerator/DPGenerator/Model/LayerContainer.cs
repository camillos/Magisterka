using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    class LayerContainer
    {
        public class Connection
        {
            public ulong ID1;
            public ulong ID2;

            public Connection(ulong id1 = 0, ulong id2 = 0)
            {
                ID1 = id1;
                ID2 = id2;
            }
        }

        public class Vertex
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public ulong ID { get; set; }

            public Vertex(int x = 0, int y = 0, int z = 0, ulong id = 0)
            {
                X = x;
                Y = y;
                Z = z;
                ID = id;
            }
        }




        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

        public ulong?[, ,] Space { get; set; }
        public List<Connection> Connections { get; set; }
        public ulong PointCount { get { return counter; } }

        private ulong counter = 0;

        public LayerContainer(int maxX, int maxY, int maxZ)
        {
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;


            Space = new ulong?[MaxX, MaxY, MaxZ];
            Connections = new List<Connection>();
        }

        public void Add(int upZ, int middleZ, int downZ, LayerConnector connector)
        {
            LayerPoint[,] layer = null;

            layer = connector.upLayer;
            int z = upZ;

            // wpisujemy do przestrzeni punkty z gornego przekroju
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;
                    if (Space[x, y, z].HasValue) continue;

                    if (!Space[x, y, z].HasValue)
                        Space[x, y, z] = counter++;
                }
            }
            // wpsiujemy do przestrzeni punkty ze srodka (wyciagane jako polaczenia z gornego przekroju)
            z = middleZ;
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;

                    int set_x = x;
                    int set_y = y;

                    if (layer[x, y].Type != LayerPoint.LayerPointType.CommonContour)
                    {
                        if (layer[x, y].ConnectionX.HasValue && layer[x, y].ConnectionY.HasValue)
                        {
                            set_x = layer[x, y].ConnectionX.Value;
                            set_y = layer[x, y].ConnectionY.Value;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("LayerContainer: Empty connection in LayerPoint");
                            continue;
                        }
                    }


                    if (!Space[set_x, set_y, z].HasValue)
                        Space[set_x, set_y, z] = counter++;
                }
            }

            // wpsiujemy do przestrzeni punkty ze srodka (wyciagane jako polaczenia z dolnego przekroju)
            // prawdopodobnie to bedzie mozna usunac po upewnieniu sie ze kod wyzej wpisuje wszytskie potrzebne punkty

            layer = connector.downLayer;
            z = middleZ;

            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;

                    int set_x = x;
                    int set_y = y;

                    if (layer[x, y].Type != LayerPoint.LayerPointType.CommonContour)
                    {
                        if (layer[x, y].ConnectionX.HasValue && layer[x, y].ConnectionY.HasValue)
                        {
                            set_x = layer[x, y].ConnectionX.Value;
                            set_y = layer[x, y].ConnectionY.Value;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("LayerContainer: Empty connection in LayerPoint");
                            continue;
                        }
                    }


                    if (!Space[set_x, set_y, z].HasValue)
                        Space[set_x, set_y, z] = counter++;
                }
            }

            // wpisujemy punkty z dolengo przekroju
            z = downZ;
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;
                    if (Space[x, y, z].HasValue) continue;

                    if (!Space[x, y, z].HasValue)
                        Space[x, y, z] = counter++;
                }
            }

            // mamy juz wszystkie punkty wiec tworzymy polaczenia

            layer = connector.upLayer;
            z = upZ;

            // dla gora - srodek
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;

                    int conn_x = x;
                    int conn_y = y;

                    if (layer[x, y].Type != LayerPoint.LayerPointType.CommonContour)
                    {
                        if (layer[x, y].ConnectionX.HasValue && layer[x, y].ConnectionY.HasValue)
                        {
                            conn_x = layer[x, y].ConnectionX.Value;
                            conn_y = layer[x, y].ConnectionY.Value;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    Connections.Add(new Connection(Space[x, y, z].Value, Space[conn_x, conn_y, middleZ].Value));
                }
            }
            // dla srodek - dol
            layer = connector.downLayer;
            z = downZ;

            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    if (layer[x, y] == null) continue;

                    int conn_x = x;
                    int conn_y = y;

                    if (layer[x, y].Type != LayerPoint.LayerPointType.CommonContour)
                    {
                        if (layer[x, y].ConnectionX.HasValue && layer[x, y].ConnectionY.HasValue)
                        {
                            conn_x = layer[x, y].ConnectionX.Value;
                            conn_y = layer[x, y].ConnectionY.Value;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    Connections.Add(new Connection(Space[conn_x, conn_y, middleZ].Value, Space[x, y, z].Value));
                }
            }

        }


        public List<Vertex> GetVertex()
        {
            List<Vertex> vertex = new List<Vertex>();

            for (int x = 0; x < MaxX; x++)
                for (int y = 0; y < MaxY; y++)
                    for (int z = 0; z < MaxZ; z++)
                    {
                        if (Space[x, y, z].HasValue)
                        {
                            vertex.Add(new Vertex(x, y, z, Space[x, y, z].Value));
                            Space[x, y, z] = null;
                        }
                    }

            vertex = vertex.OrderBy(v => v.ID).ToList();
            return vertex;

        }

        public void ClearContainer()
        {
            Connections.Clear();
            Connections = null;

            Space = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }




    }
}
