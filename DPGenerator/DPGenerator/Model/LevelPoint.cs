using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    class LevelPoint
    {
        public enum LevelPointType
        {
            Undefined,
            Countour,
            Extended
        }


        public int X { get; set; }
        public int Y { get; set; }
        public LevelPointType Type { get; set; }
        public bool Processed { get; private set; }

        public LevelPoint Owner { get; set; }


        public LevelPoint()
        {
            X = -1;
            Y = -1;
            Type = LevelPointType.Undefined;

            Helper.LevelPointCount++;
        }

        public LevelPoint(int x, int y)
        {
            X = x;
            Y = y;
            Type = LevelPointType.Undefined;

            Helper.LevelPointCount++;
        }

        public LevelPoint(int x, int y, LevelPointType type)
        {
            X = x;
            Y = y;
            Type = type;

            if (type == LevelPointType.Countour)
            {
                Processed = true;
                Owner = this;
            }

            Helper.LevelPointCount++;
        }

        public void SetAsProcessed (LevelPoint owner)
        {
            Processed = true;
            Owner = owner;

            if (Owner.Type != LevelPointType.Countour)
                System.Diagnostics.Debug.WriteLine("Wlasciciel nie jest konturem");
        }
    }
}
