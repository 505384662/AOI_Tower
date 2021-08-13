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
        static void Main(string[] args)
        {
            var tower = new TowerProcess();
            tower.Start(m_gridLength, 0, 0, 50*50, 50*50, m_splitThresShold);
            DateTime dt = DateTime.Now;
            for(UInt64 i = 0; i < m_ObjCount; i++)
            {
                tower.AddObj(i, new float[]{ 930, 12720, 320}, m_uiMask, false, true, m_viewRadius);
                tower.UpdateObjPos(i, new float[] { 930, 930, 320 });
                tower.UpdateObjPos(i, new float[] { 1000, 1000, 320 });
                //tower.RemoveObj(i);
            }
           

            Console.WriteLine(string.Format("灯塔数:{0:G} 人数:{1:G} 花费时间{2:G}毫秒",tower.GetTowerCount(),m_ObjCount,(DateTime.Now - dt).TotalMilliseconds));
        }
    }
}
