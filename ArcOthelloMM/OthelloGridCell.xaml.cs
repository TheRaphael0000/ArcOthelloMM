﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcOthelloMM
{
    /// <summary>
    /// Interaction logic for OthelloGridCell.xaml
    /// </summary>
    public partial class OthelloGridCell : UserControl
    {
        public enum States
        {
            Empty,
            Player1,
            Player2,
            PreviewPlayer1,
            PreviewPlayer2
        }

        private States _state;

        /// <summary>
        /// Set the new state of the cell, make a color transition
        /// </summary>
        public States State
        {
            get { return _state; }
            set
            {
                Color previousColor = colors[_state].Color;
                
                _state = value;
                
                //animation
                ColorAnimation ca = new ColorAnimation(colors[_state].Color, new Duration(TimeSpan.FromSeconds(0.5))); //to this
                tokenContent.Fill = new SolidColorBrush(previousColor); // from previous color
                tokenContent.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                tokenContent.Width = sizes[_state];
                tokenContent.Height = sizes[_state];
            }
        }

        public int X { get; }
        public int Y { get; }

        private bool _lastPlay;

        /// <summary>
        /// Is this cells the last one played (show a circler border)
        /// </summary>
        public bool LastPlay
        {
            get
            {
                return _lastPlay;
            }
            set
            {
                _lastPlay = value;
                tokenContent.Stroke = _lastPlay ? borderColorEllipseLastPlay : transparent;
            }
        }

        public event EventHandler Click;

        static Dictionary<States, SolidColorBrush> colors;
        static Dictionary<States, int> sizes;

        static Brush borderColorEllipseLastPlay= BrushFromColor(Color.FromRgb(84, 252, 89), 255);
        static Brush transparent = BrushFromColor(Color.FromRgb(0, 0, 0), 0);

        static Brush backgroundColor = BrushFromColor(Color.FromRgb(0, 0, 0), 160);
        static Brush borderColor = BrushFromColor(Color.FromRgb(255, 255, 255), 160);

        static int normalSize = 100;
        static byte normalOpacity = 200;

        static int previewSize = (int)(0.60*normalSize);
        static byte previewOpacity = (byte)(0.50 * normalOpacity);


        static OthelloGridCell()
        {
            colors = new Dictionary<States, SolidColorBrush>();
            colors.Add(States.Empty, BrushFromColor(Color.FromRgb(0,0,0), 0));
            colors.Add(States.Player1, BrushFromColor(Player.Player1.Color, normalOpacity));
            colors.Add(States.Player2, BrushFromColor(Player.Player0.Color, normalOpacity));
            colors.Add(States.PreviewPlayer1, BrushFromColor(Player.Player1.Color, previewOpacity));
            colors.Add(States.PreviewPlayer2, BrushFromColor(Player.Player0.Color, previewOpacity));
            sizes = new Dictionary<States, int>();
            sizes.Add(States.Empty, 0);
            sizes.Add(States.Player1, normalSize);
            sizes.Add(States.Player2, normalSize);
            sizes.Add(States.PreviewPlayer1, previewSize);
            sizes.Add(States.PreviewPlayer2, previewSize);
        }

        static SolidColorBrush BrushFromColor(Color color, byte alpha)
        {
            return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        }

        /// <summary>
        /// Create new cell for the othello game
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="states"></param>
        public OthelloGridCell(int x, int y, States states = States.Empty)
        {
            InitializeComponent();
            this.State = states;
            this.X = x;
            this.Y = y;
            this.LastPlay = false;
            button.Background = backgroundColor;
            button.BorderBrush = borderColor;
            button.Click += ButtonClick; // Adding the click on the parent of the button
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e); // trigger the parent click event on the button (children) event trigger
        }
    }
}
