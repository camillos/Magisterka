using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    /// <summary>
    /// Punkty do automatu komurkowego
    /// (9 bajtow na punkt)
    /// </summary>
    class LayerPoint
    {
        public enum LayerPointType : sbyte
        {
            CommonContour,       // punkty konturu czesci wspolnje 
            ExpansibleContour,   // punkty konturu czesci wspolnje, ktore mozna dopasaowac 
                                 // do analizowanej warstwy musza miec sasiadow z poza czesci wspolnej
            ExpandedContour,     // punkty rozszerzonego konturu
            ExpansiblePoint,     // punkty z rozszerzonej czesci    
            
            Undefined            // w przypadku gdy nie jestesmy w stanie okreslic typu komurki  
        }

        public int? ConnectionX { get; set; }
        public int? ConnectionY { get; set; }
        public LayerPointType Type { get; set; }
        public int? SeedID { get; set; }

        public LayerPoint()
        {
            Type = LayerPointType.Undefined;
        }

        public LayerPoint(int x, int y, LayerPointType type)
        {
            Type = type;

            if (Type == LayerPointType.ExpansibleContour)
            {
                ConnectionX = x;
                ConnectionY = y;
            }
        }

        //public LayerPoint GetTransformedToExpanded()
        //{
        //    if (Type != LayerPointType.ExpansiblePoint && )
        //    {
        //        System.Diagnostics.Debug.WriteLine("Transform to Extended -> wrong point. Point type:" + Type.ToString());
        //        return null;
        //    }

        //    LayerPoint transformed = new LayerPoint();

        //    Type = LayerPointType.ExpandedContour;
        //    ConnectionX = pattern.ConnectionX;
        //    ConnectionY = pattern.ConnectionY;
        //}

    }
}
