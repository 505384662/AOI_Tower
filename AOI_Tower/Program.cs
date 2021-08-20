using System;

namespace AOI_Tower
{
    class Program
    {
        public const UInt64 m_uiMask = 0xffffffffffffffff;
        public const int m_gridLength = 500;    //  单位厘米
        public const int m_splitThresShold = 32;    //一个四叉树格子最大容量
        public const int m_viewRadius = 150;

        public const int m_ObjCount = 10000;
        static void Main()
        {
            var tower = new TowerProcess();
            tower.Start(m_gridLength, 0, 0, 50 * 50, 50 * 50, m_splitThresShold);
            DateTime dt = DateTime.Now;
            for (UInt64 i = 0; i < m_ObjCount; i++)
            {
                tower.AddObj(i,  930, 12720, 320 , m_uiMask, true, true, m_viewRadius);
                //tower.RemoveObj(i);
            }

            while (true)
            {
                for (UInt64 i = 0; i < m_ObjCount; i++)
                {
                    tower.UpdateObjPos(i, 930 + 20, 930, 320 + 30);
                   // Console.WriteLine(string.Format("角色id:{0:G},位置X:{1:G},位置Y:{2:G},位置Z:{3:G}", i, x, y, z));
  
                   tower.UpdateObjPos(i, 1000 + 50, 1000, 320 + 60);
                   // Console.WriteLine(string.Format("角色id:{0:G},位置X:{1:G},位置Y:{2:G},位置Z:{3:G}", i, x, y, z));
                }
            }
        }
    }
}
