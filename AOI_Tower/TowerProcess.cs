using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOI_Tower
{
    class TowerProcess
    {
        private towerSpace_s m_pTowerSpace;

        private Dictionary<UInt64, Int32> m_AoiObjDic = new Dictionary<ulong, int>();

        private Dictionary<UInt64, Dictionary<UInt64, bool>> m_ListViewObj = new Dictionary<ulong, Dictionary<UInt64, bool>>();

        public Int32 GetTowerCount()
        {
            return m_pTowerSpace.m_iTowerNext;
        }

        private void AoiCallBack(bool bAddTo, UInt64 watcherId, UInt64 markerId)
        {
            Dictionary<UInt64, bool> markerDic;
            if (!m_ListViewObj.TryGetValue(watcherId, out markerDic))
            {
                markerDic = new Dictionary<ulong, bool>();
                m_ListViewObj[watcherId] = markerDic;
            }

            markerDic[markerId] = bAddTo;
        }

        public bool Start(float fGridLength, float fMinX, float fMinZ, float fMaxWidth, float fMaxHeigth, Int32 iSplitThreshold)
        {
            m_pTowerSpace = GlobalTowerSpace.CreateTowerSpace(fGridLength, fMinX, fMinZ, fMaxWidth, fMaxHeigth, iSplitThreshold);
            GlobalTowerSpace.TowerSpace_SetCallback(ref m_pTowerSpace, AoiCallBack);

            return true;
        }

        public void Stop()
        {
            m_AoiObjDic.Clear();
            m_ListViewObj.Clear();
        }


        public bool AddObj(UInt64 uiUserId, float PosX, float PosY, float PosZ, UInt64 uiMask, bool bMarKer, bool bWatcher, UInt32 viewRadius)
        {
            if (m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            var iSlotIndex = GlobalTowerSpace.TowerSpace_AddAoiObj(ref m_pTowerSpace, uiUserId, PosX, PosY, PosZ, uiMask);
            if (-1 == iSlotIndex)
            {
                return false;
            }

            m_AoiObjDic[uiUserId] = iSlotIndex;

            if (bMarKer)
            {
                GlobalTowerSpace.TowerSpace_AddAoiObjMarker(ref m_pTowerSpace, iSlotIndex);
            }

            if (bWatcher)
            {
                GlobalTowerSpace.TowerSpace_AddAoiObjWatcher(ref m_pTowerSpace, iSlotIndex, viewRadius);
            }

            return true;
        }

        public bool RemoveObj(UInt64 uiUserId)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            GlobalTowerSpace.TowerSpace_RemoveAoiObj(ref m_pTowerSpace, m_AoiObjDic[uiUserId]);
            return true;
        }


        public bool UpdateObjMask(UInt64 uiUserId, UInt64 uiMask)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            GlobalTowerSpace.TowerSpace_UpdateAoiObjMask(ref m_pTowerSpace, m_AoiObjDic[uiUserId], uiMask);
            return true;
        }


        public bool UpdateObjPos(UInt64 uiUserId, float PosX, float PosY, float PosZ)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            if (!GlobalTowerSpace.TowerSpace_UpdateAoiObjPos(ref m_pTowerSpace, m_AoiObjDic[uiUserId], PosX, PosY, PosZ))
            {
                return false;
            }

            return true;
        }
        
        public bool AddObjWatcher(UInt64 uiUserId, UInt32 viewRadius)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            if (!GlobalTowerSpace.TowerSpace_AddAoiObjWatcher(ref m_pTowerSpace, m_AoiObjDic[uiUserId], viewRadius))
            {
                return false;
            }

            return true;
        }

        public bool RemoveObjWatcher(UInt64 uiUserId)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            if (!GlobalTowerSpace.TowerSpace_RemoveAoiObjWatcher(ref m_pTowerSpace, m_AoiObjDic[uiUserId]))
            {
                return false;
            }

            return true;
        }

        public bool AddObjMarker(UInt64 uiUserId)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            if (!GlobalTowerSpace.TowerSpace_AddAoiObjMarker(ref m_pTowerSpace, m_AoiObjDic[uiUserId]))
            {
                return false;
            }

            return true;
        }

        public bool RemoveObjMarker(UInt64 uiUserId)
        {
            if (!m_AoiObjDic.ContainsKey(uiUserId))
            {
                return false;
            }

            if (!GlobalTowerSpace.TowerSpace_RemoveAoiObjMarker(ref m_pTowerSpace, m_AoiObjDic[uiUserId]))
            {
                return false;
            }

            return true;
        }
    }
}
