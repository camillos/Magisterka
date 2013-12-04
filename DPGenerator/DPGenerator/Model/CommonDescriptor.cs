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
        //public Color CommonColor;
        public List<Color> CommonColor;
        public Color UpColor;
        public Color DownColor;


        public bool EqualCommon(Color color)
        {
            foreach (Color commonColor in CommonColor)
            {
                if (color.R == commonColor.R &&
                    color.G == commonColor.G &&
                    color.B == commonColor.B)
                {
                    return true;
                }
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

        public int GetSeedID(Color color)
        {
            for (int i = 0; i < CommonColor.Count; i++)
            {
                if (color.R == CommonColor[i].R &&
                   color.G == CommonColor[i].G &&
                   color.B == CommonColor[i].B)
                {
                    return i;
                }
            }

            return -1;
        }


    }
}
