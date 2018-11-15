using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashfloodSegmentation
{
    class Section
    {
        public int[,] operImg;
        public int id;
        public static double threshold = 25;
        public static double edgeThreshold = 8;
        public SectionMaster master;
        public Section ownerSection;
        public List<Point> points;
        public Point basePoint;
        public int? origSection;
        public List<int> absorbees;
        public List<NeighborRecord> oldNeighborRecs;
        public Dictionary<int, HashSet<Point>> neighborRecs;


        public Section(SectionMaster m, Point bPoint)
        {
            ownerSection = this;
            points = new List<Point>();
            master = m;
            absorbees = new List<int>();
            id = master.addSection(this);
            operImg = master.operImg;
            operImg[bPoint.X, bPoint.Y] = id;
            basePoint = bPoint;
            points.Add(bPoint);
            origSection = null;
            neighborRecs = new Dictionary<int, HashSet<Point>>();
            oldNeighborRecs = new List<NeighborRecord>();
        }



        public Section getOwnerSection()
        {
            if (ownerSection == this)
            {
                return this;
            }
            else
            {
                return ownerSection.getOwnerSection();
            }
        }

        //tries to liquidate section by center color dist, returns whether succeeded
        public bool tryLiquidate(bool required) 
        {
            Section closestNeighbor = null;
            double tempColorDist;
            Section s;
            double minColorDist = double.MaxValue;
            ICollection<int> keys = neighborRecs.Keys;
            foreach (int i in keys)
            {
                s = master.getSectionById(i);
                tempColorDist = calcCenterDist(s);
                if (minColorDist > tempColorDist && this != s)
                {
                    closestNeighbor = s;
                    minColorDist = tempColorDist;
                }
            }
            if (required || minColorDist < threshold)
            {
                closestNeighbor.absorb(this, false);
                return true;
            }
            return false;

        }

        private double calcCenterDist(Section s)
        {
            return comparePoints(basePoint, s.basePoint);
        }

        //tries to dissolve section by avg color dist, returns whether succeeded
        public bool tryDissolve() 
        {
            Section closestNeighbor = null;
            double tempColorDist;
            Section s;
            double minColorDist = double.MaxValue;
            ICollection<int> keys = neighborRecs.Keys;
            foreach (int i in keys)
            {
                s = master.getSectionById(i);
                tempColorDist = calcEdgeDist(s);
                if (minColorDist > tempColorDist && this != s)
                {
                    closestNeighbor = s;
                    minColorDist = tempColorDist;
                }
            }

            if (minColorDist < edgeThreshold)
            {
                closestNeighbor.absorb(this, true);
                return true;
            }
            return false;
        }

        public void cleanupNeighborRecs()
        {
            ICollection<int> keys = neighborRecs.Keys;
            Dictionary<int, HashSet<Point>> d = new Dictionary<int, HashSet<Point>>();
            int newID;
            List<int> keysToRemove = new List<int>();
            foreach (int i in keys)
            {
                newID = master.getSectionById(i).id;
                if (newID != i)
                {
                    if (neighborRecs.ContainsKey(newID))
                    {
                        neighborRecs[newID].UnionWith(neighborRecs[i]);
                    }
                    else
                    {
                        if (d.ContainsKey(newID))
                        {
                            d[newID].UnionWith(neighborRecs[i]);
                        }
                        else
                        {
                            d.Add(newID, neighborRecs[i]);
                        }

                    }

                    keysToRemove.Add(i);
                }
            }
            ICollection<int> otherkeys = d.Keys;
            foreach (int i in keysToRemove)
            {
                neighborRecs.Remove(i);
            }
            d.ToList().ForEach(elt =>
            {
                if (neighborRecs.ContainsKey(elt.Key))
                {
                    neighborRecs[elt.Key].UnionWith(elt.Value);
                }
                else
                {
                    neighborRecs.Add(elt.Key, elt.Value);
                }
            });

        }

        private double calcEdgeDist(Section s)
        {
            HashSet<Point> yours, mine, e1, e2;
            yours = s.neighborRecs[id];
            bool x = absorbees.Contains(1);
            List<int> ab = s.absorbees;
            List<int> stuff = ab.Intersect(neighborRecs.Keys).ToList();

            mine = neighborRecs[s.id];
            if (yours.Count < mine.Count)
            {
                e1 = yours;
                e2 = mine;
            }
            else
            {
                e2 = yours;
                e1 = mine;
            }
            Dictionary<int, HashSet<Point>> e2Ref = sortByX(e2);
            double sum = 0, count = 0;
            foreach (Point p in e1)
            {
                if (e2Ref.ContainsKey(p.X - 1))
                {
                    compareToAll(p, e2Ref[p.X - 1], ref sum, ref count);
                }
                if (e2Ref.ContainsKey(p.X))
                {
                    compareToAll(p, e2Ref[p.X], ref sum, ref count);
                }
                if (e2Ref.ContainsKey(p.X + 1))
                {
                    compareToAll(p, e2Ref[p.X + 1], ref sum, ref count);
                }
            }
            return sum / count;

        }

        private void compareToAll(Point p, HashSet<Point> points, ref double sum, ref double count)
        {
            foreach (Point p1 in points)
            {
                if (Math.Abs(p.Y - p1.Y) <= 1)
                {
                    count++;
                    sum += comparePoints(p, p1);
                }
            }
        }

        private Dictionary<int, HashSet<Point>> sortByX(HashSet<Point> h)
        {
            Dictionary<int, HashSet<Point>> d = new Dictionary<int, HashSet<Point>>();
            foreach (Point p in h)
            {
                if (d.ContainsKey(p.X))
                {
                    d[p.X].Add(p);
                }
                else
                {
                    HashSet<Point> h1 = new HashSet<Point>();
                    h1.Add(p);
                    d.Add(p.X, h1);
                }
            }
            return d;
        }

        private void absorb(Section defector, bool update)
        {
            if (defector.id == id)
            {
                throw new Exception();
            }
            List<Point> otherPoints = defector.points;
            otherPoints.ForEach(x => { operImg[x.X, x.Y] = id;});
            points.AddRange(otherPoints);
            defector.neighborRecs.ToList().ForEach(x =>
            {
                if (neighborRecs.ContainsKey(x.Key))
                {
                    neighborRecs[x.Key].UnionWith(x.Value);
                }
                else if (x.Key != id)
                {
                    neighborRecs.Add(x.Key, x.Value);
                }
            }
            );
            neighborRecs.Remove(id);
            neighborRecs.Remove(defector.id);
            defector.ownerSection = this;
            absorbees.Add(defector.id);

            if (update)
            {
                ICollection<int> keys = defector.neighborRecs.Keys.ToList();
                Section s;
                foreach (int i in keys)
                {
                    s = master.getSectionById(i);
                    if (s.neighborRecs.ContainsKey(id))
                    {
                        s.neighborRecs[id].UnionWith(s.neighborRecs[defector.id]);
                    }
                    else if (s != this)
                    {
                        s.neighborRecs.Add(id, s.neighborRecs[defector.id]);
                    }
                    s.neighborRecs.Remove(defector.id);
                }
            }
            master.sections.Remove(defector);
        }

        public void recruit(Point startPoint)
        {
            if (operImg[startPoint.X, startPoint.Y] == 0)
            {
                operImg[startPoint.X, startPoint.Y] = id;
                points.Add(startPoint);
            }

            Helper h;
            Point testPoint = new Point();
            for (int i = 0; i < points.Count; i++)
            {
                h = new Helper(points[i], false, operImg.GetLength(0), operImg.GetLength(1));
                while (h.keepGoing())
                {
                    testPoint = h.getNextPoint();
                    if (operImg[testPoint.X, testPoint.Y] == 0 && comparePoints(startPoint, testPoint) < threshold)
                    {
                        operImg[testPoint.X, testPoint.Y] = id;
                        points.Add(testPoint);
                    }
                    else if (operImg[testPoint.X, testPoint.Y] != id)
                    {
                        addNeighborRecord(testPoint);
                    }
                }
            }
            if (points.Count < SectionMaster.tinyThreshold)
            {
                master.tinyList.Add(this);
            }
        }

        private void addNeighborRecord(Point p)
        {
            int i = operImg[p.X, p.Y];
            HashSet<Point> h;

            if (!neighborRecs.ContainsKey(i))
            {
                h = new HashSet<Point>();
                neighborRecs.Add(i, h);
            }
            else
            {
                h = neighborRecs[i];
            }
            h.Add(p);
        }

        public HashSet<Section> getNeighbors()
        {
            HashSet<Section> h = new HashSet<Section>();
            ICollection<int> keys = neighborRecs.Keys;
            foreach (int i in keys)
            {
                h.Add(master.getSectionById(i));
            }
            return h;
        }

        public void distributeUnassignedNeighborRecords()
        {
            HashSet<Point> h;
            if (neighborRecs.TryGetValue(0, out h))
            {
                foreach (Point p in h)
                {
                    addNeighborRecord(p);
                }
                neighborRecs.Remove(0);
            }

        }

        private bool validPoint(Point testPoint)
        {
            return (testPoint.X >= 0 && testPoint.Y >= 0 && testPoint.X < operImg.GetLength(0) 
                && testPoint.Y < operImg.GetLength(1));
        }

        public static double comparePoints(Point a, Point b)
        {
            Color c1 = SectionMaster.actualImg[a.X, a.Y];
            Color c2 = SectionMaster.actualImg[b.X, b.Y];
            int Rdiff = c1.R - c2.R;
            int Gdiff = c1.G - c2.G;
            int Bdiff = c1.B - c2.B;
            return Math.Sqrt(Rdiff * Rdiff + Gdiff * Gdiff + Bdiff * Bdiff);
        }

        private void Add(Point a)
        {
            operImg[a.X, a.Y] = id;
            points.Add(a);

        }





        private int[,] copyOperImg()
        {
            int[,] a = new int[operImg.GetLength(0), operImg.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    a[i, j] = operImg[i, j];
                }
            }
            return a;
        }





        override public bool Equals(Object o)
        {
            if (o == null || o.GetType() != typeof(Section))
            {
                return false;
            }
            Section s = (Section)o;
            return id == s.id;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}

