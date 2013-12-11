using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using DPGenerator.Model;


namespace DPGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> filesName = new List<string>();
        private List<Point3D> vertex = new List<Point3D>();

        ulong vertexCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFiles()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;

            Nullable<bool> result = dlg.ShowDialog();
            filesName.Clear();
            if (result == true)
            {
                foreach (string s in dlg.FileNames)
                {
                    filesName.Add(s);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoadFiles();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // szary
            //GenerateFrom(96, 125, 143);

            // zielony 
            GenerateFrom(75, 238, 132);


            string file = GetPlyFileName();
            if (file != string.Empty)
            {
                //GenerateFrom(96, 125, 143, file);
                SaveAsPly(file);
            }

            MessageBox.Show("vertex: " + vertexCount);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            LoadFiles();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (filesName.Count < 2) return;

            CommonPart cp = new CommonPart(filesName[0], filesName[1]);
            cp.Generate(75, 238, 132);

            string file = GetBmpFileName();
            if (file != null)
            {
                cp.SeveBitmap(file);
            }
        }


        private void GenerateFrom(int r, int g, int b)
        {
            vertex.Clear();
            for (int z = 0; z < filesName.Count; z++)
            {
                Bitmap bitmap = new Bitmap(filesName[z]);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        System.Drawing.Color color = bitmap.GetPixel(x, y);

                        if (color.R == r && color.G == g && color.B == b)
                        {
                            vertex.Add(new Point3D() { X = x, Y = y, Z = z, R = r, G = g, B = b });
                            vertexCount++;
                        }
                    }
                }
            }
        }

        private void SaveAsPly(string fileName)
        {
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
            header[2] += (vertex.Count).ToString();
            header[9] += 0;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                foreach (string s in header)
                    file.WriteLine(s);

                string line = string.Empty;
                foreach (Point3D p in vertex)
                {
                    line = p.X.ToString() + " " + p.Y.ToString() + " " + p.Z.ToString() + " " + p.R.ToString() + " " + p.G.ToString() + " " + p.B.ToString();
                    file.WriteLine(line);

                    line = string.Empty;
                }
            }
        }


        public string GetPlyFileName()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PLY File|*.ply";
            saveFileDialog.Title = "Save as Ply File";
            saveFileDialog.ShowDialog();

            return saveFileDialog.FileName;
        }

        public string GetBmpFileName()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap File|*.bmp";
            saveFileDialog.Title = "Save as Bitmap File";
            saveFileDialog.ShowDialog();

            return saveFileDialog.FileName;
        }




        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
           // LoadFiles();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
           // if (filesName.Count < 2) return;

           // CommonPart cp = new CommonPart(filesName[0], filesName[1]);
           // cp.Generate(75, 238, 132);
           // cp.Clear();

           // CommonDescriptor descriptor = new CommonDescriptor()
           //     {
           //         CommonColor = System.Drawing.Color.FromArgb(75, 238, 132),
           //         UpColor = System.Drawing.Color.FromArgb(255, 0, 0),
           //         DownColor = System.Drawing.Color.FromArgb(0, 0, 255)
           //     };

           // //CellularAutomaton2D automat2D = new CellularAutomaton2D(cp.CommonBitmap, descriptor);


           // //automat2D.Run();
           // //automat2D.ClearUnnecessaryCells();


           // //automat2D.CreateConnection();
           // //List<Cell> connection = automat2D.GetCellWithConnection();

            
           // LevelConnector connector = new LevelConnector(cp.CommonBitmap, descriptor);

           // connector.CreateLevels();
           // connector.SaveLevel();
           // connector.ProcessLevel(LevelConnector.LevelType.Up);
           // connector.ProcessLevel(LevelConnector.LevelType.Down);

           // PlyWriter.WriteConnection(connector.upLevel, connector.middleLevel, cp.CommonBitmap.Width, cp.CommonBitmap.Height);
           //// PlyWriter.WriteConnection(connector.upLevel, cp.CommonBitmap.Width, cp.CommonBitmap.Height);
        
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            LoadFiles();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (filesName.Count < 2) return;

            //CommonPart cp = new CommonPart(filesName[0], filesName[1]);
            //cp.Generate(75, 238, 132);
            //cp.Clear();

            //CommonDescriptor descriptor = new CommonDescriptor()
            //{
            //    CommonColor = System.Drawing.Color.FromArgb(75, 238, 132),
            //    UpColor = System.Drawing.Color.FromArgb(255, 0, 0),
            //    DownColor = System.Drawing.Color.FromArgb(0, 0, 255)
            //};

            CommonCreator cc = new CommonCreator(filesName[0], filesName[1]);
            cc.Generate(75, 238, 132);
            cc.GenerateCommonDescriptor();

            LayerConnector connector = new LayerConnector(cc.CommonBitmap, cc.Descriptor);
            connector.CreateLayer();
            //connector.SaveLayer(LayerConnector.LayerType.Up, "initialLayer.bmp");
            connector.Run(LayerConnector.LayerType.Up);
            //connector.SaveLayer(LayerConnector.LayerType.Up, "after_automat.bmp");
            connector.ClearLayer(LayerConnector.LayerType.Up);
            //connector.SaveLayer(LayerConnector.LayerType.Up, "after_clear.bmp");
            // nie dziala ;/
            //connector.CompleteCommonConnection(LayerConnector.LayerType.Up);

            //PlyWriter.SaveConnection(connector.upLayer, connector.width, connector.height);

            connector.Run(LayerConnector.LayerType.Down);
            connector.ClearLayer(LayerConnector.LayerType.Down);


            LayerContainer lc = new LayerContainer(connector.width, connector.height, 10);
            lc.Add(0, 1, 2, connector);

            PlyWriter.SaveContainer(lc);
        }





        
             

    }
}
