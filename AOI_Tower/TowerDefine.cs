using System;
using System.Collections.Generic;
using System.Collections;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOI_Tower
{
    public struct aoiNode_s
    {
        public Int32 m_iSlotIndex;
    }

    public struct tower_s
    {
        public Dictionary<int, aoiNode_s> m_Watcher;
        public Dictionary<int, aoiNode_s> m_Marker;
        public Int32 m_iFirstChildId;
        public Int32 m_iMarkerCount;
    }

    public struct aoiObj_s
    {
        public Int32 m_iSlotIndex;
        public UInt64 m_iMode;
        public UInt64 m_uiMask;
        public UInt64 m_uiUserId;
        public float m_fViewRadius;
        public float[] m_fLast;
        public float[] m_fPos;
    }

    public delegate void callback(bool bAddTo, UInt64 watcherId, UInt64 markerId);
    public struct towerSpace_s
    {
        public callback m_CallBack;

        public float[] m_fMin;
        public float[] m_fGridLength;

        public float m_fMovefRange;
        public Int32 m_iSplitThreshold;
        public Int32 m_iMaxWidth;
        public Int32 m_iMaxHeight;

        public Int32 [] m_pGrids;

        public tower_s [] m_pTowers;
        public Int32 m_iTowerNext;
        public Int32 m_iTowerCapacity;

        public aoiObj_s[] m_pSlotObj;
        public Int32 m_iSlotIndex;
        public Int32 m_iSlotCapacity;

    }

}
