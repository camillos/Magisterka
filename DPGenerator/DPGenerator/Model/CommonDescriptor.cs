using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DPGenerator.Model
{
    class CommonDescriptor
    {
        public Color CommonColor;
        public Color UpColor;
        public Color DownColor;


        public bool EqualCommon(Color color)
        {
            if(color.R == CommonColor.R &&
                color.G == CommonColor.G &&
                color.B == CommonColor.B)
            {
                return true;
            }

            return false;
        }

        public bool EqualUp(Color color)
        {
            if (color.R == UpColor.R &&
                color.G == UpColor.G &&
                color.B == UpColor.B)
            {
                return true;
            }

            return false;
        }


        public bool EqualDown(Color color)
        {
            if (color.R == DownColor.R &&
                color.G == DownColor.G &&
                color.B == DownColor.B)
            {
                return true;
            }

            return false;
        }


    }
}
