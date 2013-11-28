using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    class Helper
    {
        public static Point2D[] GetMoorNeighbors(int x, int y, int width, int height)
        {
            // bez warunków periodycznych
            int leftX = x - 1; if (leftX < 0) leftX = 0;
            int rightX = x + 1; if (rightX >= width) rightX = width - 1;
            int topY = y - 1; if (topY < 0) topY = 0;
            int downY = y + 1; if (downY >= height) downY = height - 1;

            Point2D[] neighbors = new Point2D[8];
            neighbors[0] = new Point2D(leftX, topY);
            neighbors[1] = new Point2D(x, topY);
            neighbors[2] = new Point2D(rightX, topY);

            neighbors[3] = new Point2D(leftX, y);
            neighbors[4] = new Point2D(rightX, y);

            neighbors[5] = new Point2D(leftX, downY);
            neighbors[6] = new Point2D(x, downY);
            neighbors[7] = new Point2D(rightX, downY);


            return neighbors;

        }

        public static long LevelPointCount;
    }
}
