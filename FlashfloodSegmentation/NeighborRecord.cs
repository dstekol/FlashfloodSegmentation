using System.Collections.Generic;
using System.Drawing;
using System;

namespace FlashfloodSegmentation
{
    //distinct NeighborRecord exists for each pair of neighboring sections
    class NeighborRecord
    {
        public Dictionary<int, HashSet<Point>> neighborMap;
        public static HashSet<NeighborRecord> allneighborRecs = new HashSet<NeighborRecord>();
        SectionMaster master;


        public NeighborRecord(int a, int b, SectionMaster m)
        {
            neighborMap = new Dictionary<int, HashSet<Point>>();
            neighborMap.Add(a, new HashSet<Point>());
            neighborMap.Add(b, new HashSet<Point>());
            master = m;
        }
    }
}

