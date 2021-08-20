using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOI_Tower;
using System.Runtime.CompilerServices;

namespace AOI_Tower
{
    public static class GlobalTowerSpace
    {
        public const UInt64 WATCHER_MODE = 0x01;
        public const UInt64 MARKER_MODE = 0x02;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static float CalculateDistance(float PosX1, float PosY1, float PosZ1, float PosX2, float PosY2, float PosZ2)
        {
            return (PosX1 - PosX2) * (PosX1 - PosX2) + (PosZ1 - PosZ2) * (PosZ1 - PosZ2);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static bool IsMoveNear(float PosX1, float PosY1, float PosZ1, float PosX2, float PosY2, float PosZ2, float fMoveRadis2)
        {
            return CalculateDistance(PosX1, PosY1, PosZ1, PosX2, PosY2, PosZ2) < fMoveRadis2;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInRect(Int32 x, Int32 y, Int32 minX, Int32 minY, Int32 maxX, Int32 maxY)
        {
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInInside(Int32 inMinX, Int32 inMinY, Int32 inMaxX, Int32 inMaxY, Int32 minX, Int32 minY, Int32 maxX, Int32 maxY)
        {
            return inMinX >= minX && inMaxX <= maxX && inMinY >= minY && inMaxY <= maxY;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static bool IsOverlap(Int32 inMinX, Int32 inMinY, Int32 inMaxX, Int32 inMaxY, Int32 minX, Int32 minY, Int32 maxX, Int32 maxY)
        {
            bool overlap = true;
            overlap = (inMinX > maxX || inMaxX < minX) ? false : overlap;
            overlap = (inMinY > maxY || inMaxY < minY) ? false : overlap;
            return overlap;
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void CalcGridLoc(ref towerSpace_s pTowerSpace, float PosX, float PosY, float PosZ, out Int32 tx, out Int32 ty)
        {
            tx = Convert.ToInt32(Math.Floor((PosX - pTowerSpace.m_fMin[0]) / pTowerSpace.m_fGridLength[0]));
            ty = Convert.ToInt32(Math.Floor((PosZ - pTowerSpace.m_fMin[1]) / pTowerSpace.m_fGridLength[0]));
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void CalcMinGridLoc(ref towerSpace_s pTowerSpace, Int32 iLod, float PosX, float PosY, float PosZ, out Int32 tx, out Int32 ty)
        {
            tx = Convert.ToInt32(Math.Floor((PosX - pTowerSpace.m_fMin[0]) / pTowerSpace.m_fGridLength[iLod]));
            ty = Convert.ToInt32(Math.Floor((PosZ - pTowerSpace.m_fMin[1]) / pTowerSpace.m_fGridLength[iLod]));
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void CalcMaxGridLoc(ref towerSpace_s pTowerSpace, Int32 iLod, float PosX, float PosY, float PosZ, out Int32 tx, out Int32 ty)
        {
            tx = Convert.ToInt32(Math.Ceiling((PosX - pTowerSpace.m_fMin[0]) / pTowerSpace.m_fGridLength[iLod]));
            ty = Convert.ToInt32(Math.Ceiling((PosZ - pTowerSpace.m_fMin[1]) / pTowerSpace.m_fGridLength[iLod]));
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void TowerCallback(ref towerSpace_s pTowerSpace, bool bAddTo, ref aoiObj_s pObj, ref Dictionary<int, aoiNode_s> aoiNodeDic)
        {
            foreach (var aoiNode in aoiNodeDic)
            {
                ref var pAoiNodeObj = ref pTowerSpace.m_pSlotObj[aoiNode.Value.m_iSlotIndex];
                if ((pAoiNodeObj.m_iSlotIndex != pObj.m_iSlotIndex) && 0 != (pObj.m_uiMask & pAoiNodeObj.m_uiMask))
                {
                    pTowerSpace.m_CallBack(bAddTo, pObj.m_uiUserId, pAoiNodeObj.m_uiUserId);
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static ref tower_s CreateGridTower(ref towerSpace_s pTowerSpace, Int32 iX, Int32 iY)
        {
            Int32 iTowerId = pTowerSpace.m_iTowerNext;
            if (iTowerId >= pTowerSpace.m_iTowerCapacity)
            {
                Int32 oldTowerCapacity = pTowerSpace.m_iTowerCapacity;
                pTowerSpace.m_iTowerCapacity *= 2;
                Array.Resize(ref pTowerSpace.m_pTowers, pTowerSpace.m_iTowerCapacity);
                for (int i = oldTowerCapacity; i < pTowerSpace.m_iTowerCapacity; i++)
                {
                    pTowerSpace.m_pTowers[i].m_Watcher = new Dictionary<int, aoiNode_s>();
                    pTowerSpace.m_pTowers[i].m_Marker = new Dictionary<int, aoiNode_s>();
                    pTowerSpace.m_pTowers[i].m_iFirstChildId = -1;
                    pTowerSpace.m_pTowers[i].m_iMarkerCount = 0;
                }
            }

            pTowerSpace.m_iTowerNext++;
            pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth] = iTowerId;

            return ref pTowerSpace.m_pTowers[iTowerId];
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void ClearGridTower(ref towerSpace_s pTowerSpace, Int32 iTowerId)
        {
            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];

            pTower.m_Watcher.Clear();
            pTower.m_Marker.Clear();

            if (-1 != pTower.m_iFirstChildId)
            {
                for (int i = 0; i < 4; i++)
                {
                    ClearGridTower(ref pTowerSpace, pTower.m_iFirstChildId + i);
                }
            }
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static Int32 InsertChildTower(ref towerSpace_s pTowerSpace, Int32 iTowerId)
        {
            if (pTowerSpace.m_iTowerNext + 3 >= pTowerSpace.m_iTowerCapacity)
            {
                pTowerSpace.m_iTowerCapacity *= 2;
                Array.Resize(ref pTowerSpace.m_pTowers, pTowerSpace.m_iTowerCapacity);
            }

            Int32 m_iFirstChildId = pTowerSpace.m_iTowerNext;
            pTowerSpace.m_iTowerNext += 4;

            ref var pParentTower = ref pTowerSpace.m_pTowers[iTowerId];
            pParentTower.m_iFirstChildId = m_iFirstChildId;

            for (Int32 i = 0; i < 4; ++i)
            {
                ref var pTower = ref pTowerSpace.m_pTowers[m_iFirstChildId + i];
                pTower.m_Watcher = new Dictionary<int, aoiNode_s>();
                pTower.m_Marker = new Dictionary<int, aoiNode_s>();
                pTower.m_iFirstChildId = -1;
                pTower.m_iMarkerCount = 0;
            }

            return m_iFirstChildId;
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void GenGridSplit(ref towerSpace_s pTowerSpace, Int32 iTowerId, Int32 iLod, Int32 iX, Int32 iY)
        {
            Int32 m_iFirstChildId = InsertChildTower(ref pTowerSpace, iTowerId);

            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
            pTower.m_iMarkerCount = 0;

            foreach (var maker in pTower.m_Marker.ToList())
            {
                pTower.m_Marker.Remove(maker.Key);
                ref var pObj = ref pTowerSpace.m_pSlotObj[maker.Value.m_iSlotIndex];

                Int32 iLodX;
                Int32 iLodY;
                CalcMinGridLoc(ref pTowerSpace, iLod, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLodX, out iLodY);

                var pLodTower = pTowerSpace.m_pTowers[pTower.m_iFirstChildId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2];
                pLodTower.m_Marker.Add(maker.Key,maker.Value);
                pLodTower.m_iMarkerCount++;
            }

            Int32 minGridX = iX * 2;
            Int32 minGridY = iY * 2;
            Int32 maxGridX = iX * 2 + 1;
            Int32 maxGridY = iY * 2 + 1;

            foreach (var watcher in pTower.m_Watcher.ToList())
            {
                ref var pObj = ref pTowerSpace.m_pSlotObj[watcher.Value.m_iSlotIndex];

                float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
                float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
                float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
                float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

                int miny;
                int minx;
                int maxx;
                int maxy;
                CalcMinGridLoc(ref pTowerSpace, iLod, bminX, 0, bminZ, out minx, out miny);
                CalcMaxGridLoc(ref pTowerSpace, iLod, bmaxX, 0, bmaxZ, out maxx, out maxy);

                if (!IsInInside(minGridX, minGridY, maxGridX, maxGridY, minx, miny, maxx, maxy))
                {
                    for (Int32 y = 0; y < 2; y++)
                    {
                        for (Int32 x = 0; x < 2; x++)
                        {
                            ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + y * 2 + x];
                            if (IsInRect(minGridX + x, minGridY + y, minx, miny, maxx, maxy))
                            {
                                pLodTower.m_Watcher.Add(watcher.Value.m_iSlotIndex, new aoiNode_s() { m_iSlotIndex = pObj.m_iSlotIndex });
                                pTower.m_Watcher.Remove(watcher.Value.m_iSlotIndex);
                            }
                            else
                            {
                                TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Marker);
                            }
                        }
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void InsertLodMarker(ref towerSpace_s pTowerSpace, Int32 iTowerId, aoiNode_s pNode, aoiObj_s pObj, Int32 iX, Int32 iY, Int32 iLodX, Int32 iLodY, float PosX, float PosY, float PosZ)
        {
            Int32 iTowerLodId = iTowerId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;

            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            Int32 iLodFirstChildId = pLodTower.m_iFirstChildId;
            if (iLodFirstChildId == -1)
            {
                if (pLodTower.m_iMarkerCount >= pTowerSpace.m_iSplitThreshold)
                {
                    GenGridSplit(ref pTowerSpace, iTowerLodId, 2, iLodX, iLodY);
                    pLodTower = pTowerSpace.m_pTowers[iTowerLodId];

                    Int32 iLod2X1;
                    Int32 iLod2Y1;
                    CalcMinGridLoc(ref pTowerSpace, 2, PosX, PosY, PosZ, out iLod2X1, out iLod2Y1);
                    ref var pLod2Tower1 = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + (iLod2X1 - iLodX * 2) + (iLod2Y1 - iLodY * 2) * 2];
                    pLod2Tower1.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                    pLod2Tower1.m_iMarkerCount++;

                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower1.m_Watcher);
                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);

                    return;
                }

                pLodTower.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                pLodTower.m_iMarkerCount++;

                TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);

                return;
            }

            Int32 iLod2X2;
            Int32 iLod2Y2;
            CalcMinGridLoc(ref pTowerSpace, 2, PosX, PosY, PosZ, out iLod2X2, out iLod2Y2);
            ref var pLod2Tower2 = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + (iLod2X2 - iLodX * 2) + (iLod2Y2 - iLodY * 2) * 2];
            pLod2Tower2.m_Marker.Add(pNode.m_iSlotIndex, pNode);
            pLod2Tower2.m_iMarkerCount++;

            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower2.m_Watcher);
            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static aoiNode_s RemoveLodMarker(ref towerSpace_s pTowerSpace, Int32 iTowerId, ref aoiObj_s pObj, Int32 iX, Int32 iY, Int32 iLodX, Int32 iLodY)
        {
            Int32 iTowerLodId = iTowerId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;
            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            if (pLodTower.m_iFirstChildId == -1)
            {
                TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Watcher);

                aoiNode_s retNode1;
                pLodTower.m_Marker.Remove(pObj.m_iSlotIndex, out retNode1);
                pLodTower.m_iMarkerCount--;
                return retNode1;
            }

            Int32 iLod2X;
            Int32 iLod2Y;
            CalcMinGridLoc(ref pTowerSpace, 2, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLod2X, out iLod2Y);
            Int32 iTowerLod2Id = pLodTower.m_iFirstChildId + (iLod2X - iLodX * 2) + (iLod2Y - iLodY * 2) * 2;
            ref var pLod2Tower = ref pTowerSpace.m_pTowers[iTowerLod2Id];

            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Watcher);
            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Watcher);

            aoiNode_s retNode2;
            pLod2Tower.m_Marker.Remove(pObj.m_iSlotIndex, out retNode2);
            pLod2Tower.m_iMarkerCount--;
            return retNode2;
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void InsertGridMarker(ref towerSpace_s pTowerSpace, aoiNode_s pNode, Int32 iX, Int32 iY, float PosX, float PosY, float PosZ)
        {
            Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];
            if (iTowerId == -1)
            {
                ref var pCreateTower = ref CreateGridTower(ref pTowerSpace, iX, iY);
                pCreateTower.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                pCreateTower.m_iMarkerCount++;
                return;
            }

            ref var pObj = ref pTowerSpace.m_pSlotObj[pNode.m_iSlotIndex];

            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
            Int32 m_iFirstChildId = pTower.m_iFirstChildId;
            if (m_iFirstChildId == -1)
            {
                if (pTower.m_iMarkerCount >= pTowerSpace.m_iSplitThreshold)
                {
                    GenGridSplit(ref pTowerSpace, iTowerId, 1, iX, iY);
                    pTower = pTowerSpace.m_pTowers[iTowerId];

                    Int32 iLodX1;
                    Int32 iLodY1;
                    CalcMinGridLoc(ref pTowerSpace, 1, PosX, PosY, PosZ, out iLodX1, out iLodY1);

                    ref var pLodTower1 = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + (iLodX1 - iX * 2) + (iLodY1 - iY * 2) * 2];
                    pLodTower1.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                    pLodTower1.m_iMarkerCount++;

                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower1.m_Watcher);
                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower.m_Watcher);
                    return;
                }

                pTower.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                pTower.m_iMarkerCount++;
                TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower.m_Watcher);
                return;
            }

            Int32 iLodX2;
            Int32 iLodY2;
            CalcMinGridLoc(ref pTowerSpace, 1, PosX, PosY, PosZ, out iLodX2, out iLodY2);
            Int32 iTowerLodId = m_iFirstChildId + (iLodX2 - iX * 2) + (iLodY2 - iY * 2) * 2;

            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            Int32 iLodFirstChildId = pLodTower.m_iFirstChildId;
            if (iLodFirstChildId == -1)
            {
                if (pLodTower.m_iMarkerCount >= pTowerSpace.m_iSplitThreshold)
                {
                    GenGridSplit(ref pTowerSpace, iTowerLodId, 2, iLodX2, iLodY2);
                    pTower = pTowerSpace.m_pTowers[iTowerId];
                    pLodTower = pTowerSpace.m_pTowers[iTowerLodId];

                    Int32 iLod2X1;
                    Int32 iLod2Y1;
                    CalcMinGridLoc(ref pTowerSpace, 2, PosX, PosY, PosZ, out iLod2X1, out iLod2Y1);
                    var pLod2Tower1 = pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + (iLod2X1 - iLodX2 * 2) + (iLod2Y1 - iLodY2 * 2) * 2];

                    pLod2Tower1.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                    pLod2Tower1.m_iMarkerCount++;

                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower1.m_Watcher);
                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);
                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower.m_Watcher);
                    return;
                }

                pLodTower.m_Marker.Add(pNode.m_iSlotIndex, pNode);
                pLodTower.m_iMarkerCount++;

                TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);
                TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower.m_Watcher);
                return;
            }

            Int32 iLod2X2;
            Int32 iLod2Y2;
            CalcMinGridLoc(ref pTowerSpace, 2, PosX, PosY, PosZ, out iLod2X2, out iLod2Y2);
            ref var pLod2Tower2 = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + (iLod2X2 - iLodX2 * 2) + (iLod2Y2 - iLodY2 * 2) * 2];
            pLod2Tower2.m_Marker.Add(pNode.m_iSlotIndex, pNode);
            pLod2Tower2.m_iMarkerCount++;
            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower2.m_Watcher);
            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);
            TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower.m_Watcher);
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static aoiNode_s RemoveGridMarker(ref towerSpace_s pTowerSpace, ref aoiObj_s pObj, Int32 iX, Int32 iY)
        {
            Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];
            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
            if (pTower.m_iFirstChildId == -1)
            {

                TowerCallback(ref pTowerSpace, false, ref pObj, ref pTower.m_Watcher);

                aoiNode_s retNode;
                pTower.m_Marker.Remove(pObj.m_iSlotIndex, out retNode);
                pTower.m_iMarkerCount--;
                return retNode;
            }

            Int32 iLodX;
            Int32 iLodY;
            CalcMinGridLoc(ref pTowerSpace, 1, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLodX, out iLodY);
            Int32 iTowerLodId = pTower.m_iFirstChildId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;
            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            if (pLodTower.m_iFirstChildId == -1)
            {
                TowerCallback(ref pTowerSpace, false, ref pObj, ref pTower.m_Watcher);
                TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Watcher);

                aoiNode_s retNode1;
                pLodTower.m_Marker.Remove(pObj.m_iSlotIndex, out retNode1);
                pLodTower.m_iMarkerCount--;
                return retNode1;
            }

            Int32 iLod2X;
            Int32 iLod2Y;
            CalcMinGridLoc(ref pTowerSpace, 2, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLod2X, out iLod2Y);
            Int32 iTowerLod2Id = pLodTower.m_iFirstChildId + (iLod2X - iLodX * 2) + (iLod2Y - iLodY * 2) * 2;
            ref var pLod2Tower = ref pTowerSpace.m_pTowers[iTowerLod2Id];

            TowerCallback(ref pTowerSpace, false, ref pObj, ref pTower.m_Watcher);
            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Watcher);
            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Watcher);

            aoiNode_s retNode2;
            pLod2Tower.m_Marker.Remove(pObj.m_iSlotIndex, out retNode2);
            pLod2Tower.m_iMarkerCount--;
            return retNode2;
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void InsertLodWatcher(ref towerSpace_s pTowerSpace, Int32 iTowerId, aoiObj_s pObj, Int32 iX, Int32 iY, Int32 iLodX, Int32 iLodY)
        {
            Int32 iTowerLodId = iTowerId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;

            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            if (pLodTower.m_iFirstChildId == -1)
            {
                var pNode = new aoiNode_s
                {
                    m_iSlotIndex = pObj.m_iSlotIndex
                };

                pLodTower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Watcher);

                return;
            }


            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;
            float bminX = pObj.m_fPos[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fPos[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fPos[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fPos[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxx, out maxy);

            if (!IsInInside(iLodX * 2, iLodY * 2, iLodX * 2 + 1, iLodY * 2 + 1, minx, miny, maxx, maxy))
            {
                for (Int32 ly = 0; ly < 2; ly++)
                {
                    for (Int32 lx = 0; lx < 2; lx++)
                    {
                        if (IsInRect(iLodX * 2 + lx, iLodY * 2 + ly, minx, miny, maxx, maxy))
                        {
                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + ly * 2 + lx];
                            var pNode = new aoiNode_s
                            {
                                m_iSlotIndex = pObj.m_iSlotIndex
                            };

                            pLod2Tower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                        }
                    }
                }
            }
            else
            {
                var pNode = new aoiNode_s
                {
                    m_iSlotIndex = pObj.m_iSlotIndex
                };

                pLodTower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                for (Int32 i = 0; i < 4; i++)
                {
                    ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + i];
                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                }
            }
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveLodWatcher(ref towerSpace_s pTowerSpace, Int32 iTowerId, aoiObj_s pObj, Int32 iX, Int32 iY, Int32 iLodX, Int32 iLodY)
        {
            Int32 iTowerLodId = iTowerId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;
            ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
            if (pLodTower.m_iFirstChildId == -1)
            {
                aoiNode_s retNode;
                pLodTower.m_Watcher.Remove(pObj.m_iSlotIndex, out retNode);

                TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Marker);

                return;
            }

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;
            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bminZ, out maxx, out maxy);

            if (!IsInInside(iLodX * 2, iLodY * 2, iLodX * 2 + 1, iLodY * 2 + 1, minx, miny, maxx, maxy))
            {
                for (Int32 ly = 0; ly < 2; ly++)
                {
                    for (Int32 lx = 0; lx < 2; lx++)
                    {
                        if (IsInRect(iLodX * 2 + lx, iLodY * 2 + ly, minx, miny, maxx, maxy))
                        {
                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + ly * 2 + lx];

                            pLod2Tower.m_Watcher.Remove(pObj.m_iSlotIndex);

                            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Marker);
                        }
                    }
                }
            }
            else
            {
                pLodTower.m_Watcher.Remove(pObj.m_iSlotIndex);

                for (Int32 i = 0; i < 4; i++)
                {
                    ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + i];
                    TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Watcher);
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void InsertGridWatcher(ref towerSpace_s pTowerSpace, ref aoiObj_s pObj, Int32 iX, Int32 iY, float[] pos)
        {
            Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];
            if (iTowerId == -1)
            {
                ref var pTower1 = ref CreateGridTower(ref pTowerSpace, iX, iY);
                var pNode = new aoiNode_s
                {
                    m_iSlotIndex = pObj.m_iSlotIndex
                };

                pTower1.m_Watcher.Add(pObj.m_iSlotIndex, pNode);
                return;
            }

            ref var pTower2 = ref pTowerSpace.m_pTowers[iTowerId];
            if (pTower2.m_iFirstChildId == -1)
            {
                var pNode = new aoiNode_s
                {
                    m_iSlotIndex = pObj.m_iSlotIndex
                };

                pTower2.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                TowerCallback(ref pTowerSpace, true, ref pObj, ref pTower2.m_Marker);

                return;
            }

            Int32 minGridX = iX * 4;
            Int32 minGridY = iY * 4;
            Int32 maxGridX = iX * 4 + 3;
            Int32 maxGridY = iY * 4 + 3;

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;


            float bminX = pos[0] - pObj.m_fViewRadius;
            float bminZ = pos[2] - pObj.m_fViewRadius;
            float bmaxX = pos[0] + pObj.m_fViewRadius;
            float bmaxZ = pos[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxx, out maxy);

            if (!IsInInside(minGridX, minGridY, maxGridX, maxGridY, minx, miny, maxx, maxy))
            {
                for (Int32 y = 0; y < 2; y++)
                {
                    for (Int32 x = 0; x < 2; x++)
                    {
                        ref var pLodTower = ref pTowerSpace.m_pTowers[pTower2.m_iFirstChildId + y * 2 + x];
                        if (pLodTower.m_iFirstChildId == -1)
                        {
                            var pNode = new aoiNode_s
                            {
                                m_iSlotIndex = pObj.m_iSlotIndex
                            };

                            pLodTower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);
                            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Marker);
                        }
                        else
                        {
                            if (!IsInInside(minGridX + x * 2, minGridY + y * 2, minGridX + x * 2 + 1, minGridY + y * 2 + 1, minx, miny, maxx, maxy))
                            {
                                for (Int32 ly = 0; ly < 2; ly++)
                                {
                                    for (Int32 lx = 0; lx < 2; lx++)
                                    {
                                        if (IsInRect(minGridX + x * 2 + lx, minGridY + y * 2 + ly, minx, miny, maxx, maxy))
                                        {
                                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + ly * 2 + lx];

                                            var pNode = new aoiNode_s
                                            {
                                                m_iSlotIndex = pObj.m_iSlotIndex
                                            };

                                            pLod2Tower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);
                                            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var pNode = new aoiNode_s
                                {
                                    m_iSlotIndex = pObj.m_iSlotIndex
                                };

                                pLodTower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                                for (Int32 i = 0; i < 4; i++)
                                {
                                    ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + i];
                                    TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var pNode = new aoiNode_s
                {
                    m_iSlotIndex = pObj.m_iSlotIndex
                };

                pTower2.m_Watcher.Add(pObj.m_iSlotIndex, pNode);

                for (Int32 i = 0; i < 4; i++)
                {
                    ref var pLodTower = ref pTowerSpace.m_pTowers[pTower2.m_iFirstChildId + i];
                    if (pLodTower.m_iFirstChildId == -1)
                    {
                        TowerCallback(ref pTowerSpace, true, ref pObj, ref pLodTower.m_Marker);
                    }
                    else
                    {
                        for (Int32 j = 0; j < 4; j++)
                        {
                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + j];
                            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                        }
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveGridWatcher(ref towerSpace_s pTowerSpace, ref aoiObj_s pObj, Int32 iX, Int32 iY)
        {
            Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];

            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
            if (pTower.m_iFirstChildId == -1)
            {
                pTower.m_Watcher.Remove(pObj.m_iSlotIndex);
                TowerCallback(ref pTowerSpace, false, ref pObj, ref pTower.m_Marker);
                return;
            }

            Int32 minGridX = iX * 4;
            Int32 minGridY = iY * 4;
            Int32 maxGridX = iX * 4 + 3;
            Int32 maxGridY = iY * 4 + 3;


            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;
            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;


            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxx, out maxy);

            if (!IsInInside(minGridX, minGridY, maxGridX, maxGridY, minx, miny, maxx, maxy))
            {
                for (Int32 y = 0; y < 2; y++)
                {
                    for (Int32 x = 0; x < 2; x++)
                    {
                        ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + y * 2 + x];
                        if (pLodTower.m_iFirstChildId == -1)
                        {
                            pLodTower.m_Watcher.Remove(pObj.m_iSlotIndex);
                            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Marker);
                        }
                        else
                        {
                            if (!IsInInside(minGridX + x * 2, minGridY + y * 2, minGridX + x * 2 + 1, minGridY + y * 2 + 1, minx, miny, maxx, maxy))
                            {
                                for (Int32 ly = 0; ly < 2; ly++)
                                {
                                    for (Int32 lx = 0; lx < 2; lx++)
                                    {
                                        if (IsInRect(minGridX + x * 2 + lx, minGridY + y * 2 + ly, minx, miny, maxx, maxy))
                                        {
                                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + ly * 2 + lx];
                                            pLod2Tower.m_Watcher.Remove(pObj.m_iSlotIndex);
                                            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Marker);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                pLodTower.m_Watcher.Remove(pObj.m_iSlotIndex);

                                for (Int32 i = 0; i < 4; i++)
                                {
                                    ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + i];
                                    TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Marker);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                pTower.m_Watcher.Remove(pObj.m_iSlotIndex);

                for (Int32 i = 0; i < 4; i++)
                {
                    ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + i];
                    TowerCallback(ref pTowerSpace, false, ref pObj, ref pLodTower.m_Marker);
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void ChangeAoiObjMaskToWatcher(ref towerSpace_s pTowerSpace, ref aoiObj_s pObj, Int32 iX, Int32 iY, UInt64 uiMask)
        {
            Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];

            ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
            if (pTower.m_iFirstChildId == -1)
            {
                Int32 iChanged;
                foreach (var marker in pTower.m_Marker)
                {
                    if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                    {
                        iChanged = 0;
                        ref var pMarkerObj = ref pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                        if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                        {
                            iChanged = 0x1;
                        }

                        if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                        {
                            iChanged |= 0x2;
                        }

                        switch (iChanged)
                        {
                            case 0x1:
                                {
                                    pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                }
                                break;
                            case 0x2:
                                {
                                    pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                }
                                break;
                        }
                    }
                }
                return;
            }

            Int32 minGridX = iX * 4;
            Int32 minGridY = iY * 4;
            Int32 maxGridX = iX * 4 + 3;
            Int32 maxGridY = iY * 4 + 3;

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;

            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxx, out maxy);

            if (!IsInInside(minGridX, minGridY, maxGridX, maxGridY, minx, miny, maxx, maxy))
            {
                for (Int32 y = 0; y < 2; y++)
                {
                    for (Int32 x = 0; x < 2; x++)
                    {
                        ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + y * 2 + x];
                        if (pLodTower.m_iFirstChildId == -1)
                        {
                            Int32 iChanged;
                            foreach (var marker in pLodTower.m_Marker)
                            {
                                if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                                {
                                    iChanged = 0;
                                    aoiObj_s pMarkerObj = pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                                    if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                                    {
                                        iChanged = 0x1;
                                    }

                                    if (0 != (pMarkerObj.m_iMode & uiMask))
                                    {
                                        iChanged |= 0x2;
                                    }

                                    switch (iChanged)
                                    {
                                        case 0x1:
                                            {
                                                pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                            }
                                            break;
                                        case 0x2:
                                            {
                                                pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                            }
                                            break;
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (!IsInInside(minGridX + x * 2, minGridY + y * 2, minGridX + x * 2 + 1, minGridY + y * 2 + 1, minx, miny, maxx, maxy))
                            {
                                for (Int32 ly = 0; ly < 2; ly++)
                                {
                                    for (Int32 lx = 0; lx < 2; lx++)
                                    {
                                        if (IsInRect(minGridX + x * 2 + lx, minGridY + y * 2 + ly, minx, miny, maxx, maxy))
                                        {
                                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + ly * 2 + lx];
                                            Int32 iChanged;
                                            foreach (var marker in pLod2Tower.m_Marker)
                                            {
                                                if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                                                {
                                                    iChanged = 0;
                                                    aoiObj_s pMarkerObj = pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                                                    if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                                                    {
                                                        iChanged = 0x1;
                                                    }

                                                    if (0 != (pMarkerObj.m_iMode & uiMask))
                                                    {
                                                        iChanged |= 0x2;
                                                    }

                                                    switch (iChanged)
                                                    {
                                                        case 0x1:
                                                            {
                                                                pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                                            }
                                                            break;
                                                        case 0x2:
                                                            {
                                                                pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                                            }
                                                            break;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Int32 iChanged;
                                for (Int32 i = 0; i < 4; i++)
                                {
                                    ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + i];
                                    foreach (var marker in pLod2Tower.m_Marker)
                                    {
                                        if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                                        {
                                            ref var pMarkerObj = ref pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                                            iChanged = 0;
                                            if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                                            {
                                                iChanged = 0x1;
                                            }

                                            if (0 != (pMarkerObj.m_iMode & uiMask))
                                            {
                                                iChanged |= 0x2;
                                            }

                                            switch (iChanged)
                                            {
                                                case 0x1:
                                                    {
                                                        pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                                    }
                                                    break;
                                                case 0x2:
                                                    {
                                                        pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Int32 iChanged;
                for (Int32 i = 0; i < 4; i++)
                {
                    ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + i];
                    if (pLodTower.m_iFirstChildId == -1)
                    {
                        foreach (var marker in pLodTower.m_Marker)
                        {
                            if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                            {
                                ref var pMarkerObj = ref pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                                iChanged = 0;
                                if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                                {
                                    iChanged = 0x1;
                                }

                                if (0 != (pMarkerObj.m_iMode & uiMask))
                                {
                                    iChanged |= 0x2;
                                }

                                switch (iChanged)
                                {
                                    case 0x1:
                                        {
                                            pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                        }
                                        break;
                                    case 0x2:
                                        {
                                            pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (Int32 j = 0; j < 4; j++)
                        {
                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + j];
                            foreach (var marker in pLod2Tower.m_Marker)
                            {
                                if (marker.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                                {
                                    ref var pMarkerObj = ref pTowerSpace.m_pSlotObj[marker.Value.m_iSlotIndex];
                                    iChanged = 0;
                                    if (0 != (pMarkerObj.m_iMode & pObj.m_uiMask))
                                    {
                                        iChanged = 0x1;
                                    }

                                    if (0 != (pMarkerObj.m_iMode & uiMask))
                                    {
                                        iChanged |= 0x2;
                                    }

                                    switch (iChanged)
                                    {
                                        case 0x1:
                                            {
                                                pTowerSpace.m_CallBack(false, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                            }
                                            break;
                                        case 0x2:
                                            {
                                                pTowerSpace.m_CallBack(true, pObj.m_uiUserId, pMarkerObj.m_uiUserId);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static void ChangeAoiObjWatcher(ref towerSpace_s pTowerSpace, ref aoiObj_s pObj)
        {
            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

            Int32 minxLast;
            Int32 minyLast;
            Int32 maxxLast;
            Int32 maxyLast;

            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minxLast, out minyLast);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxxLast, out maxyLast);

            minxLast = minxLast > 0 ? minxLast : 0;
            minyLast = minyLast > 0 ? minyLast : 0;
            maxxLast = maxxLast < pTowerSpace.m_iMaxWidth * 4 ? maxxLast : pTowerSpace.m_iMaxWidth * 4 - 1;
            maxyLast = maxyLast < pTowerSpace.m_iMaxHeight * 4 ? maxyLast : pTowerSpace.m_iMaxHeight * 4 - 1;

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;

            bminX = pObj.m_fPos[0] - pObj.m_fViewRadius;
            bminZ = pObj.m_fPos[2] - pObj.m_fViewRadius;
            bmaxX = pObj.m_fPos[0] + pObj.m_fViewRadius;
            bmaxZ = pObj.m_fPos[2] + pObj.m_fViewRadius;


            CalcMinGridLoc(ref pTowerSpace, 2, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 2, bmaxX, 0, bmaxZ, out maxx, out maxy);

            minx = minx > 0 ? minx : 0;
            miny = miny > 0 ? miny : 0;
            maxx = maxx < pTowerSpace.m_iMaxWidth * 4 ? maxx : pTowerSpace.m_iMaxWidth * 4 - 1;
            maxy = maxy < pTowerSpace.m_iMaxHeight * 4 ? maxy : pTowerSpace.m_iMaxHeight * 4 - 1;

            if (IsOverlap(minx, miny, maxx, maxy, minxLast, minyLast, maxxLast, maxyLast))
            {
                Int32 iMinX = minx < minxLast ? minx : minxLast;
                Int32 iMinY = miny < minyLast ? miny : minyLast;
                Int32 iMaxX = maxx >= maxxLast ? maxx : maxxLast;
                Int32 iMaxY = maxy >= maxyLast ? maxy : maxyLast;

                Int32 iChanged = 0;

                for (Int32 iY = iMinY / 4; iY <= (iMaxY + 3) / 4; ++iY)
                {
                    for (Int32 iX = iMinX / 4; iX <= (iMaxX + 3) / 4; ++iX)
                    {
                        iChanged = 0;
                        if (IsInRect(iX, iY, minxLast / 4, minyLast / 4, (maxxLast + 3) / 4, (maxyLast + 3) / 4))
                        {
                            iChanged = 0x1;
                        }

                        if (IsInRect(iX, iY, minx / 4, miny / 4, (maxx + 3) / 4, (maxy + 3) / 4))
                        {
                            iChanged |= 0x2;
                        }

                        switch (iChanged)
                        {
                            case 0x1:
                                {
                                    RemoveGridWatcher(ref pTowerSpace, ref pObj, iX, iY);
                                }
                                break;
                            case 0x2:
                                {
                                    InsertGridWatcher(ref pTowerSpace, ref pObj, iX, iY, pObj.m_fPos);
                                }
                                break;
                            case 0x3:
                                {
                                    Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];
                                    ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
                                    if (pTower.m_iFirstChildId == -1)
                                    {
                                        continue;
                                    }

                                    for (Int32 ly = 0; ly < 2; ly++)
                                    {
                                        for (Int32 lx = 0; lx < 2; lx++)
                                        {
                                            iChanged = 0;
                                            if (IsInRect(iX * 2 + lx, iY * 2 + ly, minxLast / 2, minyLast / 2, (maxxLast + 1) / 2, (maxyLast + 1) / 2))
                                            {
                                                iChanged = 0x1;
                                            }

                                            if (IsInRect(iX * 2 + lx, iY * 2 + ly, minx / 2, miny / 2, (maxx + 1) / 2, (maxy + 1) / 2))
                                            {
                                                iChanged |= 0x2;
                                            }

                                            switch (iChanged)
                                            {
                                                case 0x1:
                                                    {
                                                        RemoveLodWatcher(ref pTowerSpace, iTowerId, pObj, iX, iY, iX * 2 + lx, iY * 2 + ly);
                                                    }
                                                    break;
                                                case 0x2:
                                                    {
                                                        InsertLodWatcher(ref pTowerSpace, iTowerId, pObj, iX, iY, iX * 2 + lx, iY * 2 + ly);
                                                    }
                                                    break;
                                                case 0x3:
                                                    {
                                                        ref var pLodTower = ref pTowerSpace.m_pTowers[pTower.m_iFirstChildId + ly * 2 + lx];
                                                        if (pLodTower.m_iFirstChildId == -1)
                                                        {
                                                            continue;
                                                        }

                                                        for (Int32 l2y = 0; l2y < 2; l2y++)
                                                        {
                                                            for (Int32 l2x = 0; l2x < 2; l2x++)
                                                            {
                                                                iChanged = 0;
                                                                if (IsInRect(iX * 4 + lx * 2 + l2x, iY * 4 + ly * 2 + l2y, minxLast, minyLast, maxxLast, maxyLast))
                                                                {
                                                                    iChanged = 0x1;
                                                                }

                                                                if (IsInRect(iX * 4 + lx * 2 + l2x, iY * 4 + ly * 2 + l2y, minx, miny, maxx, maxy))
                                                                {
                                                                    iChanged |= 0x2;
                                                                }

                                                                switch (iChanged)
                                                                {
                                                                    case 0x1:
                                                                        {
                                                                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + l2y * 2 + l2x];
                                                                            pLod2Tower.m_Watcher.Remove(pObj.m_iSlotIndex);
                                                                            TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2Tower.m_Marker);
                                                                        }
                                                                        break;
                                                                    case 0x2:
                                                                        {
                                                                            ref var pLod2Tower = ref pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + l2y * 2 + l2x];
                                                                            var pNode = new aoiNode_s
                                                                            {
                                                                                m_iSlotIndex = pObj.m_iSlotIndex
                                                                            };

                                                                            pLod2Tower.m_Watcher.Add(pObj.m_iSlotIndex, pNode);
                                                                            TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2Tower.m_Marker);
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                for (Int32 iY = minyLast / 4; iY <= (maxyLast + 3) / 4; ++iY)
                {
                    for (Int32 iX = minxLast / 4; iX <= (maxxLast + 3) / 4; ++iX)
                    {
                        RemoveGridWatcher(ref pTowerSpace, ref pObj, iX, iY);
                    }
                }

                for (Int32 iY = miny / 4; iY <= (maxy + 3) / 4; ++iY)
                {
                    for (Int32 iX = minx / 4; iX <= (maxx + 3) / 4; ++iX)
                    {
                        InsertGridWatcher(ref pTowerSpace, ref pObj, iX, iY, pObj.m_fPos);
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]

        static internal void DefaultCallback(bool bAddTo, UInt64 watcherId, UInt64 markerId) { }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal towerSpace_s CreateTowerSpace(float fGridLength, float fMinX, float fMinZ,
            float fMaxWidth, float fMaxHeigth, Int32 iSplitThreshold)
        {
            towerSpace_s pTowerSpace = new towerSpace_s
            {
                m_CallBack = DefaultCallback,
                m_fGridLength = new float[3] { fGridLength, fGridLength / 2, fGridLength / 4 },
                m_fMovefRange = (fGridLength / 8) * (fGridLength / 8),
                m_fMin = new float[2] { fMinX, fMinZ },
                m_iSplitThreshold = iSplitThreshold,
                m_iMaxWidth = (Int32)Math.Ceiling(fMaxWidth / fGridLength),
                m_iMaxHeight = (Int32)Math.Ceiling(fMaxHeigth / fGridLength),
                m_iTowerCapacity = 512,
                m_iTowerNext = 0,
                m_pTowers = new tower_s[512],
                m_iSlotCapacity = 128,
                m_iSlotIndex = 0,
                m_pSlotObj = new aoiObj_s[128],
            };

            for (int i = 0; i < 512; i++)
            {
                pTowerSpace.m_pTowers[i].m_Watcher = new Dictionary<int, aoiNode_s>();
                pTowerSpace.m_pTowers[i].m_Marker = new Dictionary<int, aoiNode_s>();
                pTowerSpace.m_pTowers[i].m_iFirstChildId = -1;
                pTowerSpace.m_pTowers[i].m_iMarkerCount = 0;
            }

            pTowerSpace.m_pGrids = new int[pTowerSpace.m_iMaxWidth * pTowerSpace.m_iMaxHeight];
            for (Int32 i = 0; i < pTowerSpace.m_iMaxWidth * pTowerSpace.m_iMaxHeight; i++)
            {
                pTowerSpace.m_pGrids[i] = -1;
            }

            for (Int32 i = 0; i < pTowerSpace.m_iSlotCapacity; i++)
            {
                pTowerSpace.m_pSlotObj[i].m_iSlotIndex = i + 1;
                pTowerSpace.m_pSlotObj[i].m_fPos = new float[3];
                pTowerSpace.m_pSlotObj[i].m_fLast = new float[3];
            }

            return pTowerSpace;
        }

        static internal void TowerSpace_Release(ref towerSpace_s pTowerSpace)
        {
            Int32 iIndex;
            for (Int32 i = 0; i < pTowerSpace.m_iMaxWidth * pTowerSpace.m_iMaxHeight; ++i)
            {
                iIndex = pTowerSpace.m_pGrids[i];
                if (iIndex != -1)
                {
                    ClearGridTower(ref pTowerSpace, iIndex);
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal void TowerSpace_SetCallback(ref towerSpace_s pTowerSpace, callback fn)
        {
            pTowerSpace.m_CallBack = fn;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]

        static internal Int32 TowerSpace_AddAoiObj(ref towerSpace_s pTowerSpace, UInt64 uiUserId, float PosX, float PosY, float PosZ, UInt64 uiMask)
        {
            Int32 iX;
            Int32 iY;
            CalcGridLoc(ref pTowerSpace, PosX, PosY, PosZ, out iX, out iY);
            if (iX < 0 || iX >= pTowerSpace.m_iMaxWidth || iY < 0 || iY >= pTowerSpace.m_iMaxHeight)
            {
                return -1;
            }

            Int32 iSlotIndex = pTowerSpace.m_iSlotIndex;
            if (iSlotIndex >= pTowerSpace.m_iSlotCapacity)
            {
                Int32 iOldiSlotCapacity = pTowerSpace.m_iSlotCapacity;
                pTowerSpace.m_iSlotCapacity *= 2;
                Array.Resize<aoiObj_s>(ref pTowerSpace.m_pSlotObj,pTowerSpace.m_iSlotCapacity);
                for (Int32 i = iOldiSlotCapacity; i < pTowerSpace.m_iSlotCapacity; i++)
                {
                    pTowerSpace.m_pSlotObj[i].m_iSlotIndex = i + 1;
                    pTowerSpace.m_pSlotObj[i].m_fPos = new float[3];
                    pTowerSpace.m_pSlotObj[i].m_fLast = new float[3];
                }
            }

            ref aoiObj_s pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            pTowerSpace.m_iSlotIndex = pObj.m_iSlotIndex;
            pObj.m_iSlotIndex = iSlotIndex;

            var pos = new float[] { PosX, PosY, PosZ };
            pObj.m_fPos = pos;
            pObj.m_fLast = pos;
            pObj.m_fViewRadius = 0;
            pObj.m_uiMask = uiMask;
            pObj.m_uiUserId = uiUserId;
            pObj.m_iMode = 0;
            return iSlotIndex;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal void TowerSpace_UpdateAoiObjMask(ref towerSpace_s pTowerSpace, Int32 iSlotIndex, UInt64 uiMask)
        {
            ref aoiObj_s pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return;
            }

            if (pObj.m_uiMask == uiMask)
            {
                return;
            }

            if (0 != (pObj.m_iMode & MARKER_MODE))
            {
                Int32 iX;
                Int32 iY;
                CalcGridLoc(ref pTowerSpace, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iX, out iY);
                Int32 iTowerId = pTowerSpace.m_pGrids[iX + iY * pTowerSpace.m_iMaxWidth];
                ref var pTower = ref pTowerSpace.m_pTowers[iTowerId];
                if (pTower.m_iFirstChildId == -1)
                {
                    Int32 iChanged = 0;
                    foreach (var watcher in pTower.m_Watcher)
                    {
                        if (watcher.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                        {
                            aoiObj_s pWatcherObj = pTowerSpace.m_pSlotObj[watcher.Value.m_iSlotIndex];
                            iChanged = 0;
                            if (0 != (pWatcherObj.m_iMode & pObj.m_uiMask))
                            {
                                iChanged = 0x1;
                            }

                            if (0 != (pWatcherObj.m_iMode & uiMask))
                            {
                                iChanged |= 0x2;
                            }

                            switch (iChanged)
                            {
                                case 0x1:
                                    {
                                        pTowerSpace.m_CallBack(false, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                    }
                                    break;
                                case 0x2:
                                    {
                                        pTowerSpace.m_CallBack(true, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                    }
                                    break;
                            }
                        }
                    }

                }
                else
                {
                    Int32 iLodX;
                    Int32 iLodY;
                    CalcMinGridLoc(ref pTowerSpace, 1, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLodX, out iLodY);
                    Int32 iTowerLodId = pTower.m_iFirstChildId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;
                    ref var pLodTower = ref pTowerSpace.m_pTowers[iTowerLodId];
                    if (pLodTower.m_iFirstChildId == -1)
                    {
                        Int32 iChanged = 0;
                        foreach (var watcher in pTower.m_Watcher)
                        {
                            if (watcher.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                            {
                                ref var pWatcherObj = ref pTowerSpace.m_pSlotObj[watcher.Value.m_iSlotIndex];
                                iChanged = 0;
                                if (0 != (pWatcherObj.m_iMode & pObj.m_uiMask))
                                {
                                    iChanged = 0x1;
                                }

                                if (0 != (pWatcherObj.m_iMode & uiMask))
                                {
                                    iChanged |= 0x2;
                                }

                                switch (iChanged)
                                {
                                    case 0x1:
                                        {
                                            pTowerSpace.m_CallBack(false, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                        }
                                        break;
                                    case 0x2:
                                        {
                                            pTowerSpace.m_CallBack(true, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Int32 iLod2X;
                        Int32 iLod2Y;
                        CalcMinGridLoc(ref pTowerSpace, 2, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLod2X, out iLod2Y);
                        Int32 iTowerLod2Id = pLodTower.m_iFirstChildId + (iLod2X - iLodX * 2) + (iLod2Y - iLodY * 2) * 2;
                        ref var pLod2Tower = ref pTowerSpace.m_pTowers[iTowerLod2Id];

                        Int32 iChanged = 0;
                        foreach (var watcher in pTower.m_Watcher)
                        {
                            if (watcher.Value.m_iSlotIndex != pObj.m_iSlotIndex)
                            {
                                ref var pWatcherObj = ref pTowerSpace.m_pSlotObj[watcher.Value.m_iSlotIndex];
                                iChanged = 0;
                                if (0 != (pWatcherObj.m_iMode & pObj.m_uiMask))
                                {
                                    iChanged = 0x1;
                                }

                                if (0 != (pWatcherObj.m_iMode & uiMask))
                                {
                                    iChanged |= 0x2;
                                }

                                switch (iChanged)
                                {
                                    case 0x1:
                                        {
                                            pTowerSpace.m_CallBack(false, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                        }
                                        break;
                                    case 0x2:
                                        {
                                            pTowerSpace.m_CallBack(true, Convert.ToUInt64(watcher.Value.m_iSlotIndex), Convert.ToUInt64(pObj.m_iSlotIndex));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (0 != (pObj.m_iMode & WATCHER_MODE))
            {
                Int32 minx;
                Int32 miny;
                Int32 maxx;
                Int32 maxy;

                float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
                float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
                float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
                float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

                CalcMinGridLoc(ref pTowerSpace, 0, bminX, 0, bminZ, out minx, out miny);
                CalcMaxGridLoc(ref pTowerSpace, 0, bmaxX, 0, bmaxZ, out maxx, out maxy);

                minx = minx > 0 ? minx : 0;
                miny = miny > 0 ? miny : 0;
                maxx = maxx < pTowerSpace.m_iMaxWidth ? maxx : pTowerSpace.m_iMaxWidth - 1;
                maxy = maxy < pTowerSpace.m_iMaxHeight ? maxy : pTowerSpace.m_iMaxHeight - 1;

                for (Int32 iY = miny; iY <= maxy; ++iY)
                {
                    for (Int32 iX = minx; iX <= maxx; ++iX)
                    {
                        ChangeAoiObjMaskToWatcher(ref pTowerSpace, ref pObj, iX, iY, uiMask);
                    }
                }
            }

            pObj.m_uiMask = uiMask;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal bool TowerSpace_UpdateAoiObjPos(ref towerSpace_s pTowerSpace, Int32 iSlotIndex, float PosX, float PosY, float PosZ)
        {
            ref aoiObj_s pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return false;
            }

            Int32 iX;
            Int32 iY;
            CalcGridLoc(ref pTowerSpace, PosX, PosY, PosZ, out iX, out iY);
            if (iX < 0 || iX >= pTowerSpace.m_iMaxWidth || iY < 0 || iY >= pTowerSpace.m_iMaxHeight)
            {
                return false;
            }

            pObj.m_fPos = new float[] { PosX, PosY, PosZ };
            if (pObj.m_iMode == 0)
            {
                pObj.m_fLast = pObj.m_fPos;
                return true;
            }

            if (!IsMoveNear(PosX, PosY, PosZ, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], pTowerSpace.m_fMovefRange))
            {
                Int32 iLastX;
                Int32 iLastY;
                CalcGridLoc(ref pTowerSpace, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLastX, out iLastY);

                if (iX != iLastX || iY != iLastY)
                {
                    if (0 != (pObj.m_iMode & MARKER_MODE))
                    {
                        aoiNode_s pNode = RemoveGridMarker(ref pTowerSpace, ref pObj, iLastX, iLastY);
                        InsertGridMarker(ref pTowerSpace, pNode, iX, iY, pObj.m_fPos[0], pObj.m_fPos[1], pObj.m_fPos[2]);
                    }

                    if (0 != (pObj.m_iMode & WATCHER_MODE))
                    {
                        ChangeAoiObjWatcher(ref pTowerSpace, ref pObj);
                    }
                }
                else
                {
                    if (0 != (pObj.m_iMode & MARKER_MODE))
                    {
                        Int32 iTowerId = pTowerSpace.m_pGrids[iLastX + iLastY * pTowerSpace.m_iMaxWidth];
                        var pTower = pTowerSpace.m_pTowers[iTowerId];
                        if (pTower.m_iFirstChildId != -1)
                        {
                            Int32 iLodXLast;
                            Int32 iLodYLast;
                            CalcMinGridLoc(ref pTowerSpace, 1, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLodXLast, out iLodYLast);

                            Int32 iLodX;
                            Int32 iLodY;
                            CalcMinGridLoc(ref pTowerSpace, 1, pObj.m_fPos[0], pObj.m_fPos[1], pObj.m_fPos[2], out iLodX, out iLodY);

                            if (iLodX != iLodXLast || iLodY != iLodYLast)
                            {
                                aoiNode_s pNode = RemoveLodMarker(ref pTowerSpace, pTower.m_iFirstChildId, ref pObj, iLastX, iLastY, iLodXLast, iLodYLast);
                                InsertLodMarker(ref pTowerSpace, pTower.m_iFirstChildId, pNode, pObj, iLastX, iLastY, iLodX, iLodY, pObj.m_fPos[0], pObj.m_fPos[1], pObj.m_fPos[2]);
                            }
                            else
                            {
                                Int32 iTowerLodId = pTower.m_iFirstChildId + (iLodX - iX * 2) + (iLodY - iY * 2) * 2;
                                var pLodTower = pTowerSpace.m_pTowers[iTowerLodId];
                                if (pLodTower.m_iFirstChildId != -1)
                                {
                                    Int32 iLod2XLast;
                                    Int32 iLod2YLast;
                                    CalcMinGridLoc(ref pTowerSpace, 2, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iLod2XLast, out iLod2YLast);

                                    Int32 iLod2X;
                                    Int32 iLod2Y;
                                    CalcMinGridLoc(ref pTowerSpace, 2, pObj.m_fPos[0], pObj.m_fPos[1], pObj.m_fPos[2], out iLod2X, out iLod2Y);
                                    if (iLod2X != iLod2XLast || iLod2Y != iLod2YLast)
                                    {
                                        Int32 iTowerLod2IdLast = pLodTower.m_iFirstChildId + (iLod2XLast - iLodXLast * 2) + (iLod2YLast - iLodYLast * 2) * 2;
                                        var pLod2TowerLast = pTowerSpace.m_pTowers[iTowerLod2IdLast];
                                        TowerCallback(ref pTowerSpace, false, ref pObj, ref pLod2TowerLast.m_Watcher);

                                        aoiNode_s findNode;
                                        pLod2TowerLast.m_Marker.Remove(pObj.m_iSlotIndex, out findNode);
                                        pLod2TowerLast.m_iMarkerCount--;

                                        var pLod2Tower = pTowerSpace.m_pTowers[pLodTower.m_iFirstChildId + (iLod2X - iLodX * 2) + (iLod2Y - iLodY * 2) * 2];
                                        pLod2Tower.m_Marker.Add(pObj.m_iSlotIndex, findNode);
                                        ++pLod2Tower.m_iMarkerCount;
                                        TowerCallback(ref pTowerSpace, true, ref pObj, ref pLod2TowerLast.m_Watcher);

                                    }
                                }
                            }
                        }
                    }

                    if (0 != (pObj.m_iMode & WATCHER_MODE))
                    {
                        ChangeAoiObjWatcher(ref pTowerSpace, ref pObj);
                    }
                }

                pObj.m_fLast = pObj.m_fPos;
            }
            return true;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal bool TowerSpace_AddAoiObjWatcher(ref towerSpace_s pTowerSpace, Int32 iSlotIndex, float fViewRadius)
        {
            ref var pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return false;
            }

            if (0 != (pObj.m_iMode & WATCHER_MODE))
            {
                return false;
            }

            pObj.m_iMode |= WATCHER_MODE;
            pObj.m_fViewRadius = fViewRadius;

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;

            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 0, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 0, bmaxX, 0, bmaxZ, out maxx, out maxy);

            minx = minx > 0 ? minx : 0;
            miny = miny > 0 ? miny : 0;
            maxx = maxx < pTowerSpace.m_iMaxWidth ? maxx : pTowerSpace.m_iMaxWidth - 1;
            maxy = maxy < pTowerSpace.m_iMaxHeight ? maxy : pTowerSpace.m_iMaxHeight - 1;

            for (Int32 iY = miny; iY <= maxy; ++iY)
            {
                for (Int32 iX = minx; iX <= maxx; ++iX)
                {
                    InsertGridWatcher(ref pTowerSpace, ref pObj, iX, iY, pObj.m_fLast);
                }
            }

            return true;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal bool TowerSpace_RemoveAoiObjWatcher(ref towerSpace_s pTowerSpace, Int32 iSlotIndex)
        {
            ref var pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return false;
            }

            if (0 == (pObj.m_iMode & WATCHER_MODE))
            {
                return false;
            }

            Int32 minx;
            Int32 miny;
            Int32 maxx;
            Int32 maxy;

            float bminX = pObj.m_fLast[0] - pObj.m_fViewRadius;
            float bminZ = pObj.m_fLast[2] - pObj.m_fViewRadius;
            float bmaxX = pObj.m_fLast[0] + pObj.m_fViewRadius;
            float bmaxZ = pObj.m_fLast[2] + pObj.m_fViewRadius;

            CalcMinGridLoc(ref pTowerSpace, 0, bminX, 0, bminZ, out minx, out miny);
            CalcMaxGridLoc(ref pTowerSpace, 0, bmaxX, 0, bmaxZ, out maxx, out maxy);

            minx = minx > 0 ? minx : 0;
            miny = miny > 0 ? miny : 0;
            maxx = maxx < pTowerSpace.m_iMaxWidth ? maxx : pTowerSpace.m_iMaxWidth - 1;
            maxy = maxy < pTowerSpace.m_iMaxHeight ? maxy : pTowerSpace.m_iMaxHeight - 1;

            for (Int32 iY = miny; iY <= maxy; ++iY)
            {
                for (Int32 iX = minx; iX <= maxx; ++iX)
                {
                    RemoveGridWatcher(ref pTowerSpace, ref pObj, iX, iY);
                }
            }

            pObj.m_iMode &= ~WATCHER_MODE;
            return true;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal bool TowerSpace_AddAoiObjMarker(ref towerSpace_s pTowerSpace, Int32 iSlotIndex)
        {
            ref aoiObj_s pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return false;
            }

            if (0 != (pObj.m_iMode & MARKER_MODE))
            {
                return false;
            }
            pObj.m_iMode |= MARKER_MODE;
            Int32 iX;
            Int32 iY;
            CalcGridLoc(ref pTowerSpace, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iX, out iY);

            var pNode = new aoiNode_s
            {
                m_iSlotIndex = iSlotIndex
            };

            InsertGridMarker(ref pTowerSpace, pNode, iX, iY, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2]);
            return true;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal bool TowerSpace_RemoveAoiObjMarker(ref towerSpace_s pTowerSpace, Int32 iSlotIndex)
        {
            ref var pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return false;
            }

            if (0 != (pObj.m_iMode & MARKER_MODE))
            {
                return false;
            }

            Int32 iX;
            Int32 iY;
            CalcGridLoc(ref pTowerSpace, pObj.m_fLast[0], pObj.m_fLast[1], pObj.m_fLast[2], out iX, out iY);

            RemoveGridMarker(ref pTowerSpace, ref pObj, iX, iY);
            pObj.m_iMode &= ~MARKER_MODE;
            return true;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static internal void TowerSpace_RemoveAoiObj(ref towerSpace_s pTowerSpace, Int32 iSlotIndex)
        {
            ref aoiObj_s pObj = ref pTowerSpace.m_pSlotObj[iSlotIndex];
            if (pObj.m_iSlotIndex != iSlotIndex)
            {
                return;
            }

            if (0 != (pObj.m_iMode & WATCHER_MODE))
            {
                TowerSpace_RemoveAoiObjWatcher(ref pTowerSpace, iSlotIndex);
            }

            if (0 != (pObj.m_iMode & MARKER_MODE))
            {
                TowerSpace_RemoveAoiObjMarker(ref pTowerSpace, iSlotIndex);
            }

            Int32 iIndex = pObj.m_iSlotIndex;
            pObj.m_iSlotIndex = pTowerSpace.m_iSlotIndex;
            pTowerSpace.m_iSlotIndex = iIndex;
        }
    }
}
