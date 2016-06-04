using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;

namespace Beans
{
    public class PathFinder
    {
        private int[,] mVirtualMap = new int[Constants.BLOCK_ROW_COUNT + 2, Constants.BLOCK_COLUMN_COUNT + 2]; // 要有邊框才能正確結束
        private int[,] mMap = new int[Constants.BLOCK_ROW_COUNT, Constants.BLOCK_COLUMN_COUNT];
        private List<Position> mPath = new List<Position>();

        private int mSourceRow = -1;
        private int mSourceColumn = -1;
        private int mTargetRow = -1;
        private int mTargetColumn = -1;

        public PathFinder()
        {
            mVirtualMap.Initialize();
        }

        public PathFinder(int[,] map, int sr, int sc, int tr, int tc)
        {
            mMap = (int[,])map.Clone();
            InitVirtualMap();
            mSourceRow = sr + 1;
            mSourceColumn = sc + 1;
            mTargetRow = tr + 1;
            mTargetColumn = tc + 1;
        }

        private void InitVirtualMap()
        {
            int nRowCount = Constants.BLOCK_ROW_COUNT + 2;
            int nColCount = Constants.BLOCK_COLUMN_COUNT + 2;
            for (int row = 0; row < nRowCount; ++row)
            {
                for (int col = 0; col < nColCount; ++col)
                {
                    // 把 x、y 為 0 的元素及 x、y 為最大值的元素填上 9
                    if (row == 0 || col == 0 || (row == nRowCount - 1) || (col == nColCount - 1))
                    {
                        mVirtualMap[row, col] = 9;
                    }
                    else
                    {
                        mVirtualMap[row, col] = mMap[row - 1, col - 1];
                    }
                }
            }

            //TraceMap();
        }

        public Boolean StartFind()
        {
            Boolean bFind = false;

            mPath.Clear();
            if (mSourceRow != -1 && mTargetRow != -1)
            {
                bFind = Visit(mSourceRow, mSourceColumn);
            }

            return bFind;
        }

        public List<Position> GetPath()
        {
            List<Position> reversePath = new List<Position>();
            for (int i = mPath.Count - 1; i >= 0; --i)
            {
                reversePath.Add(new Position(mPath[i].Row -1, mPath[i].Column -1));
            }
            //foreach (Position pos in mPath)
            //{
            //    pos.Row -= 1;
            //    pos.Column -= 1;
            //}
            return reversePath;
        }

        /// <summary>
        /// 終點是路徑之一嗎？
        /// </summary>
        private Boolean IsArrived()
        {
            return (mVirtualMap[mTargetRow, mTargetColumn] == Constants.PATH_TYPE);
        }

        /// <summary>
        /// 循訪空節點找路徑
        /// </summary>
        private Boolean Visit(int row, int col)
        {
            mVirtualMap[row, col] = Constants.PATH_TYPE;

            if (!IsArrived() && mVirtualMap[row - 1, col] == 0)
            {
                Visit(row - 1, col);
            }
            if (!IsArrived() && mVirtualMap[row, col - 1] == 0)
            {
                Visit(row, col - 1);
            }
            if (!IsArrived() && mVirtualMap[row + 1, col] == 0)
            {
                Visit(row + 1, col);
            }
            if (!IsArrived() && mVirtualMap[row, col + 1] == 0)
            {
                Visit(row, col + 1);
            }

            if (!IsArrived())
            {
                mVirtualMap[row, col] = 0;
            }

            if (mVirtualMap[row, col] == Constants.PATH_TYPE)
            {
                mPath.Add(new Position(row, col));
            }

            return IsArrived();
        }

        private void TraceMap()
        {
            int nRowCount = Constants.BLOCK_ROW_COUNT + 2;
            int nColCount = Constants.BLOCK_COLUMN_COUNT + 2;

            String strDebug = "";
            for (int row = 0; row < nRowCount; ++row)
            {
                strDebug = String.Empty;
                for (int col = 0; col < nColCount; ++col)
                {
                    strDebug += String.Format("{0:d2}  ", mVirtualMap[row, col]);
                }
                Debug.WriteLine(strDebug);
            }
        }
    }
}
