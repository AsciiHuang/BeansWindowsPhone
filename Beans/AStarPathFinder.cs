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

namespace Beans
{
    public class AStarPathFinder
    {
        #region Initialization
        Position[] _movements;
        List<Position> mPath = new List<Position>();

        CompleteSquare[,] _squares = new CompleteSquare[Constants.BLOCK_ROW_COUNT, Constants.BLOCK_COLUMN_COUNT];
        public CompleteSquare[,] Squares
        {
            get { return _squares; }
            set { _squares = value; }
        }

        public AStarPathFinder()
        {
            InitMovements(4);
            ClearSquares();
        }

        public void InitMovements(int movementCount)
        {
            if (movementCount == 4)
            {
                _movements = new Position[]
                {
                    new Position(0, -1),
                    new Position(1, 0),
                    new Position(0, 1),
                    new Position(-1, 0)
                };
            }
            else
            {
                _movements = new Position[]
                {
                    new Position(-1, -1),
                    new Position(0, -1),
                    new Position(1, -1),
                    new Position(1, 0),
                    new Position(1, 1),
                    new Position(0, 1),
                    new Position(-1, 1),
                    new Position(-1, 0)
                };
            }
        }

        public void ClearSquares()
        {
            foreach (Position pos in AllSquares())
            {
                _squares[pos.Row, pos.Column] = new CompleteSquare();
            }
        }

        public void ClearLogic()
        {
            foreach (Position pos in AllSquares())
            {
                int row = pos.Row;
                int col = pos.Column;
                _squares[row, col].DistanceSteps = 10000;
                _squares[row, col].IsPath = false;
            }
        }
        #endregion

        public void ReadMap(int[,] map, int sr, int sc, int tr, int tc)
        {
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    if (map[row, col] == 0)
                    {
                        _squares[row, col].ContentCode = SquareContent.Empty;
                    }
                    else
                    {
                        _squares[row, col].ContentCode = SquareContent.Wall;
                    }
                }
            }

            // 起點為 Monster
            _squares[sr, sc].ContentCode = SquareContent.Monster;

            // 終點為 Hero
            _squares[tr, tc].ContentCode = SquareContent.Hero;

        }

        public void Pathfind()
        {
            Position startingPos = FindCode(SquareContent.Hero);
            int heroRow = startingPos.Row;
            int heroCol = startingPos.Column;
            if (heroRow == -1 || heroCol == -1)
            {
                return;
            }

            _squares[heroRow, heroCol].DistanceSteps = 0;

            while (true)
            {
                bool madeProgress = false;

                foreach (Position mainPos in AllSquares())
                {
                    int row = mainPos.Row;
                    int col = mainPos.Column;

                    if (SquareOpen(row, col)) // 回傳是否可走(空、起、終點)
                    {
                        int passHere = _squares[row, col].DistanceSteps;

                        foreach (Position movePos in ValidMoves(row, col)) // 合法的移動
                        {
                            int newRow = movePos.Row;
                            int newCol = movePos.Column;
                            int newPass = passHere + 1;

                            if (_squares[newRow, newCol].DistanceSteps > newPass)
                            {
                                _squares[newRow, newCol].DistanceSteps = newPass;
                                madeProgress = true;
                            }
                        }
                    }
                }

                if (!madeProgress)
                {
                    break;
                }
            }
        }

        public List<Position> GetPath()
        {
            if (mPath.Count > 0)
            {
                Position startingPos = FindCode(SquareContent.Monster);
                mPath.Insert(0, startingPos);
            }
            return mPath;
        }

        public void HighlightPath()
        {
            mPath.Clear();
            Position startingPos = FindCode(SquareContent.Monster);
            int posRow = startingPos.Row;
            int posCol = startingPos.Column;
            if (posRow == -1 && posCol == -1)
            {
                return;
            }

            while (true)
            {
                Position lowestPos = Position.Empty;
                int lowest = 10000;

                foreach (Position movePos in ValidMoves(posRow, posCol))
                {
                    int count = _squares[movePos.Row, movePos.Column].DistanceSteps;
                    if (count < lowest)
                    {
                        lowest = count;
                        lowestPos.Row = movePos.Row;
                        lowestPos.Column = movePos.Column;
                    }
                }
                if (lowest != 10000)
                {
                    mPath.Add(new Position(lowestPos.Row, lowestPos.Column));
                    _squares[lowestPos.Row, lowestPos.Column].IsPath = true;
                    posRow = lowestPos.Row;
                    posCol = lowestPos.Column;
                }
                else
                {
                    break;
                }

                if (_squares[posRow, posCol].ContentCode == SquareContent.Hero)
                {
                    break;
                }
            }
        }

        // 是否在 0~11 之內
        private bool ValidCoordinates(int row, int col)
        {
            if (row < 0)
            {
                return false;
            }
            if (col < 0)
            {
                return false;
            }
            if (row > (Constants.BLOCK_ROW_COUNT - 1))
            {
                return false;
            }
            if (col > (Constants.BLOCK_COLUMN_COUNT - 1))
            {
                return false;
            }
            return true;
        }

        // 回傳是否可以走 (H、M、Empty)
        private bool SquareOpen(int row, int col)
        {
            switch (_squares[row, col].ContentCode)
            {
                case SquareContent.Empty:
                    return true;
                case SquareContent.Hero:
                    return true;
                case SquareContent.Monster:
                    return true;
                case SquareContent.Wall:
                default:
                    return false;
            }
        }

        // 指定 H、M 回傳位置
        private Position FindCode(SquareContent contentIn)
        {
            foreach (Position pos in AllSquares())
            {
                if (_squares[pos.Row, pos.Column].ContentCode == contentIn)
                {
                    return new Position(pos.Row, pos.Column);
                }
            }
            return new Position(-1, -1);
        }

        // 列舉出所有的節點
        private static IEnumerable<Position> AllSquares()
        {
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; row++)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; col++)
                {
                    yield return new Position(row, col);
                }
            }
        }

        // 列舉出所有 ValidCoordinates 與 SquareOpen
        private IEnumerable<Position> ValidMoves(int row, int col)
        {
            foreach (Position movePoint in _movements)
            {
                int newRow = row + movePoint.Row;
                int newCol = col + movePoint.Column;

                if (ValidCoordinates(newRow, newCol) &&
                    SquareOpen(newRow, newCol))
                {
                    yield return new Position(newRow, newCol);
                }
            }
        }
    }
}
