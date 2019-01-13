﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArcOthelloMM
{
    /// <summary>
    /// Interaction logic for OthelloBoard.xaml
    /// </summary>
    public partial class OthelloBoard : Window
    {
        private const int NB_COL = 9;
        private const int NB_ROW = 7;
        private const string LABEL_COL = "ABCDEFGHI";
        private const string LABEL_ROW = "1234567";

        private OthelloGridCell[,] othelloGridCells;

        private bool turnWhite;
        private Tuple<int, int> lastPlay;
        private Dictionary<Tuple<int, int>, HashSet<Tuple<int, int>>> currentPossibleMoves;


        Timer timerUpdateGui;

        Stopwatch swPlayer1;
        Stopwatch swPlayer2;
        const long totalTimeMililseconds = 5 * 60 * 1000; //5min

        public OthelloBoard()
        {
            InitializeComponent();
            NewGame();
        }

        private void NewGame()
        {
            swPlayer1 = new Stopwatch();
            swPlayer2 = new Stopwatch();
            timerUpdateGui = new Timer();
            timerUpdateGui.Interval = 0.1;
            timerUpdateGui.Elapsed += TimerUpdateGui_Elapsed;
            timerUpdateGui.Start();
            othelloGridCells = new OthelloGridCell[NB_COL, NB_ROW];

            turnWhite = false; //black start
            lastPlay = null;
            LogicalBoard.GetInstance().ResetGame();
            currentPossibleMoves = LogicalBoard.GetInstance().GetListPossibleMove(turnWhite);

            GenerateGrid();
            UpdateGui();
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void TimerUpdateGui_Elapsed(object sender, ElapsedEventArgs e)
        {
            //this.Dispatcher.Invoke(() =>
            //{
                //UpdateTimers(); // je sais que ça crash là à la fermeture j'investigue
            //});
        }

        private void GenerateGrid()
        {
            //add row col labels
            for (int i = 0; i < NB_COL; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                graphicalBoard.ColumnDefinitions.Add(col);
            }

            for (int i = 0; i < NB_ROW; i++)
            {
                RowDefinition row = new RowDefinition();
                graphicalBoard.RowDefinitions.Add(row);
            }

            for (int x = 0; x < othelloGridCells.GetLength(0); x++)
            {
                for (int y = 0; y < othelloGridCells.GetLength(1); y++)
                {
                    OthelloGridCell cell = new OthelloGridCell(x, y);

                    cell.Click += Cell_Click;

                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);

                    othelloGridCells[x, y] = cell;
                    graphicalBoard.Children.Add(cell);
                }
            }
        }

        private void UpdateBoard()
        {
            currentPossibleMoves = LogicalBoard.GetInstance().GetListPossibleMove(turnWhite);
            int[,] lboard = LogicalBoard.GetInstance().GetBoard();
            for (int x = 0; x < othelloGridCells.GetLength(0); x++)
            {
                for (int y = 0; y < othelloGridCells.GetLength(1); y++)
                {
                    Tuple<int, int> pos = new Tuple<int, int>(x, y);
                    OthelloGridCell gcell = othelloGridCells[x, y];
                    
                    int lcell = lboard[x,y];
                    if (lcell == 0)
                        gcell.State = OthelloGridCell.States.Player1;
                    else if (lcell == 1)
                        gcell.State = OthelloGridCell.States.Player2;
                    else if(lcell == -1 && currentPossibleMoves.ContainsKey(pos))
                    {
                        if (turnWhite)
                            gcell.State = OthelloGridCell.States.PreviewPlayer1;
                        else
                            gcell.State = OthelloGridCell.States.PreviewPlayer2;
                    }
                    else
                        gcell.State = OthelloGridCell.States.Empty;

                    if (lastPlay != null)
                        gcell.LastPlay = pos.Item1 == lastPlay.Item1 && pos.Item2 == lastPlay.Item2;
                }
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            OthelloGridCell s = (OthelloGridCell)sender;
            int x = s.X;
            int y = s.Y;
            Tuple<int, int> pos = new Tuple<int, int>(x, y);
            if (currentPossibleMoves.ContainsKey(pos))
            {
                LogicalBoard.GetInstance().PlayMove(x, y, turnWhite);
                lastPlay = pos;
                NextTurn();
                UpdateGui();
            }
        }

        private void UpdateGui()
        {
            UpdateBoard();
            UpdateGameData();
        }

        private void UpdateGameData()
        {
            UpdateTimers();
            lblTurn.Content = turnWhite ? "blanc" : "noir";
        }

        private void UpdateTimers()
        {
            UpdateTimer(swPlayer1, lblTimeBlack);
            UpdateTimer(swPlayer2, lblTimeWhite);
        }

        private void UpdateTimer(Stopwatch sw, Label lbl)
        {
            TimeSpan remainingTime = TimeSpan.FromMilliseconds(totalTimeMililseconds - sw.ElapsedMilliseconds);
            lbl.Content = remainingTime.Hours + ":" + remainingTime.Minutes + ":" + remainingTime.Seconds + "." + remainingTime.Milliseconds;
        }

        private void NextTurn()
        {
            Console.WriteLine("------");
            foreach (Tuple<int, int> b in currentPossibleMoves.Keys)
            {
                Console.WriteLine(b);
            }

            turnWhite = !turnWhite;
            currentPossibleMoves = LogicalBoard.GetInstance().GetListPossibleMove(turnWhite);

            if (currentPossibleMoves.Count <= 0)
            {
                turnWhite = !turnWhite;
                currentPossibleMoves = LogicalBoard.GetInstance().GetListPossibleMove(turnWhite);
                if (currentPossibleMoves.Count <= 0)
                {
                    Console.WriteLine("Game End :" + currentPossibleMoves.Count);
                }
                else
                {
                    Console.WriteLine("Turn skiped :" + currentPossibleMoves.Count);
                }
            }
            else
            {
                Console.WriteLine("Next turn :" + currentPossibleMoves.Count);
            }

            foreach(Tuple<int, int>  b in currentPossibleMoves.Keys)
            {
                Console.WriteLine(b);
            }

            if (turnWhite)
            {
                swPlayer1.Start();
                swPlayer2.Stop();
            }
            else
            {
                swPlayer1.Stop();
                swPlayer2.Start();
            }
        }

        private void Board_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double min = Math.Min(border.ActualHeight / NB_ROW, border.ActualWidth / NB_COL);

            for (int i = 0; i < NB_COL; i++)
                graphicalBoard.ColumnDefinitions[i].Width = new GridLength(min, GridUnitType.Pixel);

            for (int i = 0; i < NB_ROW; i++)
                graphicalBoard.RowDefinitions[i].Height = new GridLength(min, GridUnitType.Pixel);

            double w = (border.ActualWidth - min * NB_COL) / 2;
            double h = (border.ActualHeight - min * NB_ROW) / 2;
            
            graphicalBoard.Margin = new Thickness(w, h, w, h);
        }
    }
}
