using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace Beans
{
    public partial class GamePage : PhoneApplicationPage
    {
        private int[,] mVirtualMap = new int[Constants.BLOCK_ROW_COUNT, Constants.BLOCK_COLUMN_COUNT];
        private Position[,] mVisualMap = new Position[Constants.BLOCK_ROW_COUNT, Constants.BLOCK_COLUMN_COUNT];
        private Image[,] mBeansMap = new Image[Constants.BLOCK_ROW_COUNT, Constants.BLOCK_COLUMN_COUNT]; // 記下每個格子的圖片
        private List<Position> mPath = new List<Position>();
        private List<Position> mChain = new List<Position>();
        private int[] mNextBeans = new int[3]; // 接下來要出現的三顆種子
        private int mBeansMaxCount = Constants.BEANS_COUNT_HARD; // 此局要出現多少豆子種類

        private DispatcherTimer timerMessageHide = null;
        private Boolean mIsSelectedRole = false;
        private Boolean mAnimationRunning = false;
        private Position mPosPrevious = new Position(-1, -1);
        private Position mPosCurrent = new Position(-1, -1);
        Random mRandom = new Random();
        private Boolean mShowGameOver = false;
        private int mScore = 0;

        public GamePage()
        {
            InitializeComponent();
            InitBensCount();
            InitMapPosition();
            CreateBeansMap();
            InitNewGame();

            timerMessageHide = new DispatcherTimer();
            timerMessageHide.Interval = TimeSpan.FromSeconds(1.2);
            timerMessageHide.Tick += new EventHandler(OnShowTimerTick);
        }

        private void InitBensCount()
        {
            mBeansMaxCount = Utility.GetKeyValue(Constants.BEANS_SETTING_GAME_MODE, Constants.BEANS_COUNT_HARD);
        }

        private void InitNewGame()
        {
            mIsSelectedRole = false;
            mAnimationRunning = false;
            mPosCurrent.Row = -1;
            mPosCurrent.Column = -1;
            Role.Visibility = Visibility.Collapsed;
            InitVirtualMap();
            InitBeansMap();
            CreateNextBeans();
            PutNextBeans();
            mScore = 0;
            ScorePanel.Text = mScore.ToString();
            if (mShowGameOver)
            {
                mShowGameOver = false;
                HideMessagePanel();
            }
        }

        /// <summary>
        /// 運算出每個格子的 Left、Top
        /// </summary>
        private void InitMapPosition()
        {
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    int nLeft = ((col + 1) * 3) + (col * Constants.BLOCK_SIZE);
                    int nTop = ((row + 1) * 3) + (row * Constants.BLOCK_SIZE);
                    mVisualMap[row, col] = new Position(nTop, nLeft);
                }
            }
        }

        /// <summary>
        /// 產生足夠的種子圖片元件到 GameSpace 裡面
        /// </summary>
        private void CreateBeansMap()
        {
            // 把 mBeansMap 依 mVisualMap 的座標排好
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    mBeansMap[row, col] = new Image();
                    mBeansMap[row, col].Width = Constants.BLOCK_SIZE;
                    mBeansMap[row, col].Height = Constants.BLOCK_SIZE;
                    mBeansMap[row, col].RenderTransformOrigin = new Point(0.5, 0.5);
                    ScaleTransform trans = new ScaleTransform();
                    mBeansMap[row, col].RenderTransform = trans;
                    GameSpace.Children.Add(mBeansMap[row, col]);
                    Canvas.SetLeft(mBeansMap[row, col], mVisualMap[row, col].X);
                    Canvas.SetTop(mBeansMap[row, col], mVisualMap[row, col].Y);
                }
            }
        }

        /// <summary>
        /// 將每個格子記錄的種子類型清空
        /// </summary>
        private void InitVirtualMap()
        {
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    mVirtualMap[row, col] = 0;
                }
            }
        }

        /// <summary>
        /// 將每個格子記錄的圖片清空
        /// </summary>
        private void InitBeansMap()
        {
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    mBeansMap[row, col].Source = null;
                }
            }
        }

        private void StoreScore()
        {
            int easyScore = Utility.GetKeyValue(Constants.BEANS_SETTING_EASYMODE_SCORE, 0);
            int hardScore = Utility.GetKeyValue(Constants.BEANS_SETTING_HARDMODE_SCORE, 0);
            if(mBeansMaxCount == Constants.BEANS_COUNT_HARD)
            {
                // 難
                if (mScore > hardScore)
                {
                    Utility.SetKeyValue(Constants.BEANS_SETTING_HARDMODE_SCORE, mScore);
                }
            }
            else
            {
                // 易
                if (mScore > easyScore)
                {
                    Utility.SetKeyValue(Constants.BEANS_SETTING_EASYMODE_SCORE, mScore);
                }
            }
        }

        private void OnShowTimerTick(object sender, EventArgs e)
        {
            timerMessageHide.Stop();
            mShowGameOver = false;
            HideMessagePanel();
        }

        #region Message Panel
        private void ShowInvalidPath()
        {
            ShowMessage("Invalid Path", true);
        }

        private void ShowGameOver()
        {
            if (mShowGameOver)
            {
                return;
            }
            ShowMessage("Game Over", false);
            mShowGameOver = true;
        }

        private void ShowMessage(String msg, Boolean autoHide)
        {
            MessageTextPanel.Text = msg;

            ScaleTransform panel = MessagePanel.RenderTransform as ScaleTransform;
            DoubleAnimation animX = new DoubleAnimation();
            animX.From = 0;
            animX.To = 1;
            animX.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            DoubleAnimation animY = new DoubleAnimation();
            animY.From = 0;
            animY.To = 1;
            animY.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            Storyboard.SetTarget(animX, panel);
            Storyboard.SetTarget(animY, panel);
            Storyboard.SetTargetProperty(animX, new PropertyPath(ScaleTransform.ScaleXProperty));
            Storyboard.SetTargetProperty(animY, new PropertyPath(ScaleTransform.ScaleYProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animX);
            storyboard.Children.Add(animY);
            storyboard.Begin();

            if (autoHide)
            {
                timerMessageHide.Start();
            }
        }

        private void HideMessagePanel()
        {
            ScaleTransform panel = MessagePanel.RenderTransform as ScaleTransform;
            DoubleAnimation animX = new DoubleAnimation();
            animX.From = 1;
            animX.To = 0;
            animX.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            DoubleAnimation animY = new DoubleAnimation();
            animY.From = 1;
            animY.To = 0;
            animY.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            Storyboard.SetTarget(animX, panel);
            Storyboard.SetTarget(animY, panel);
            Storyboard.SetTargetProperty(animX, new PropertyPath(ScaleTransform.ScaleXProperty));
            Storyboard.SetTargetProperty(animY, new PropertyPath(ScaleTransform.ScaleYProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animX);
            storyboard.Children.Add(animY);
            storyboard.Begin();
        }
        #endregion

        #region EventHandler
        private void OnPhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void InitAction()
        {
            mIsSelectedRole = false;
            Role.Visibility = Visibility.Collapsed;
            mPosPrevious.X = mPosPrevious.Y = mPosCurrent.X = mPosCurrent.Y = -1;
        }

        private void OnGameSpaceMouseUp(object sender, MouseButtonEventArgs e)
        {
            // 先依選中的格子來判斷走什麼流程
            if (mAnimationRunning || mShowGameOver)
            {
                return;
            }

            Position ptVisual = GetCoordinate(e.GetPosition(GameSpace)); // 回傳為視覺 left、top 座標
            if (ptVisual.X >= 0)
            {
                int nCurrentBlockType = mVirtualMap[mPosCurrent.Row, mPosCurrent.Column];
                if (nCurrentBlockType == 0)
                {
                    // 選中空格子，若上一步是選種子，即運算路徑
                    if (mIsSelectedRole)
                    {
                        MoveBean();
                        InitAction();
                    }
                    else
                    {
                        InitAction();
                    }
                }
                else
                {
                    // 選中的格子內有種子
                    Canvas.SetLeft(Role, ptVisual.X);
                    Canvas.SetTop(Role, ptVisual.Y);
                    Role.Visibility = Visibility.Visible;
                    mIsSelectedRole = true;
                }
            }
            else
            {
                InitAction();
            }
        }

        private void OnNewGameButtonClick(object sender, RoutedEventArgs e)
        {
            InitNewGame();
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        #endregion

        #region GameFunction
        private void AddBeansScore(int n)
        {
            mScore += n;
            ScorePanel.Text = mScore.ToString();
        }

        /// <summary>
        /// 把號碼轉為圖片路徑
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private BitmapImage GetBeanImagePath(int type)
        {
            String strPath = String.Format(Constants.BEAN_PATH_TEMPLATE, type);
            return new BitmapImage(new Uri(strPath, UriKind.Relative));
        }

        /// <summary>
        /// 用 Point 換算出在 Map 中的列欄座標
        /// </summary>
        /// <param name="pt">相對於 GameSpace 的點座標</param>
        /// <returns>座落區塊的 Left、Top</returns>
        private Position GetCoordinate(Point pt)
        {
            Boolean bFind = false;
            Position ptRes = new Position(-1, -1);

            mPosPrevious.Row = -1;
            mPosPrevious.Column = -1;

            const int FUZZY_BLOCK_SIZE = Constants.BLOCK_SIZE + 2;
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    // 讀出此列此欄的 Rect
                    int nLeft = (int)mVisualMap[row, col].X - 1;
                    int nTop = (int)mVisualMap[row, col].Y - 1;
                    int nRight = nLeft + FUZZY_BLOCK_SIZE;
                    int nBottom = nTop + FUZZY_BLOCK_SIZE;
                    // 是否在範圍內
                    if (pt.X >= nLeft && pt.Y >= nTop && pt.X <= nRight && pt.Y <= nBottom)
                    {
                        mPosPrevious.Row = mPosCurrent.Row;
                        mPosPrevious.Column = mPosCurrent.Column;
                        mPosCurrent.Row = row;
                        mPosCurrent.Column = col;
                        ptRes = mVisualMap[row, col];
                        bFind = true;
                        break;
                    }
                }
                if (bFind)
                {
                    break;
                }
            }

            return ptRes;
        }

        /// <summary>
        /// 亂數跑三顆種子待命
        /// </summary>
        private void CreateNextBeans()
        {
            for (int i = 0; i < 3; ++i)
            {
                mNextBeans[i] = mRandom.Next(mBeansMaxCount) + 1;
            }
            FirstNextImg.Source = GetBeanImagePath(mNextBeans[0]);
            SecondNextImg.Source = GetBeanImagePath(mNextBeans[1]);
            ThirdNextImg.Source = GetBeanImagePath(mNextBeans[2]);
        }

        private List<Position> GetSpaceBlockList()
        {
            List<Position> lstNullBlock = new List<Position>(); // 用來記錄空位置的索引

            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
                {
                    if (mVirtualMap[row, col] == 0)
                    {
                        lstNullBlock.Add(new Position(row, col)); // 把空的位置記下來
                    }
                }
            }

            return lstNullBlock;
        }

        /// <summary>
        /// 丟三個種子到地圖裡面
        /// </summary>
        /// <returns>成功否</returns>
        private Boolean PutNextBeans()
        {
            Boolean bCompleted = false;
            List<Position> lstNullBlock = GetSpaceBlockList(); // 用來記錄空位置的索引

            // 至少要剩 4 個空間，加進去就還剩 1 個可以移動
            if (lstNullBlock.Count > 3)
            {
                bCompleted = true;
                for (int nFind = 0; nFind < 3; ++nFind)
                {
                    int nNextIndex = mRandom.Next(lstNullBlock.Count);
                    Position ptNext = lstNullBlock[nNextIndex];
                    int nRow = (int)ptNext.Row;
                    int nCol = (int)ptNext.Column;
                    mVirtualMap[nRow, nCol] = mNextBeans[nFind]; // 在這個空格裡填入種子
                    AddToBeansMap(nRow, nCol, mNextBeans[nFind]);
                    lstNullBlock.RemoveAt(nNextIndex); // 從空格子列表中移除此顆

                    if (CheckChain(nRow, nCol))
                    {
                        // 把 mChain 數量記下來，後來想想根本沒必要，反正上面已經指定 bCompleted = true;
                        // 而且發生這種事只會增加空格絕不會導致結果變成 false
                        RemoveBeanChain();

                        // 當新加入的豆子造成地圖中出現串列，即不再加入新豆子
                        // 這樣就解決了在消豆子動畫時 mChain 被清空的問題了 :p
                        break;
                    }
                }

                CreateNextBeans();
            }

            return bCompleted;
        }

        /// <summary>
        /// 請確定該座標沒有種子再加
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bean"></param>
        private void AddToBeansMap(int r, int c, int bean)
        {
            mBeansMap[r, c].Source = GetBeanImagePath(bean);
        }

        private Boolean MoveBean()
        {
            Boolean bFindPath = false;
            int nSourceRow = mPosPrevious.Row;
            int nSourceCol = mPosPrevious.Column;
            int nTargetRow = mPosCurrent.Row;
            int nTargetColumn = mPosCurrent.Column;
            int nBeanType = mVirtualMap[nSourceRow, nSourceCol];
            if (nSourceCol != -1)
            {
                #region A* Algorithm
                AStarPathFinder finder = new AStarPathFinder();
                finder.ReadMap(mVirtualMap, nSourceRow, nSourceCol, nTargetRow, nTargetColumn);
                finder.Pathfind();
                finder.HighlightPath();
                mPath = finder.GetPath();
                if (mPath.Count > 0)
                {
                    bFindPath = true;
                    mVirtualMap[nSourceRow, nSourceCol] = 0;
                    mVirtualMap[nTargetRow, nTargetColumn] = nBeanType;
                    ShowVisitAnimation(nBeanType);
                }
                else
                {
                    ShowInvalidPath();
                }
                #endregion

                #region Recursive Visit
                //PathFinder finder = new PathFinder(mVirtualMap, nSourceRow, nSourceCol, nTargetRow, nTargetColumn);
                //bFindPath = finder.StartFind();
                //if (bFindPath)
                //{
                //    mPath = finder.GetPath();
                //    if (mPath.Count > 0)
                //    {
                //        mVirtualMap[nSourceRow, nSourceCol] = 0;
                //        mVirtualMap[nTargetRow, nTargetColumn] = nBeanType;
                //        ShowVisitAnimation(nBeanType);
                //    }
                //}
                #endregion
            }
            return bFindPath;
        }

        private void ShowVisitAnimation(int beanType)
        {
            Position posSource = mPath[0];
            int nSourceRow = (int)posSource.Row;
            int nSourceColumn = (int)posSource.Column;
            int nSourcePositionX = (int)mVisualMap[nSourceRow, nSourceColumn].X - Constants.BLOCK_RADIUS;
            int nSourcePositionY = (int)mVisualMap[nSourceRow, nSourceColumn].Y - Constants.BLOCK_RADIUS;

            PointAnimationUsingKeyFrames frames = new PointAnimationUsingKeyFrames();
            Double dblSeconePosition = 0.0;
            for (int i = 0; i < mPath.Count; ++i)
            {
                KeyTime kTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(dblSeconePosition));
                Position posNew = mVisualMap[mPath[i].Row, mPath[i].Column];
                Point ptNew = new Point(posNew.X - nSourcePositionX, posNew.Y - nSourcePositionY);
                frames.KeyFrames.Add(new LinearPointKeyFrame() { KeyTime = kTime, Value = ptNew });
                dblSeconePosition += Constants.ANIMATION_STEP_BLOCK;
            }
            Storyboard.SetTarget(frames, RoleBean);
            Storyboard.SetTargetProperty(frames, new PropertyPath(EllipseGeometry.CenterProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(frames);
            storyboard.Completed += OnStoryboardCompleted;

            // 把 EllipseGeometry 貼上正確的圖片並秀出來、移到正確的位置
            RoleBeanElement.Fill = new ImageBrush() { ImageSource = GetBeanImagePath(beanType) };
            Canvas.SetLeft(RoleBeanElement, mVisualMap[nSourceRow, nSourceColumn].X);
            Canvas.SetTop(RoleBeanElement, mVisualMap[nSourceRow, nSourceColumn].Y);
            // 把原本在底下的圖片清掉
            mBeansMap[nSourceRow, nSourceColumn].Source = null;

            // 開始跑
            mAnimationRunning = true;
            storyboard.Begin();
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            Position ptTarget = mPath[mPath.Count - 1];
            int nTargetRow = (int)ptTarget.Row;
            int nTargetColumn = (int)ptTarget.Column;
            Debug.WriteLine("MoveCompleted :: " + nTargetRow + ", " + nTargetColumn);
            // 把 EllipseGeometry 的圖片清掉
            RoleBeanElement.Fill = null;

            // 把目的地的圖貼上去
            mBeansMap[nTargetRow, nTargetColumn].Source = GetBeanImagePath(mVirtualMap[nTargetRow, nTargetColumn]);

            // 檢查是不是拼到五顆了
            if (CheckChain(nTargetRow, nTargetColumn))
            {
                // 有消掉一條則此次動作不加入豆子
                RemoveBeanChain();
                mAnimationRunning = false;
            }
            else
            {
                if (!PutNextBeans())
                {
                    StoreScore();
                    ShowGameOver();
                }
                mAnimationRunning = false;
            }
        }

        private void RemoveBeanChain()
        {
            Debug.WriteLine("RemoveBeanChain Start :: " + mChain.Count);
            for (int i = 0; i < mChain.Count; ++i)
            {
                Position pos = mChain[i];
                mVirtualMap[pos.Row, pos.Column] = 0;
                #region Animation
                ScaleTransform panel = mBeansMap[pos.Row, pos.Column].RenderTransform as ScaleTransform;
                DoubleAnimation animX = new DoubleAnimation();
                animX.From = 1;
                animX.To = 0;
                animX.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                DoubleAnimation animY = new DoubleAnimation();
                animY.From = 1;
                animY.To = 0;
                animY.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                Storyboard.SetTarget(animX, panel);
                Storyboard.SetTarget(animY, panel);
                Storyboard.SetTargetProperty(animX, new PropertyPath(ScaleTransform.ScaleXProperty));
                Storyboard.SetTargetProperty(animY, new PropertyPath(ScaleTransform.ScaleYProperty));
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animX);
                storyboard.Children.Add(animY);
                if (i >= (mChain.Count - 1))
                {
                    // 最後一個動畫結束後要把畫面清空
                    storyboard.Completed += OnRemoveBeanStoryboardCompleted;
                }
                storyboard.Begin();
                #endregion
            }
        }

        void OnRemoveBeanStoryboardCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("RemoveCompleted :: " + mChain.Count);
            foreach (Position pos in mChain)
            {
                mBeansMap[pos.Row, pos.Column].Source = null;
                ScaleTransform panel = mBeansMap[pos.Row, pos.Column].RenderTransform as ScaleTransform;
                panel.ScaleX = 1;
                panel.ScaleY = 1;
            }
            AddBeansScore(mChain.Count);
            mChain.Clear();
        }

        private Boolean CheckChain(int r, int c)
        {
            Boolean bFindChain = false;
            Position posTarget = new Position(r, c);
            int TargetType = mVirtualMap[r, c];
            Debug.WriteLine("CheckChain");
            mChain.Clear();
            int preType = 0;

            #region 檢查橫向
            preType = 0;
            List<Position> listColumn = new List<Position>();
            for (int col = 0; col < Constants.BLOCK_COLUMN_COUNT; ++col)
            {
                Boolean toCheck = false;
                if (mVirtualMap[r, col] == TargetType)
                {
                    if (listColumn.Count < 1 || preType == TargetType)
                    {
                        // 出現第一個或連續
                        listColumn.Add(new Position(r, col));
                    }

                    // 避免最後一格是正確的豆子
                    if (col >= (Constants.BLOCK_COLUMN_COUNT - 1))
                    {
                        toCheck = true;
                    }
                }
                else
                {
                    toCheck = true;
                }

                if (toCheck)
                {
                    if (listColumn.Count >= 5)
                    {
                        // 把 list 加到總表裡
                        AddToChain(listColumn, posTarget);
                    }
                    listColumn.Clear();
                }

                preType = mVirtualMap[r, col];
            }
            #endregion

            #region 檢查縱向
            preType = 0;
            List<Position> listRow = new List<Position>();
            for (int row = 0; row < Constants.BLOCK_ROW_COUNT; ++row)
            {
                Boolean toCheck = false;
                if (mVirtualMap[row, c] == TargetType)
                {
                    if (listRow.Count < 1 || preType == TargetType)
                    {
                        // 出現第一個或連續
                        listRow.Add(new Position(row, c));
                    }

                    // 避免最後一格是正確的豆子
                    if (row >= (Constants.BLOCK_ROW_COUNT - 1))
                    {
                        toCheck = true;
                    }
                }
                else
                {
                    toCheck = true;
                }

                if (toCheck)
                {
                    if (listRow.Count >= 5)
                    {
                        // 把 list 加到總表裡
                        AddToChain(listRow, posTarget);
                    }
                    listRow.Clear();
                }

                preType = mVirtualMap[row, c];
            }
            #endregion

            #region 檢查斜邊
            List<Position> listVariable = new List<Position>();
            for (int i = -(Constants.BLOCK_ROW_COUNT); i < Constants.BLOCK_ROW_COUNT; ++i)
            {
                // 往右找
                if ((r + i) < Constants.BLOCK_ROW_COUNT && (c + i) < Constants.BLOCK_COLUMN_COUNT && (r + i) >= 0 && (c + i) >= 0)
                {
                    // 加入
                    listVariable.Add(new Position(r + i, c + i));
                }
            }

            preType = 0;
            List<Position> list45 = new List<Position>();
            for(int i = 0; i < listVariable.Count; ++i)
            {
                Position pos = listVariable[i];
                Boolean toCheck = false;
                if (mVirtualMap[pos.Row, pos.Column] == TargetType)
                {
                    if (list45.Count < 1 || preType == TargetType)
                    {
                        // 出現第一個或連續
                        list45.Add(new Position(pos.Row, pos.Column));
                    }

                    // 避免最後一格是正確的豆子
                    if (i >= (listVariable.Count - 1))
                    {
                        toCheck = true;
                    }
                }
                else
                {
                    toCheck = true;
                }

                if (toCheck)
                {
                    if (list45.Count >= 5)
                    {
                        // 把 list 加到總表裡
                        AddToChain(list45, posTarget);
                    }
                    list45.Clear();
                }

                preType = mVirtualMap[pos.Row, pos.Column];
            }
            #endregion

            #region 檢查反斜邊
            listVariable.Clear();
            for (int i = -(Constants.BLOCK_ROW_COUNT); i < Constants.BLOCK_ROW_COUNT; ++i)
            {
                // 往右找
                if ((r + i) < Constants.BLOCK_ROW_COUNT && (c - i) < Constants.BLOCK_COLUMN_COUNT && (r + i) >= 0 && (c - i) >= 0)
                {
                    // 加入
                    listVariable.Add(new Position(r + i, c - i));
                }
            }

            preType = 0;
            list45.Clear();
            for (int i = 0; i < listVariable.Count; ++i)
            {
                Position pos = listVariable[i];
                Boolean toCheck = false;
                if (mVirtualMap[pos.Row, pos.Column] == TargetType)
                {
                    if (list45.Count < 1 || preType == TargetType)
                    {
                        // 出現第一個或連續
                        list45.Add(new Position(pos.Row, pos.Column));
                    }

                    // 避免最後一格是正確的豆子
                    if (i >= (listVariable.Count - 1))
                    {
                        toCheck = true;
                    }
                }
                else
                {
                    toCheck = true;
                }

                if (toCheck)
                {
                    if (list45.Count >= 5)
                    {
                        // 把 list 加到總表裡
                        AddToChain(list45, posTarget);
                    }
                    list45.Clear();
                }

                preType = mVirtualMap[pos.Row, pos.Column];
            }
            #endregion

            if (mChain.Count > 0)
            {
                bFindChain = true;
                
                // 加入目標點
                mChain.Add(posTarget);
            }
            
            return bFindChain;
        }

        private void AddToChain(List<Position> list, Position skipPos)
        {
            foreach (Position pos in list)
            {
                if (!(pos.Row == skipPos.Row && pos.Column == skipPos.Column))
                {
                    mChain.Add(new Position(pos.Row, pos.Column));
                }
            }
        }

        private void TraceMap()
        {
            String strDebug = "";
            for (int y = 0; y < Constants.BLOCK_ROW_COUNT; ++y)
            {
                strDebug = String.Empty;
                for (int x = 0; x < Constants.BLOCK_COLUMN_COUNT; ++x)
                {
                    //strDebug += String.Format("{0:d2}  ", mVirtualMap[x, y]);
                    strDebug += String.Format("{0},", mVirtualMap[x, y]);
                }
                Debug.WriteLine(strDebug);
            }
        }
        #endregion
    }
}