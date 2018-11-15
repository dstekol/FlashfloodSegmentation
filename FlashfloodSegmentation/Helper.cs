using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace FlashfloodSegmentation
{
    //helper class for doing circular search through section map array
    public class Helper 
    {
        public static int[][] moveAround =
            {
            new int[] { -1, -1 },
            new int[] { 0, -1 },
            new int[] { 1, -1 },
            new int[] { -1, 0 },
            new int[] { 1, 0 },
            new int[] { -1, 1 },
            new int[] { 0, 1 },
            new int[] { 1, 1 }
            };
        public static int[][] moveForward =
        {
            new int[] {-1, 1 },
            new int[] {0, 1 },
            new int[] {1, 1 },
            new int[] {1, 0 }
        };
        Point start;
        bool forwardOnly;
        public int[][] idealMovePlan;
        public List<Point> actualMovePlan;
        int currentPos;
        int maxX, maxY;

        public Helper(Point s, bool fOnly, int maxX, int maxY) //creates Helper object, compiles movement plan
        {
            start = s;
            forwardOnly = fOnly;
            currentPos = -1;
            this.maxX = maxX;
            this.maxY = maxY;
            if (forwardOnly)
            {
                idealMovePlan = moveForward;
            }
            else
            {
                idealMovePlan = moveAround;
            }

            actualMovePlan = new List<Point>();

            for (int i = 0; i < idealMovePlan.Length; i++)
            {
                int x = start.X + idealMovePlan[i][0];
                int y = start.Y + idealMovePlan[i][1];

                if (x >= 0 && x < this.maxX && y >= 0 && y < this.maxY)
                {
                    actualMovePlan.Add(new Point(x, y));
                }
            }
        }

        public bool keepGoing() //returns whether or not traversal around point is finished
        {
            return currentPos + 1 < actualMovePlan.Count;
        }

        public Point getNextPoint() //returns the next point to look at (absolute position)
        {
            currentPos++;
            return actualMovePlan[currentPos];
        }

        private static bool withinBounds<T>(T[,] arr, int x, int y) //returns whether point in question is within array bounds (used internally)
        {
            return x >= 0 && y >= 0 && x < arr.GetLength(0) && y < arr.GetLength(1);
        }
    }
}
