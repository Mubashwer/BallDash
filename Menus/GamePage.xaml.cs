﻿// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using SharpDX;
using System;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace Project.Menus {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // TASK 4: Instructions Page
    public sealed partial class GamePage {
        private MainPage parent;

        public GamePage(MainPage parent) {
            this.parent = parent;
            InitializeComponent();
            parent.Game.AllLevelsComplete += Game_AllLevelsComplete;
        }

        public void UpdateDebugStats(string stats) {
            txtDebugStats.Text = stats;
        }

        public void UpdateStats(LevelStatus status) {
            // update score text
            txtCurrentTime.Text = "Time:" + status.Time.TotalSeconds.ToString();
            txtBestTime.Text = "Best:" + (status.BestTime.HasValue ? status.BestTime.Value.TotalSeconds.ToString() : "None");

            txtHintUsed.Text = "Hint used:" + (status.HintUsed ? "Yes - High Score Forfeit!" : "No");
            txtCurrentCollisions.Text = "Collisions:" + status.Collisions;
            txtBestCollisions.Text = "Best:" + (status.BestCollisions.HasValue ? status.BestCollisions.Value.ToString() : "None");

            // set colours based on score values
            if (status.BestTime.HasValue) {
                if ((status.Time < status.BestTime)) {
                    txtCurrentTime.Foreground = new SolidColorBrush(Colors.Green);
                }
                else {
                    txtCurrentTime.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            else {
                txtCurrentTime.Foreground = new SolidColorBrush(Colors.White);
            }

            if (status.BestCollisions.HasValue) {
                if (status.Collisions < status.BestCollisions.Value) {
                    txtCurrentCollisions.Foreground = new SolidColorBrush(Colors.Green);
                }
                else {
                    txtCurrentCollisions.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            else {
                txtBestCollisions.Foreground = new SolidColorBrush(Colors.White);
            }

            if (status.HintUsed) {
                txtCurrentTime.Foreground = new SolidColorBrush(Colors.Red);
                txtHintUsed.Foreground = new SolidColorBrush(Colors.Red);
                txtCurrentCollisions.Foreground = new SolidColorBrush(Colors.Red);
            }
            else {
                txtHintUsed.Foreground = new SolidColorBrush(Colors.Green);
            }

        }

        // Button for Hint
        private void ChangeHint(object sender, RoutedEventArgs e) {
            parent.Game.MazeSolver.Enabled = !parent.Game.MazeSolver.Enabled;
        }

        private void Back(object sender, RoutedEventArgs e) {
            StopGame();
        }

        private void Game_AllLevelsComplete(object sender, EventArgs e) {
            StopGame();
        }

        private void StopGame() {
            parent.Game.StopGame();

            parent.Children.Add(new MainMenu(parent));
            parent.Children.Remove(this);
            parent.Game.AllLevelsComplete -= Game_AllLevelsComplete;
        }
    }
}
