using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashfloodSegmentation
{
    //class for managing all sections the image has been divided into
    class SectionMaster
    {
        public static Color[,] actualImg; //color array of image
        public int[,] operImg; //map of which pixel belongs to which section
        public List<Section> sections; //list of currently existing sections
        public int sectionTracker = 0; //for tracking section # and assigning ids
        public List<Section> sectionRef; //list of all sections (even ones that self-liquidated)
        static public int tinyThreshold = 200; // size threshold for section to automatically self-liquidate
        public List<Section> tinyList;
        public static List<string> logStuff = new List<string>();

        public SectionMaster(Color[,] aImg)
        {
            actualImg = aImg;
            operImg = new int[aImg.GetLength(0), aImg.GetLength(1)];
            sections = new List<Section>();
            sectionRef = new List<Section>();
            tinyList = new List<Section>();
        }

        public Section getSectionById(int i)
        {
            return sectionRef[i - 1].getOwnerSection();
        }

        //adds section to section list, returns unique id for that section
        public int addSection(Section s) 
        {
            sections.Add(s);
            sectionRef.Add(s);
            return ++sectionTracker;
        }

        public void liquidateTinies()
        {

            foreach (Section s in tinyList)
            {
                if (s.points.Count < tinyThreshold)
                {
                    s.tryLiquidate(true);
                }
            }
        }

        public void tryLiquidateAll()
        {
            for (int i = sections.Count - 1; i >= 0; i--)
            {
                sections[i].tryLiquidate(false);
            }
        }

        public void tryDissolveAll()
        {
            for (int i = sections.Count - 1; i >= 0; i--)
            {
                sections[i].tryDissolve();
            }
        }


        public void divide()
        {

            Point testpoint = new Point();
            for (int x = 0; x < operImg.GetLength(0); x++)
            {
                for (int y = 0; y < operImg.GetLength(1); y++) //loop through section map
                {
                    if (operImg[x, y] == 0)
                    {
                        testpoint = new Point(x, y);
                        (new Section(this, testpoint)).recruit(testpoint); //create a new section and have it collect points around it
                    }
                }
            }

            neighborSearch();
            liquidateTinies();
            cleanupAllNeighborRecs();
            tryDissolveAll();

        }

        private void checkNeighborRecs()
        {
            foreach (Section s in sections)
            {
                ICollection<int> keys = s.neighborRecs.Keys.ToList();
                foreach (int k in keys)
                {
                    if (!getSectionById(k).neighborRecs.ContainsKey(s.id))
                    {
                        throw new FormatException();
                    }
                }
            }
        }

        private void cleanupAllNeighborRecs()
        {
            foreach (Section s in sections)
            {
                s.cleanupNeighborRecs();
            }
        }

        public Bitmap colorSections()
        {
            Bitmap bmap = new Bitmap(operImg.GetLength(0), operImg.GetLength(1));
            Color[] colorList = new Color[sectionRef.Count + 1];
            Random rand = new Random();

            for (int i = 0; i < colorList.Length; i++)
            {
                colorList[i] = Color.FromArgb(255, rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
            }

            for (int x = 0; x < operImg.GetLength(0); x++)
            {
                for (int y = 0; y < operImg.GetLength(1); y++)
                {
                    bmap.SetPixel(x, y, colorList[operImg[x, y]]);
                }
            }
            return bmap;
        }



        public Bitmap colorOneSection(int sID)
        {
            Bitmap bmap = new Bitmap(operImg.GetLength(0), operImg.GetLength(1));
            Random rand = new Random();
            Color sectionColor = Color.FromArgb(255, rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
            Color white = Color.FromArgb(255, 255, 255, 255);

            for (int x = 0; x < operImg.GetLength(0); x++)
            {
                for (int y = 0; y < operImg.GetLength(1); y++)
                {
                    if (operImg[x, y] == sID)
                    {
                        bmap.SetPixel(x, y, sectionColor);
                    }
                    else
                    {
                        bmap.SetPixel(x, y, white);
                    }

                }
            }
            return bmap;
        }

        public int getNumDistinctSections()
        {
            List<int> encountered = new List<int>();
            for (int x = 0; x < operImg.GetLength(0); x++)
            {
                for (int y = 0; y < operImg.GetLength(1); y++)
                {
                    if (!encountered.Contains(operImg[x, y]))
                    {
                        encountered.Add(operImg[x, y]);
                    }
                }
            }
            return encountered.Count;
        }

        //goes through section map, finds all neighbor pairs
        public void neighborSearch() 
        {
            foreach (Section s in sections)
            {
                s.distributeUnassignedNeighborRecords();
            }
        }
    }
}